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
        VerifyGate.Enter();
        try
        {
            Check(args.Length == 0, "Usage: VerifyUpdate.exe");
            // Mock traffic must stay off any system proxy - the mock is on the
            // loopback interface and the requests are all ours.
            WebRequest.DefaultWebProxy = null;

            CheckVersionTable();
            CheckFullChain();
            CheckDualAssetAndStaging();

            Console.WriteLine("PASS TOTAL: update checks (version table + offline full chain + dual-form staging)");
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
        Check(current[0] == 1 && current[1] == 2 && current[2] == 0,
            "AppInfo.CurrentVersion does not read the declared 1.2.0");
        Check(AppInfo.IsNewer("v1.2.1", current), "v1.2.1 must read newer");
        Check(AppInfo.IsNewer("v1.3.0", current), "v1.3.0 must read newer");
        Check(AppInfo.IsNewer("v2.0.0", current), "v2.0.0 must read newer");
        Check(AppInfo.IsNewer("v10.0.0", current), "v10.0.0 must read newer (numeric order, not string order)");
        Check(!AppInfo.IsNewer("v0.9.9", current), "v0.9.9 must read older");
        Check(!AppInfo.IsNewer("v1.0.0", current), "v1.0.0 must read older, not newer");
        Check(!AppInfo.IsNewer("v1.0.1", current), "v1.0.1 must read older, not newer");
        Check(!AppInfo.IsNewer("v1.1.0", current), "v1.1.0 must read older, not newer");
        Check(!AppInfo.IsNewer("v1.2.0", current), "v1.2.0 must read equal, not newer");
        Check(!AppInfo.IsNewer("1.2.0", current), "bare 1.2.0 must read equal, not newer");
        Check(!AppInfo.IsNewer("v1.0", current), "two segments must fail closed");
        Check(!AppInfo.IsNewer("v1.0.0.1", current), "four segments must fail closed");
        Check(!AppInfo.IsNewer("v1.0.0-beta", current), "prerelease suffix must fail closed");
        Check(!AppInfo.IsNewer("v1.0.x", current), "non-numeric segment must fail closed");
        Check(!AppInfo.IsNewer("latest", current), "word tag must fail closed");
        Check(!AppInfo.IsNewer("", current), "empty tag must fail closed");
        Check(!AppInfo.IsNewer(null, current), "null tag must fail closed");
        Check(!AppInfo.IsNewer("v9999999999.0.0", current), "overflowing segment must fail closed");
        Check(!AppInfo.IsNewer("v 1.2.0", current), "inner whitespace tag must fail closed");
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
                server.LatestJson = BuildLatestJson("v1.2.0", "sha256:" + payloadSha,
                    payload.Length, server.Base + "/asset", null);
                Check(Updater.Check(server.Base) == null, "an equal tag must read as no update");
                server.LatestJson = BuildLatestJson("v1.2.0-rc1", "sha256:" + payloadSha,
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

                // -- concurrent replaces on the same target (V7S): two flows
                // released by a start gun enter ReplaceSequence together, the
                // on-disk replay of two settings windows clicking 立即更新 at
                // the same time. Unfixed this collides nearly every round (one
                // flow loses the backup race or copies onto a recreated
                // target); under the update mutex exactly one effective swap
                // runs, the second flow ends as the no-op success - proven by
                // the backup still holding the old bytes, since a second full
                // swap would have renamed the new bytes into the backup name.
                const int RaceRounds = 50;
                int badRounds = 0;
                StringBuilder raceSamples = new StringBuilder();
                for (int round = 1; round <= RaceRounds; round++)
                {
                    string twin = Path.Combine(work, "race-" + round + ".exe");
                    File.WriteAllBytes(twin, oldBytes);
                    File.WriteAllBytes(twin + ".old", Encoding.UTF8.GetBytes("STALE"));
                    string srcA = Path.Combine(work, "race-a-" + round + ".bin");
                    string srcB = Path.Combine(work, "race-b-" + round + ".bin");
                    File.WriteAllBytes(srcA, payload);
                    File.WriteAllBytes(srcB, payload);
                    string[] errors = RaceReplace(twin, srcA, srcB);
                    byte[] terminal = File.Exists(twin) ? File.ReadAllBytes(twin) : null;
                    bool targetOk = terminal != null && BytesEqual(terminal, payload);
                    bool backupOk = File.Exists(twin + ".old")
                        && BytesEqual(File.ReadAllBytes(twin + ".old"), oldBytes);
                    if (errors[0] == null && errors[1] == null && targetOk && backupOk) continue;
                    badRounds++;
                    if (raceSamples.Length < 400)
                    {
                        string first = errors[0] ?? errors[1];
                        raceSamples.Append("#").Append(round).Append(": ")
                            .Append(first ?? (targetOk ? "backup corrupted" : "target corrupted")).Append("; ");
                    }
                }
                Check(badRounds == 0, RaceRounds + " concurrent replace rounds produced " + badRounds
                    + " bad rounds (" + raceSamples + ")");
                Console.WriteLine("PASS: concurrent replaces serialize - one effective swap, "
                    + "second flow no-op, backup intact (x" + RaceRounds + ")");
            }
        }
        finally
        {
            try { Directory.Delete(work, true); } catch { }
        }
    }

    // 3. V10 dual-form additions (D1/D2): the Setup asset resolution, the
    // installed-form probe, the staged predownload (clean staging, verify,
    // failure cleanup, per-form asset pick and fail-closed refusal), and the
    // frozen silent-restart command line - asserted as data, the installer
    // itself is never executed.
    private static void CheckDualAssetAndStaging()
    {
        // The frozen asset-name formula first (RESEARCH §7).
        Check(Updater.SetupAssetNameFor("v9.9.9") == "OneClickBlackScreen-Setup-9.9.9.exe",
            "SetupAssetNameFor must strip the v prefix");
        Check(Updater.SetupAssetNameFor("1.2.3") == "OneClickBlackScreen-Setup-1.2.3.exe",
            "SetupAssetNameFor must accept a bare version");
        // The frozen silent arguments as data - never launched.
        Check(Updater.SetupSilentArguments == "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS",
            "the silent Setup arguments must match the frozen contract");
        System.Diagnostics.ProcessStartInfo startInfo = Updater.SetupStartInfo(@"C:\somewhere\setup.exe");
        Check(startInfo.FileName == @"C:\somewhere\setup.exe", "SetupStartInfo must carry the path");
        Check(startInfo.Arguments == Updater.SetupSilentArguments,
            "SetupStartInfo must carry the frozen arguments");
        Console.WriteLine("PASS: Setup asset name + silent command line pinned as data (no installer executed)");

        string work = Path.Combine(Path.GetTempPath(), "VerifyUpdateStaged-" + Path.GetRandomFileName());
        Directory.CreateDirectory(work);
        string staging = Updater.StagingDirectory;
        try
        {
            using (MockServer server = MockServer.Start())
            {
                byte[] exePayload = BuildPayload();
                byte[] setupPayload = new byte[100 * 1024];
                for (int i = 0; i < setupPayload.Length; i++)
                    setupPayload[i] = (byte)(i * 17 + 3);
                string exeSha = Sha256Hex(exePayload);
                string setupSha = Sha256Hex(setupPayload);

                // -- dual-asset release: both URLs/hashes ride on UpdateInfo.
                server.LatestJson = BuildDualJson("v9.9.9", exeSha, setupSha,
                    exePayload.Length, setupPayload.Length, server.Base);
                server.AssetBytes = exePayload;
                server.SetupBytes = setupPayload;
                Updater.UpdateInfo dual = Updater.Check(server.Base);
                Check(dual != null, "the dual release must resolve");
                Check(dual.SetupUrl == server.Base + "setup", "SetupUrl mismatch: " + dual.SetupUrl);
                Check(dual.SetupSize == setupPayload.Length, "SetupSize mismatch");
                Check(dual.SetupSha256Hex == setupSha, "SetupSha256Hex mismatch");
                Check(dual.DownloadUrl != null && dual.Sha256Hex == exeSha,
                    "the exe fields must stay populated alongside the Setup fields");
                Console.WriteLine("PASS: dual-asset Check (Setup + exe URLs/hashes on UpdateInfo)");

                // -- installed form: staged Setup download, clean staging, junk gone.
                Updater.UseInstalledFormProbe(delegate { return true; });
                try
                {
                    Directory.CreateDirectory(staging);
                    File.WriteAllText(Path.Combine(staging, "stale-junk.bin"), "STALE");
                    string staged = Updater.DownloadStaged(dual, null);
                    Check(Path.GetFullPath(staged)
                        == Path.GetFullPath(Path.Combine(staging, "OneClickBlackScreen-Setup-9.9.9.exe")),
                        "the staged file must be the Setup asset name in the staging dir: " + staged);
                    Check(BytesEqual(File.ReadAllBytes(staged), setupPayload),
                        "the staged Setup bytes must match the payload");
                    string[] left = Directory.GetFiles(staging);
                    Check(left.Length == 1, "staging must hold exactly the staged file, saw " + left.Length);
                    Console.WriteLine("PASS: installed-form staged predownload (staging reset, verified artifact resident)");

                    // -- tampered Setup digest: refused, staging cleaned.
                    string wrongSha = Sha256Hex(Encoding.UTF8.GetBytes("not the setup"));
                    server.LatestJson = BuildDualJson("v9.9.9", exeSha, wrongSha,
                        exePayload.Length, setupPayload.Length, server.Base);
                    Updater.UpdateInfo tampered = Updater.Check(server.Base);
                    ExpectFailure(delegate { Updater.DownloadStaged(tampered, null); },
                        "a tampered Setup digest", "SHA-256");
                    Check(!Directory.Exists(staging) || Directory.GetFiles(staging).Length == 0,
                        "a failed staged download must leave the staging directory clean");
                    Console.WriteLine("PASS: tampered Setup digest refused + staging cleaned");

                    // -- Setup asset missing + installed form: fail-closed per form.
                    server.LatestJson = BuildLatestJson("v9.9.9", "sha256:" + exeSha,
                        exePayload.Length, server.Base + "/asset", null);
                    Updater.UpdateInfo exeOnly = Updater.Check(server.Base);
                    Check(exeOnly != null && exeOnly.SetupUrl == null,
                        "an exe-only release must resolve with SetupUrl null");
                    ExpectFailure(delegate { Updater.DownloadStaged(exeOnly, null); },
                        "the installed form against an exe-only release", "安装包资产");
                    Console.WriteLine("PASS: missing Setup asset refuses the installed form (fail-closed by form)");
                }
                finally
                {
                    Updater.UseInstalledFormProbe(null);
                }

                // -- portable form: the staged artifact is the exe itself.
                Updater.UseInstalledFormProbe(delegate { return false; });
                try
                {
                    server.LatestJson = BuildDualJson("v9.9.9", exeSha, setupSha,
                        exePayload.Length, setupPayload.Length, server.Base);
                    Updater.UpdateInfo dualAgain = Updater.Check(server.Base);
                    string stagedExe = Updater.DownloadStaged(dualAgain, null);
                    Check(Path.GetFileName(stagedExe) == AppInfo.UpdateAssetName,
                        "the portable form must stage the exe asset: " + stagedExe);
                    Check(BytesEqual(File.ReadAllBytes(stagedExe), exePayload),
                        "the staged exe bytes must match the payload");
                    Console.WriteLine("PASS: portable-form staged predownload (exe asset, verified)");
                }
                finally
                {
                    Updater.UseInstalledFormProbe(null);
                }

                // -- Setup digest absent, SHA256SUMS fallback carries it.
                string sumsBody = setupSha + "  OneClickBlackScreen-Setup-9.9.9.exe\n"
                    + exeSha + "  " + AppInfo.UpdateAssetName + "\n";
                server.LatestJson = BuildDualJson("v9.9.9", exeSha, null,
                    exePayload.Length, setupPayload.Length, server.Base, true);
                server.SumsBody = sumsBody;
                Updater.UseInstalledFormProbe(delegate { return true; });
                try
                {
                    Updater.UpdateInfo sumsOnly = Updater.Check(server.Base);
                    Check(sumsOnly != null && sumsOnly.SetupSha256Hex == null,
                        "the digest-less Setup asset must resolve before the fallback");
                    string stagedSums = Updater.DownloadStaged(sumsOnly, null);
                    Check(BytesEqual(File.ReadAllBytes(stagedSums), setupPayload),
                        "the SHA256SUMS fallback must verify the staged Setup");
                    Console.WriteLine("PASS: SHA256SUMS fallback verified the Setup asset line");
                }
                finally
                {
                    Updater.UseInstalledFormProbe(null);
                }
            }
        }
        finally
        {
            try { if (Directory.Exists(staging)) Directory.Delete(staging, true); } catch { }
            try { Directory.Delete(work, true); } catch { }
        }
    }

    // A release carrying both update assets (plus the SUMS asset when asked),
    // each with its own digest or null.
    private static string BuildDualJson(string tag, string exeSha, string setupSha,
        long exeSize, long setupSize, string baseUrl)
    {
        return BuildDualJson(tag, exeSha, setupSha, exeSize, setupSize, baseUrl, true);
    }

    private static string BuildDualJson(string tag, string exeSha, string setupSha,
        long exeSize, long setupSize, string baseUrl, bool withSums)
    {
        StringBuilder json = new StringBuilder();
        json.Append("{\"tag_name\": \"").Append(tag).Append("\", \"prerelease\": false, \"assets\": [");
        json.Append("{\"name\": \"").Append(AppInfo.UpdateAssetName)
            .Append("\", \"browser_download_url\": \"").Append(baseUrl).Append("asset")
            .Append("\", \"size\": ").Append(exeSize)
            .Append(", \"digest\": ").Append(exeSha == null ? "null" : "\"sha256:" + exeSha + "\"")
            .Append("}");
        json.Append(", {\"name\": \"").Append(Updater.SetupAssetNameFor(tag))
            .Append("\", \"browser_download_url\": \"").Append(baseUrl).Append("setup")
            .Append("\", \"size\": ").Append(setupSize)
            .Append(", \"digest\": ").Append(setupSha == null ? "null" : "\"sha256:" + setupSha + "\"")
            .Append("}");
        if (withSums)
            json.Append(", {\"name\": \"SHA256SUMS\", \"browser_download_url\": \"")
                .Append(baseUrl).Append("sums").Append("\", \"size\": 96, \"digest\": null}");
        json.Append("]}");
        return json.ToString();
    }

    // One barrier round: both waiters park on the gun, the gun releases them
    // into ReplaceSequence at the same instant, and the per-slot errors come
    // back after Join (slot i is written only by thread i, so no locking).
    private static string[] RaceReplace(string target, string srcA, string srcB)
    {
        string[] errors = new string[2];
        using (ManualResetEvent gun = new ManualResetEvent(false))
        {
            Thread a = new Thread(delegate()
            {
                gun.WaitOne();
                try { Updater.ReplaceSequence(srcA, target); } catch (Exception ex) { errors[0] = ex.Message; }
            });
            Thread b = new Thread(delegate()
            {
                gun.WaitOne();
                try { Updater.ReplaceSequence(srcB, target); } catch (Exception ex) { errors[1] = ex.Message; }
            });
            a.Start();
            b.Start();
            gun.Set();
            a.Join();
            b.Join();
        }
        return errors;
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
