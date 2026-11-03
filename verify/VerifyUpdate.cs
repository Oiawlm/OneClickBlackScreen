using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

// Offline updater verifier (B8): exercises AppInfo's version comparison table,
// then drives the whole Updater chain against a local HttpListener mock of
// releases/latest - JSON payload, release asset, digest, SHA256SUMS fallback,
// 302 redirect - and rehearses the replace sequence on copies inside a temp
// directory. The real dist exe is never touched, and no packet leaves the loop
// back interface; the live GitHub endpoint stays an I-wave concern (I4).
internal static class VerifyUpdate
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void ExpectFailure(Action action, string what, string messagePart)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (messagePart != null)
                Check(ex.Message.IndexOf(messagePart, StringComparison.Ordinal) >= 0,
                    what + " failed with an unexpected message: " + ex.Message);
            return;
        }
        throw new Exception(what + " did not fail");
    }

    private static int Main(string[] args)
    {
        try
        {
            Check(args.Length == 0, "Usage: VerifyUpdate.exe");
            // Mock traffic must stay off any system proxy - the mock is on the
            // loopback interface and the requests are all ours.
            WebRequest.DefaultWebProxy = null;

            CheckVersionTable();
            CheckFullChain();

            Console.WriteLine("PASS TOTAL: update checks (version table + offline full chain)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }

    // 1. Version comparison (B2): newer per segment, equal, older, and every
    // malformed shape reading fail-closed.
    private static void CheckVersionTable()
    {
        int[] current = AppInfo.CurrentVersion();
        Check(current[0] == 1 && current[1] == 0 && current[2] == 1,
            "AppInfo.CurrentVersion does not read the declared 1.0.1");
        Check(AppInfo.IsNewer("v1.0.2", current), "v1.0.2 must read newer");
        Check(AppInfo.IsNewer("v1.1.0", current), "v1.1.0 must read newer");
        Check(AppInfo.IsNewer("v2.0.0", current), "v2.0.0 must read newer");
        Check(AppInfo.IsNewer("v10.0.0", current), "v10.0.0 must read newer (numeric order, not string order)");
        Check(!AppInfo.IsNewer("v0.9.9", current), "v0.9.9 must read older");
        Check(!AppInfo.IsNewer("v1.0.0", current), "v1.0.0 must read older, not newer");
        Check(!AppInfo.IsNewer("v1.0.1", current), "v1.0.1 must read equal, not newer");
        Check(!AppInfo.IsNewer("1.0.1", current), "bare 1.0.1 must read equal, not newer");
        Check(!AppInfo.IsNewer("v1.0", current), "two segments must fail closed");
        Check(!AppInfo.IsNewer("v1.0.0.1", current), "four segments must fail closed");
        Check(!AppInfo.IsNewer("v1.0.0-beta", current), "prerelease suffix must fail closed");
        Check(!AppInfo.IsNewer("v1.0.x", current), "non-numeric segment must fail closed");
        Check(!AppInfo.IsNewer("latest", current), "word tag must fail closed");
        Check(!AppInfo.IsNewer("", current), "empty tag must fail closed");
        Check(!AppInfo.IsNewer(null, current), "null tag must fail closed");
        Check(!AppInfo.IsNewer("v9999999999.0.0", current), "overflowing segment must fail closed");
        Check(!AppInfo.IsNewer("v 1.0.1", current), "inner whitespace tag must fail closed");
        int[] parsed;
        Check(AppInfo.TryParseVersion("v1.2.3", out parsed)
            && parsed[0] == 1 && parsed[1] == 2 && parsed[2] == 3,
            "TryParseVersion must accept v-prefixed tags");
        Check(AppInfo.TryParseVersion("0.20.300", out parsed)
            && parsed[0] == 0 && parsed[1] == 20 && parsed[2] == 300,
            "TryParseVersion must accept zero-padded bare versions");
        Check(!AppInfo.TryParseVersion("v.1.2", out parsed), "empty segment must fail closed");
        Check(!AppInfo.TryParseVersion("V1.0.1", out parsed), "uppercase V prefix must fail closed (our tags are lowercase v)");
        Console.WriteLine("PASS: version comparison table (newer/equal/older/malformed, fail-closed)");
    }

    // 2. Full chain against the local mock (B3-B6): check -> 302 download ->
    // digest verify -> replace, plus every rejection and the rollback path.
    private static void CheckFullChain()
    {
        string work = Path.Combine(Path.GetTempPath(), "VerifyUpdate-" + Path.GetRandomFileName());
        Directory.CreateDirectory(work);
        try
        {
            using (MockServer server = MockServer.Start())
            {
                byte[] payload = BuildPayload();
                string payloadSha = Sha256Hex(payload);
                byte[] oldBytes = Encoding.UTF8.GetBytes("PREVIOUS-EXE-CONTENT");

                // -- happy path: digest + 302 redirect + replace with rollback file.
                server.LatestJson = BuildLatestJson("v9.9.9", "sha256:" + payloadSha,
                    payload.Length, server.Base + "/redirect", server.Base + "/sums");
                server.AssetBytes = payload;
                server.SumsBody = payloadSha + "  " + AppInfo.UpdateAssetName + "\n";
                Updater.UpdateInfo info = Updater.Check(server.Base);
                Check(info != null, "Check must report the newer release");
                Check(info.Tag == "v9.9.9", "tag mismatch: " + info.Tag);
                Check(info.Size == payload.Length, "size mismatch");
                Check(info.Sha256Hex == payloadSha, "digest mismatch");
                Check(server.LastUserAgent != null
                    && server.LastUserAgent.IndexOf("OneClickBlackScreen-Updater/" + AppInfo.Version, StringComparison.Ordinal) >= 0,
                    "requests must carry the custom User-Agent, saw: " + server.LastUserAgent);
                string target = Path.Combine(work, "app.exe");
                File.WriteAllBytes(target, oldBytes);
                List<int> progress = new List<int>();
                Updater.DownloadAndApply(info, target, delegate(int percent) { progress.Add(percent); });
                Check(BytesEqual(File.ReadAllBytes(target), payload),
                    "target must hold the downloaded payload after the swap");
                Check(BytesEqual(File.ReadAllBytes(target + ".old"), oldBytes),
                    "the .old backup must hold the previous exe");
                Check(progress.Count >= 2 && progress[progress.Count - 1] == 100,
                    "progress must stream during the download and finish at 100");
                for (int i = 1; i < progress.Count; i++)
                    Check(progress[i] >= progress[i - 1], "progress must be monotonic");
                Console.WriteLine("PASS: full chain (check -> 302 download -> digest verify -> replace) on a temp copy");

                // -- tampered digest: refuse, target untouched, no backup left.
                string wrongSha = Sha256Hex(Encoding.UTF8.GetBytes("not the payload"));
                server.LatestJson = BuildLatestJson("v9.9.9", "sha256:" + wrongSha,
                    payload.Length, server.Base + "/asset", null);
                Updater.UpdateInfo tamperedInfo = Updater.Check(server.Base);
                Check(tamperedInfo != null && tamperedInfo.Sha256Hex == wrongSha,
                    "the tampered release must still resolve");
                string tampered = Path.Combine(work, "tampered.exe");
                File.WriteAllBytes(tampered, oldBytes);
                ExpectFailure(delegate { Updater.DownloadAndApply(tamperedInfo, tampered, null); },
                    "a tampered digest", "SHA-256");
                Check(BytesEqual(File.ReadAllBytes(tampered), oldBytes), "tampered run must leave the target unchanged");
                Check(!File.Exists(tampered + ".old"), "tampered run must not leave a backup");
                Console.WriteLine("PASS: tampered digest refused, target unchanged");

                // -- digest absent, SHA256SUMS fallback carries the chain (B5).
                server.LatestJson = BuildLatestJson("v9.9.9", null,
                    payload.Length, server.Base + "/asset", server.Base + "/sums");
                Updater.UpdateInfo sumsInfo = Updater.Check(server.Base);
                Check(sumsInfo != null && sumsInfo.Sha256Hex == null && sumsInfo.SumsUrl != null,
                    "the digest-less release must expose the SUMS fallback");
                string sumsRun = Path.Combine(work, "sums.exe");
                File.WriteAllBytes(sumsRun, oldBytes);
                Updater.DownloadAndApply(sumsInfo, sumsRun, null);
                Check(BytesEqual(File.ReadAllBytes(sumsRun), payload),
                    "SHA256SUMS fallback must complete the swap");
                Console.WriteLine("PASS: SHA256SUMS fallback completed the chain");

                // -- double missing (no digest, no SUMS asset): refuse (B5).
                server.LatestJson = BuildLatestJson("v9.9.9", null,
                    payload.Length, server.Base + "/asset", null);
                Updater.UpdateInfo noHashInfo = Updater.Check(server.Base);
                Check(noHashInfo != null && noHashInfo.SumsUrl == null,
                    "the hash-less release must resolve before the refusal");
                string noHash = Path.Combine(work, "nohash.exe");
                File.WriteAllBytes(noHash, oldBytes);
                ExpectFailure(delegate { Updater.DownloadAndApply(noHashInfo, noHash, null); },
                    "a digest-less, SUMS-less release", "校验和");
                Check(BytesEqual(File.ReadAllBytes(noHash), oldBytes), "hash-less run must leave the target unchanged");
                // -- SUMS asset present but no matching line: same refusal.
                server.LatestJson = BuildLatestJson("v9.9.9", null,
                    payload.Length, server.Base + "/asset", server.Base + "/sums");
                server.SumsBody = Sha256Hex(oldBytes) + "  some-other-file.zip\n";
                Updater.UpdateInfo wrongSumsInfo = Updater.Check(server.Base);
                Check(wrongSumsInfo != null, "the wrong-SUMS release must resolve before the refusal");
                ExpectFailure(delegate { Updater.DownloadAndApply(wrongSumsInfo, noHash, null); },
                    "a SUMS file without the update asset line", "校验和");
                Console.WriteLine("PASS: digest and SHA256SUMS both missing/mismatched refused");

                // -- size disagreement: refuse.
                server.LatestJson = BuildLatestJson("v9.9.9", "sha256:" + payloadSha,
                    payload.Length + 1, server.Base + "/asset", null);
                Updater.UpdateInfo shortInfo = Updater.Check(server.Base);
                Check(shortInfo != null, "the short-size release must resolve before the refusal");
                ExpectFailure(delegate { Updater.DownloadAndApply(shortInfo, noHash, null); },
                    "a declared size that disagrees with the bytes", "下载不完整");
                Check(BytesEqual(File.ReadAllBytes(noHash), oldBytes), "short-size run must leave the target unchanged");
                Console.WriteLine("PASS: size mismatch refused");

                // -- 404 and malformed payloads fail closed (B8).
                server.LatestJson = null;
                try
                {
                    Updater.Check(server.Base);
                    throw new Exception("a 404 latest endpoint did not fail");
                }
                catch (WebException) { }
                server.LatestJson = "<html>not json at all";
                ExpectFailure(delegate { Updater.Check(server.Base); }, "malformed JSON", "JSON");
                server.LatestJson = "{\"tag_name\": \"v9.9.9\", \"assets\": []}";
                ExpectFailure(delegate { Updater.Check(server.Base); }, "a release without the asset", "更新资产");
                Console.WriteLine("PASS: 404 / malformed JSON / missing asset fail closed");

                // -- not newer / malformed tags read as no update (B2).
                server.LatestJson = BuildLatestJson("v1.0.1", "sha256:" + payloadSha,
                    payload.Length, server.Base + "/asset", null);
                Check(Updater.Check(server.Base) == null, "an equal tag must read as no update");
                server.LatestJson = BuildLatestJson("v1.0.1-rc1", "sha256:" + payloadSha,
                    payload.Length, server.Base + "/asset", null);
                Check(Updater.Check(server.Base) == null, "a malformed tag must read as no update");
                Console.WriteLine("PASS: equal and malformed tags read as no update");

                // -- replace sequence details on real files (B6).
                string keep = Path.Combine(work, "keep.exe");
                File.WriteAllBytes(keep, oldBytes);
                File.WriteAllBytes(keep + ".old", Encoding.UTF8.GetBytes("STALE"));
                string fresh = Path.Combine(work, "fresh.bin");
                File.WriteAllBytes(fresh, payload);
                Updater.ReplaceSequence(fresh, keep);
                Check(BytesEqual(File.ReadAllBytes(keep), payload), "ReplaceSequence must install the new bytes");
                Check(BytesEqual(File.ReadAllBytes(keep + ".old"), oldBytes),
                    "a stale .old must be cleared and replaced by the real backup");
                // -- rollback: a missing source fails after the rename, restore runs.
                string rollback = Path.Combine(work, "rollback.exe");
                File.WriteAllBytes(rollback, oldBytes);
                ExpectFailure(delegate { Updater.ReplaceSequence(Path.Combine(work, "missing.bin"), rollback); },
                    "a replace whose copy step fails", "已还原");
                Check(BytesEqual(File.ReadAllBytes(rollback), oldBytes), "rollback must restore the original bytes");
                Check(!File.Exists(rollback + ".old"), "rollback must consume the backup");
                // -- a locked .old blocks before the self-move, target untouched.
                string locked = Path.Combine(work, "locked.exe");
                File.WriteAllBytes(locked, oldBytes);
                string lockedOld = locked + ".old";
                File.WriteAllBytes(lockedOld, Encoding.UTF8.GetBytes("HELD"));
                File.SetAttributes(lockedOld, File.GetAttributes(lockedOld) | FileAttributes.ReadOnly);
                try
                {
                    ExpectFailure(delegate { Updater.ReplaceSequence(fresh, locked); },
                        "a replace blocked by a read-only .old", null);
                }
                finally
                {
                    File.SetAttributes(lockedOld, FileAttributes.Normal);
                }
                Check(BytesEqual(File.ReadAllBytes(locked), oldBytes), "blocked replace must leave the target unchanged");
                Console.WriteLine("PASS: replace sequence (stale .old cleared, rollback restored, blocked pre-clean)");
            }
        }
        finally
        {
            try { Directory.Delete(work, true); } catch { }
        }
    }

    private static byte[] BuildPayload()
    {
        // ~300 KB so the 64 KB read loop reports several progress steps.
        byte[] payload = new byte[300 * 1024];
        for (int i = 0; i < payload.Length; i++)
            payload[i] = (byte)(i * 31 + (i >> 8));
        return payload;
    }

    private static string Sha256Hex(byte[] bytes)
    {
        using (SHA256 sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }

    private static bool BytesEqual(byte[] a, byte[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

    private static string BuildLatestJson(string tag, string digest, long size, string assetUrl, string sumsUrl)
    {
        StringBuilder json = new StringBuilder();
        json.Append("{\"tag_name\": \"").Append(tag).Append("\", \"prerelease\": false, \"assets\": [");
        json.Append("{\"name\": \"").Append(AppInfo.UpdateAssetName)
            .Append("\", \"browser_download_url\": \"").Append(assetUrl)
            .Append("\", \"size\": ").Append(size)
            .Append(", \"digest\": ").Append(digest == null ? "null" : "\"" + digest + "\"")
            .Append("}");
        if (sumsUrl != null)
            json.Append(", {\"name\": \"SHA256SUMS\", \"browser_download_url\": \"")
                .Append(sumsUrl).Append("\", \"size\": 96, \"digest\": null}");
        json.Append("]}");
        return json.ToString();
    }

    // Loopback mock of the GitHub endpoints this updater talks to. State is
    // plain fields the test reconfigures between calls; a single pump thread
    // serves the strictly sequential requests the Updater issues.
    private sealed class MockServer : IDisposable
    {
        private readonly HttpListener listener;
        private readonly Thread pump;
        private readonly int port;

        public string LatestJson;
        public byte[] AssetBytes;
        public string SumsBody;
        public string LastUserAgent;

        public string Base
        {
            get { return "http://127.0.0.1:" + port + "/"; }
        }

        private MockServer(HttpListener listener, int port)
        {
            this.listener = listener;
            this.port = port;
            pump = new Thread(Run);
            pump.IsBackground = true;
        }

        public static MockServer Start()
        {
            Random random = new Random();
            int port = random.Next(20000, 40000);
            for (int attempt = 0; attempt < 60; attempt++)
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
                try
                {
                    listener.Start();
                    MockServer server = new MockServer(listener, port);
                    server.pump.Start();
                    return server;
                }
                catch (HttpListenerException)
                {
                    listener.Close();
                    port++;
                }
            }
            throw new Exception("could not bind a loopback mock port");
        }

        private void Run()
        {
            while (listener.IsListening)
            {
                HttpListenerContext context;
                try { context = listener.GetContext(); }
                catch (Exception) { break; }
                try { Handle(context); }
                catch (Exception) { }
            }
        }

        private void Handle(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;
            if (path == "/repos/" + AppInfo.RepoOwner + "/" + AppInfo.RepoName + "/releases/latest")
            {
                LastUserAgent = context.Request.Headers["User-Agent"];
                if (LatestJson == null) { NotFound(context); return; }
                WriteBytes(context, Encoding.UTF8.GetBytes(LatestJson), "application/json");
                return;
            }
            if (path == "/redirect")
            {
                context.Response.StatusCode = 302;
                context.Response.Headers["Location"] = Base + "asset";
                context.Response.ContentLength64 = 0;
                context.Response.Close();
                return;
            }
            if (path == "/asset")
            {
                if (AssetBytes == null) { NotFound(context); return; }
                WriteBytes(context, AssetBytes, "application/octet-stream");
                return;
            }
            if (path == "/sums")
            {
                if (SumsBody == null) { NotFound(context); return; }
                WriteBytes(context, Encoding.UTF8.GetBytes(SumsBody), "text/plain");
                return;
            }
            NotFound(context);
        }

        private static void NotFound(HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentLength64 = 0;
            context.Response.Close();
        }

        private static void WriteBytes(HttpListenerContext context, byte[] body, string contentType)
        {
            context.Response.ContentType = contentType;
            context.Response.ContentLength64 = body.Length;
            context.Response.OutputStream.Write(body, 0, body.Length);
            context.Response.Close();
        }

        public void Dispose()
        {
            try { listener.Stop(); } catch { }
            try { listener.Close(); } catch { }
            pump.Join(3000);
        }
    }
}
