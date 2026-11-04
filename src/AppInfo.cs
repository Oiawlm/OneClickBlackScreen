// Single source for the application identity and the update channel (B2): the
// version, the repository coordinates and the release asset name exist exactly
// once here (RESEARCH-V5 §4 single-registration table) - the settings footer
// reads them, and the release tooling/tags must match them. Version comparison
// is strict three-segment numeric; anything malformed reads as "not newer"
// (fail-closed, B2).
using System;

internal static class AppInfo
{
    public const string Version = "1.0.2";
    public const string RepoOwner = "Oiawlm";
    public const string RepoName = "OneClickBlackScreen";
    public const string UpdateAssetName = "OneClickBlackScreen.exe";

    // Production endpoint for the releases/latest check. This is the only copy
    // of the host in src/; offline verifiers inject a local base URL through
    // Updater.Check's parameter instead (B8).
    public const string UpdateApiBase = "https://api.github.com/";

    private static readonly int[] currentVersion = ParseVersion(Version);

    public static int[] CurrentVersion()
    {
        return currentVersion;
    }

    // Strict "v?D.D.D": exactly three non-empty all-digit segments after an
    // optional single lowercase "v" (our own release-tag convention). Missing
    // segments, suffixes like "-beta", non-digits, overflows and null all read
    // false, so the caller's "not newer" branch is the safe outcome.
    public static bool TryParseVersion(string tag, out int[] version)
    {
        version = null;
        if (tag == null) return false;
        string text = tag.Trim();
        if (text.StartsWith("v", StringComparison.Ordinal)) text = text.Substring(1);
        string[] parts = text.Split('.');
        if (parts.Length != 3) return false;
        int[] parsed = new int[3];
        for (int i = 0; i < 3; i++)
        {
            // <=9 digits keeps int.Parse overflow-free by construction.
            if (parts[i].Length == 0 || parts[i].Length > 9) return false;
            for (int c = 0; c < parts[i].Length; c++)
                if (parts[i][c] < '0' || parts[i][c] > '9') return false;
            parsed[i] = int.Parse(parts[i]);
        }
        version = parsed;
        return true;
    }

    // Segment-by-segment numeric compare (never string order); equal or older
    // reads false, and a malformed tag reads false as well (fail-closed).
    public static bool IsNewer(string tag, int[] current)
    {
        int[] candidate;
        if (!TryParseVersion(tag, out candidate)) return false;
        for (int i = 0; i < 3; i++)
            if (candidate[i] != current[i]) return candidate[i] > current[i];
        return false;
    }

    private static int[] ParseVersion(string tag)
    {
        int[] parsed;
        return TryParseVersion(tag, out parsed) ? parsed : new int[] { 0, 0, 0 };
    }
}
