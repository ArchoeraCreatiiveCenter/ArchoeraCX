using System.Globalization;

namespace Archoera.Domain.Loaders;

/// <summary>
/// Decodes the loader version strings NeoForge publishes in its Maven
/// metadata into the Minecraft version a build targets.
/// </summary>
/// <remarks>
/// <para>
/// Modern NeoForge encodes the game version in the loader version itself:
/// <c>20.2.x</c> targets 1.20.2, <c>21.0.x</c> targets 1.21, <c>21.1.x</c>
/// targets 1.21.1, and the post-1.21 line uses the game version as its own
/// prefix (<c>26.1.2.126</c>, <c>26.1-pre-3</c>). The <c>0.x</c> line is used
/// for snapshot game versions. The legacy 1.20.1 build keeps Forge's
/// game-prefixed spelling (<c>1.20.1-47.1.106</c>).
/// </para>
/// <para>
/// Filtering by string prefix (the previous approach) cannot express this:
/// <c>1.21</c> maps to <c>21.0.x</c> while <c>1.21.1</c> maps to
/// <c>21.1.x</c>, so a <c>"21."</c> prefix also matched every later line.
/// </para>
/// </remarks>
public static class NeoForgeVersionText
{
    /// <summary>
    /// Splits a published NeoForge version into the game version it targets
    /// and the loader build shown to the user.
    /// </summary>
    /// <returns>False when the string is not a recognized NeoForge version.</returns>
    public static bool TryResolve(
        string? version,
        out string gameVersion,
        out string loaderVersion)
    {
        gameVersion = string.Empty;
        loaderVersion = string.Empty;

        var text = version?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            return false;
        }

        // Legacy 1.20.1 line keeps Forge's "<game>-<build>" spelling.
        if (LoaderVersionText.TrySplitGamePrefixed(text, out gameVersion, out loaderVersion))
        {
            return true;
        }

        var first = text.IndexOf('.');
        if (first <= 0)
        {
            return false;
        }

        var second = text.IndexOf('.', first + 1);
        if (second < 0)
        {
            return false;
        }

        if (!int.TryParse(
                text.AsSpan(0, first),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var major))
        {
            return false;
        }

        // Snapshot line, for example "0.25w14craftmine.3-beta": the middle
        // segment is the snapshot game version.
        if (major == 0)
        {
            var snapshot = text[(first + 1)..second];
            if (snapshot.Length == 0)
            {
                return false;
            }

            gameVersion = snapshot;
            loaderVersion = text;
            return true;
        }

        if (!int.TryParse(
                text.AsSpan(first + 1, second - first - 1),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var minor))
        {
            return false;
        }

        loaderVersion = text;

        // From the 26.x line on, the loader version starts with the game
        // version itself; a ".0" second component means the game version has
        // no such patch ("26.3.0.99" targets 26.3).
        if (major >= 26)
        {
            var third = text.IndexOf('.', second + 1);
            if (third < 0)
            {
                return false;
            }

            if (!int.TryParse(
                    text.AsSpan(second + 1, third - second - 1),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var patch))
            {
                return false;
            }

            var prefixLength = patch == 0 ? second : third;
            var prefix = text[..prefixLength];

            // Pre-releases carry the game version after '+'
            // ("26.1.0.0-alpha.1+snapshot-1" targets 26.1-snapshot-1).
            var plus = text.IndexOf('+');
            gameVersion = plus < 0 ? prefix : prefix + "-" + text[(plus + 1)..];
            return gameVersion.Length > 0;
        }

        // 1.20.2–1.21.x: "<major>.<minor>.<build>" targets "1.<major>.<minor>",
        // and ".0" means the game line has no patch ("21.0.x" targets 1.21).
        if (major < 20)
        {
            // A game-prefixed version without a dash, or an unknown line.
            loaderVersion = string.Empty;
            return false;
        }

        gameVersion = minor == 0
            ? "1." + text[..first]
            : "1." + text[..second];
        return true;
    }
}
