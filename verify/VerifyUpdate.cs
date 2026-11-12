using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

// Offline updater verifier (B8): exercises AppInfo's version comparison table,
// then drives the single-form update chain (V11 D4) against a local
// HttpListener mock of releases/latest - JSON payload, Setup asset, digest,
// SHA256SUMS fallback, 302 redirect - and asserts the staged predownload into
// %TEMP%\BlackScreenUpdate\ (Setup asset name, verification, failure cleanup).
// The portable self-replace chain is gone with the portable form; the
// installer itself is never executed - the silent command line is pinned as
// data. The real dist exe is never touched, and no packet leaves the loopback
// interface; the live GitHub endpoint stays an I-wave concern (I4).
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
        VerifyGate.Enter();
        try
        {
            Check(args.Length == 0, "Usage: VerifyUpdate.exe");
            // Mock traffic must stay off any system proxy - the mock is on the
            // loopback interface and the requests are all ours.
            WebRequest.DefaultWebProxy = null;

            CheckVersionTable();
            CheckSingleFormChain();

            Console.WriteLine("PASS TOTAL: update checks (version table + single-form chain + staged Setup predownload)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }

    // 1. Version comparison (B2): newer per segment, equal, older, and every
    // malformed shape reading fail-closed. The table mirrors AppInfo.Version's
    // declared digits; the version itself moves only in the I wave.
    private static void CheckVersionTable()
    {
        int[] current = AppInfo.CurrentVersion();
        Check(current[0] == 1 && current[1] == 3 && current[2] == 0,
            "AppInfo.CurrentVersion does not read the declared 1.3.0");
        Check(AppInfo.IsNewer("v1.3.1", current), "v1.3.1 must read newer");
        Check(AppInfo.IsNewer("v1.4.0", current), "v1.4.0 must read newer");
        Check(AppInfo.IsNewer("v2.0.0", current), "v2.0.0 must read newer");
        Check(AppInfo.IsNewer("v10.0.0", current), "v10.0.0 must read newer (numeric order, not string order)");
        Check(!AppInfo.IsNewer("v0.9.9", current), "v0.9.9 must read older");
        Check(!AppInfo.IsNewer("v1.0.0", current), "v1.0.0 must read older, not newer");
        Check(!AppInfo.IsNewer("v1.0.1", current), "v1.0.1 must read older, not newer");
        Check(!AppInfo.IsNewer("v1.1.0", current), "v1.1.0 must read older, not newer");
        Check(!AppInfo.IsNewer("v1.2.0", current), "v1.2.0 must read older, not newer");
        Check(!AppInfo.IsNewer("v1.3.0", current), "v1.3.0 must read equal, not newer");
        Check(!AppInfo.IsNewer("1.3.0", current), "bare 1.3.0 must read equal, not newer");
        Check(!AppInfo.IsNewer("v1.0", current), "two segments must fail closed");
        Check(!AppInfo.IsNewer("v1.0.0.1", current), "four segments must fail closed");
        Check(!AppInfo.IsNewer("v1.0.0-beta", current), "prerelease suffix must fail closed");
        Check(!AppInfo.IsNewer("v1.0.x", current), "non-numeric segment must fail closed");
        Check(!AppInfo.IsNewer("latest", current), "word tag must fail closed");
        Check(!AppInfo.IsNewer("", current), "empty tag must fail closed");
        Check(!AppInfo.IsNewer(null, current), "null tag must fail closed");
        Check(!AppInfo.IsNewer("v9999999999.0.0", current), "overflowing segment must fail closed");
        Check(!AppInfo.IsNewer("v 1.3.0", current), "inner whitespace tag must fail closed");
        int[] parsed;
        Check(AppInfo.TryParseVersion("v1.2.3", out parsed)
            && parsed[0] == 1 && parsed[1] == 2 && parsed[2] == 3,
            "TryParseVersion must accept v-prefixed tags");
        Check(AppInfo.TryParseVersion("0.20.300", out parsed)
            && parsed[0] == 0 && parsed[1] == 20 && parsed[2] == 300,
            "TryParseVersion must accept zero-padded bare versions");
        Check(!AppInfo.TryParseVersion("v.1.2", out parsed), "empty segment must fail closed");
        Check(!AppInfo.TryParseVersion("V1.1.0", out parsed), "uppercase V prefix must fail closed (our tags are lowercase v)");
        Console.WriteLine("PASS: version comparison table (newer/equal/older/malformed, fail-closed)");
    }

    // 2. Single-form chain against the local mock (V11 D4): check -> 302
    // staged Setup download -> digest verify, plus every refusal, the staging
    // discipline, and the frozen silent command line. The replace rehearsal,
    // the swap race and the dual-form probes are gone with the portable form.
    private static void CheckSingleFormChain()
    {
        // The frozen asset-name constants first (RESEARCH-V11 §7 tables 1/2).
        Check(AppInfo.SetupAssetNamePrefix == "OneClickBlackScreen-Setup-",
            "AppInfo.SetupAssetNamePrefix must match the frozen contract");
        Check(AppInfo.ChecksumsAssetName == "SHA256SUMS.txt",
            "AppInfo.ChecksumsAssetName must match the frozen contract");
        Check(Updater.SetupAssetNameFor("v9.9.9") == "OneClickBlackScreen-Setup-9.9.9.exe",
            "SetupAssetNameFor must build prefix + stripped tag + .exe");
        Check(Updater.SetupAssetNameFor("1.2.3") == "OneClickBlackScreen-Setup-1.2.3.exe",
            "SetupAssetNameFor must accept a bare version");
        // The frozen silent arguments as data - never launched.
        Check(Updater.SetupSilentArguments == "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS",
            "the silent Setup arguments must match the frozen contract");
        System.Diagnostics.ProcessStartInfo startInfo = Updater.SetupStartInfo(@"C:\somewhere\setup.exe");
        Check(startInfo.FileName == @"C:\somewhere\setup.exe", "SetupStartInfo must carry the path");
        Check(startInfo.Arguments == Updater.SetupSilentArguments,
            "SetupStartInfo must carry the frozen arguments");
        Console.WriteLine("PASS: Setup asset constants + silent command line pinned as data (no installer executed)");

        string staging = Updater.StagingDirectory;
        try
        {
            using (MockServer server = MockServer.Start())
            {
                byte[] setupPayload = BuildPayload();
                string setupSha = Sha256Hex(setupPayload);

                // -- happy path: digest + 302 redirect to the Setup asset.
                server.LatestJson = BuildReleaseJson("v9.9.9", "sha256:" + setupSha,
                    setupPayload.Length, server.Base + "/redirect", server.Base + "/sums");
                server.SetupBytes = setupPayload;
                server.SumsBody = setupSha + "  OneClickBlackScreen-Setup-9.9.9.exe\n";
                Updater.UpdateInfo info = Updater.Check(server.Base);
                Check(info != null, "Check must report the newer release");
                Check(info.Tag == "v9.9.9", "tag mismatch: " + info.Tag);
                Check(info.SetupUrl == server.Base + "/redirect", "SetupUrl mismatch: " + info.SetupUrl);
                Check(info.SetupSize == setupPayload.Length, "SetupSize mismatch");
                Check(info.SetupSha256Hex == setupSha, "Setup digest mismatch");
                Check(server.LastUserAgent != null
                    && server.LastUserAgent.IndexOf("OneClickBlackScreen-Updater/" + AppInfo.Version, StringComparison.Ordinal) >= 0,
                    "requests must carry the custom User-Agent, saw: " + server.LastUserAgent);

                // -- staged predownload: clean staging, Setup asset name resident.
                Directory.CreateDirectory(staging);
                File.WriteAllText(Path.Combine(staging, "stale-junk.bin"), "STALE");
                List<int> progress = new List<int>();
                string staged = Updater.DownloadStaged(info, delegate(int percent) { progress.Add(percent); });
                Check(Path.GetFullPath(staged)
                    == Path.GetFullPath(Path.Combine(staging, "OneClickBlackScreen-Setup-9.9.9.exe")),
                    "the staged file must be the Setup asset name in the staging dir: " + staged);
                Check(BytesEqual(File.ReadAllBytes(staged), setupPayload),
                    "the staged Setup bytes must match the payload");
                string[] left = Directory.GetFiles(staging);
                Check(left.Length == 1, "staging must hold exactly the staged file, saw " + left.Length);
                Check(progress.Count >= 2 && progress[progress.Count - 1] == 100,
                    "progress must stream during the download and finish at 100");
                for (int i = 1; i < progress.Count; i++)
                    Check(progress[i] >= progress[i - 1], "progress must be monotonic");
                Console.WriteLine("PASS: staged predownload (302 download -> digest verify -> Setup artifact resident, staging reset)");

                // -- tampered digest: refuse, staging cleaned.
                string wrongSha = Sha256Hex(Encoding.UTF8.GetBytes("not the setup"));
                server.LatestJson = BuildReleaseJson("v9.9.9", "sha256:" + wrongSha,
                    setupPayload.Length, server.Base + "/setup", null);
                Updater.UpdateInfo tampered = Updater.Check(server.Base);
                Check(tampered != null && tampered.SetupSha256Hex == wrongSha,
                    "the tampered release must still resolve");
                ExpectFailure(delegate { Updater.DownloadStaged(tampered, null); },
                    "a tampered Setup digest", "SHA-256");
                Check(!Directory.Exists(staging) || Directory.GetFiles(staging).Length == 0,
                    "a failed staged download must leave the staging directory clean");
                Console.WriteLine("PASS: tampered Setup digest refused + staging cleaned");

                // -- Setup asset missing from the release: honest failure naming it.
                server.LatestJson = BuildReleaseJson("v9.9.9", null, 0, null, server.Base + "/sums");
                ExpectFailure(delegate { Updater.Check(server.Base); },
                    "a release without the Setup asset", Updater.SetupAssetNameFor("v9.9.9"));
                Console.WriteLine("PASS: missing Setup asset fails with its name in the message");

                // -- digest absent, SHA256SUMS fallback carries the chain (B5),
                //    matching the frozen SUMS entry format "<hex> *<name>".
                server.LatestJson = BuildReleaseJson("v9.9.9", null,
                    setupPayload.Length, server.Base + "/setup", server.Base + "/sums");
                server.SumsBody = setupSha + " *OneClickBlackScreen-Setup-9.9.9.exe\n"
                    + Sha256Hex(Encoding.UTF8.GetBytes("other")) + "  some-other-file.zip\n";
                Updater.UpdateInfo sumsInfo = Updater.Check(server.Base);
                Check(sumsInfo != null && sumsInfo.SetupSha256Hex == null && sumsInfo.SumsUrl != null,
                    "the digest-less release must expose the SUMS fallback");
                string stagedSums = Updater.DownloadStaged(sumsInfo, null);
                Check(BytesEqual(File.ReadAllBytes(stagedSums), setupPayload),
                    "the SHA256SUMS fallback must verify the staged Setup");
                Console.WriteLine("PASS: SHA256SUMS fallback verified the Setup asset line (<hex> *<name>)");

                // -- double missing (no digest, no SUMS asset): refuse (B5).
                server.LatestJson = BuildReleaseJson("v9.9.9", null,
                    setupPayload.Length, server.Base + "/setup", null);
                Updater.UpdateInfo noHashInfo = Updater.Check(server.Base);
                Check(noHashInfo != null && noHashInfo.SumsUrl == null,
                    "the hash-less release must resolve before the refusal");
                ExpectFailure(delegate { Updater.DownloadStaged(noHashInfo, null); },
                    "a digest-less, SUMS-less release", "校验和");
                // -- SUMS asset present but no matching line: same refusal.
                server.LatestJson = BuildReleaseJson("v9.9.9", null,
                    setupPayload.Length, server.Base + "/setup", server.Base + "/sums");
                server.SumsBody = Sha256Hex(Encoding.UTF8.GetBytes("other")) + "  some-other-file.zip\n";
                Updater.UpdateInfo wrongSumsInfo = Updater.Check(server.Base);
                Check(wrongSumsInfo != null, "the wrong-SUMS release must resolve before the refusal");
                ExpectFailure(delegate { Updater.DownloadStaged(wrongSumsInfo, null); },
                    "a SUMS file without the Setup asset line", "校验和");
                Console.WriteLine("PASS: digest and SHA256SUMS both missing/mismatched refused");

                // -- size disagreement: refuse, staging cleaned.
                server.LatestJson = BuildReleaseJson("v9.9.9", "sha256:" + setupSha,
                    setupPayload.Length + 1, server.Base + "/setup", null);
                Updater.UpdateInfo shortInfo = Updater.Check(server.Base);
                Check(shortInfo != null, "the short-size release must resolve before the refusal");
                ExpectFailure(delegate { Updater.DownloadStaged(shortInfo, null); },
                    "a declared size that disagrees with the bytes", "下载不完整");
                Check(!Directory.Exists(staging) || Directory.GetFiles(staging).Length == 0,
                    "the short-size failure must leave the staging directory clean");
                Console.WriteLine("PASS: size mismatch refused + staging cleaned");

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
                Console.WriteLine("PASS: 404 / malformed JSON fail closed");

                // -- not newer / malformed tags read as no update (B2).
                server.LatestJson = BuildReleaseJson("v1.3.0", "sha256:" + setupSha,
                    setupPayload.Length, server.Base + "/setup", null);
                Check(Updater.Check(server.Base) == null, "an equal tag must read as no update");
                server.LatestJson = BuildReleaseJson("v1.2.0-rc1", "sha256:" + setupSha,
                    setupPayload.Length, server.Base + "/setup", null);
                Check(Updater.Check(server.Base) == null, "a malformed tag must read as no update");
                Console.WriteLine("PASS: equal and malformed tags read as no update");
            }
        }
        finally
        {
            try { if (Directory.Exists(staging)) Directory.Delete(staging, true); } catch { }
        }
    }

    // A release carrying the Setup asset (plus the SUMS asset when asked), the
    // digest optionally null; a null setupUrl omits the Setup entry entirely
    // (the missing-asset refusal case).
    private static string BuildReleaseJson(string tag, string digest, long size, string setupUrl, string sumsUrl)
    {
        StringBuilder json = new StringBuilder();
        json.Append("{\"tag_name\": \"").Append(tag).Append("\", \"prerelease\": false, \"assets\": [");
        if (setupUrl != null)
            json.Append("{\"name\": \"").Append(Updater.SetupAssetNameFor(tag))
                .Append("\", \"browser_download_url\": \"").Append(setupUrl)
                .Append("\", \"size\": ").Append(size)
                .Append(", \"digest\": ").Append(digest == null ? "null" : "\"" + digest + "\"")
                .Append("}");
        if (sumsUrl != null)
        {
            if (setupUrl != null) json.Append(", ");
            json.Append("{\"name\": \"").Append(AppInfo.ChecksumsAssetName)
                .Append("\", \"browser_download_url\": \"").Append(sumsUrl)
                .Append("\", \"size\": 96, \"digest\": null}");
        }
        json.Append("]}");
        return json.ToString();
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

    // Loopback mock of the GitHub endpoints this updater talks to. State is
    // plain fields the test reconfigures between calls; a single pump thread
    // serves the strictly sequential requests the Updater issues.
    private sealed class MockServer : IDisposable
    {
        private readonly HttpListener listener;
        private readonly Thread pump;
        private readonly int port;

        public string LatestJson;
        public byte[] SetupBytes;
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
                context.Response.Headers["Location"] = Base + "setup";
                context.Response.ContentLength64 = 0;
                context.Response.Close();
                return;
            }
            if (path == "/setup")
            {
                if (SetupBytes == null) { NotFound(context); return; }
                WriteBytes(context, SetupBytes, "application/octet-stream");
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
