namespace Archoera.Domain.Loaders;

/// <summary>
/// The single place that turns a loader build into its Maven identity.
/// </summary>
/// <remarks>
/// <para>
/// NeoForge's 1.20.1 line was published as <c>net.neoforged:forge</c> with
/// Forge's game-prefixed version (<c>1.20.1-47.1.106</c>). From 1.20.2 on it
/// publishes <c>net.neoforged:neoforge</c> with a bare version
/// (<c>21.1.265</c>). That fact used to be re-derived by the installer
/// planner, the launch preflight, instance discovery and the version list;
/// they now all call <see cref="For"/> or <see cref="TryParseLibrary"/>.
/// </para>
/// </remarks>
public static class ForgelikeArtifacts
{
    /// <summary>Forge's Maven group.</summary>
    public const string ForgeGroup = "net.minecraftforge";

    /// <summary>NeoForge's Maven group, used by both its artifacts.</summary>
    public const string NeoForgeGroup = "net.neoforged";

    /// <summary>The only game version the legacy <c>net.neoforged:forge</c> line covers.</summary>
    public const string LegacyNeoForgeGameVersion = "1.20.1";

    /// <summary>
    /// True for the NeoForge line that used Forge's artifact id and
    /// game-prefixed version spelling.
    /// </summary>
    public static bool IsLegacyNeoForge(string? gameVersion)
        => string.Equals(
            gameVersion?.Trim(),
            LegacyNeoForgeGameVersion,
            StringComparison.Ordinal);

    /// <summary>
    /// Maven identity for a loader build. <paramref name="loaderVersion"/> may
    /// be any spelling an imported profile carries (profile id, game-prefixed
    /// Maven version or bare build); it is normalized before the artifact is
    /// built.
    /// </summary>
    /// <exception cref="ArgumentException">A required version is empty.</exception>
    public static ForgelikeArtifact For(
        LoaderKind kind,
        string? gameVersion,
        string? loaderVersion)
    {
        var game = gameVersion?.Trim() ?? string.Empty;
        var loader = loaderVersion?.Trim() ?? string.Empty;
        if (game.Length == 0)
        {
            throw new ArgumentException("game version is required", nameof(gameVersion));
        }

        if (loader.Length == 0)
        {
            throw new ArgumentException("loader version is required", nameof(loaderVersion));
        }

        // Accepts every spelling an imported profile may carry
        // ("1.20.1-forge-47.4.18", "forge-1.20.1-47.4.18", bare build).
        loader = LoaderVersionText.Normalize(kind.ToString(), game, loader);
        if (loader.Length == 0)
        {
            throw new ArgumentException("loader version is required", nameof(loaderVersion));
        }

        return kind switch
        {
            LoaderKind.Forge =>
                new ForgelikeArtifact(kind, ForgeGroup, "forge", $"{game}-{loader}"),
            LoaderKind.NeoForge when IsLegacyNeoForge(game) =>
                new ForgelikeArtifact(kind, NeoForgeGroup, "forge", $"{game}-{loader}"),
            LoaderKind.NeoForge =>
                new ForgelikeArtifact(kind, NeoForgeGroup, "neoforge", loader),
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "unsupported loader kind"),
        };
    }

    /// <summary>
    /// Reads the loader family back out of a library coordinate
    /// (<c>g:a:v[:classifier]</c>). The version is returned as published, so
    /// legacy entries still carry the game prefix.
    /// </summary>
    public static bool TryParseLibrary(string? name, out ForgelikeArtifact artifact)
    {
        artifact = null!;
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var parts = name.Trim().Split(':');
        if (parts.Length < 3)
        {
            return false;
        }

        var group = parts[0].Trim();
        var id = parts[1].Trim();
        var version = parts[2].Trim();
        if (group.Length == 0 || id.Length == 0 || version.Length == 0)
        {
            return false;
        }

        LoaderKind kind;
        if (string.Equals(group, ForgeGroup, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(id, "forge", StringComparison.OrdinalIgnoreCase))
        {
            kind = LoaderKind.Forge;
        }
        else if (string.Equals(group, NeoForgeGroup, StringComparison.OrdinalIgnoreCase) &&
                 (string.Equals(id, "forge", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(id, "neoforge", StringComparison.OrdinalIgnoreCase)))
        {
            // net.neoforged:forge is the 1.20.1 line; it is still NeoForge and
            // must not be reported as Forge just because of the artifact id.
            kind = LoaderKind.NeoForge;
        }
        else
        {
            return false;
        }

        artifact = new ForgelikeArtifact(kind, group, id, version);
        return true;
    }
}
