namespace Archoera.Domain.Loaders;

/// <summary>
/// Maven identity of a Forge/NeoForge loader artifact.
/// </summary>
/// <remarks>
/// Forge and NeoForge disagree on how the loader build is spelled in the
/// artifact version: Forge always prefixes the game version
/// (<c>1.20.1-47.2.0</c>), NeoForge dropped the prefix from 1.20.2 on
/// (<c>21.1.265</c>) but kept Forge's spelling for its 1.20.1 line, which was
/// published as <c>net.neoforged:forge</c>. Everything that needs a loader
/// path goes through <see cref="ForgelikeArtifacts.For"/> so the rule exists
/// once.
/// </remarks>
/// <param name="Kind">Loader family that owns the artifact.</param>
/// <param name="GroupId">Maven group id, for example <c>net.neoforged</c>.</param>
/// <param name="ArtifactId">Maven artifact id, for example <c>neoforge</c>.</param>
/// <param name="Version">Maven version exactly as published.</param>
public sealed record ForgelikeArtifact(
    LoaderKind Kind,
    string GroupId,
    string ArtifactId,
    string Version)
{
    /// <summary>Repository-relative directory that holds the artifact.</summary>
    public string RelativeDirectory =>
        $"{GroupId.Replace('.', '/')}/{ArtifactId}/{Version}";

    /// <summary>Repository-relative path of the loader jar without its extension.</summary>
    public string RelativeStem => $"{RelativeDirectory}/{ArtifactId}-{Version}";

    /// <summary>Repository-relative path of the loader jar.</summary>
    public string RelativePath => $"{RelativeStem}.jar";

    /// <summary>Installer file name, for example <c>neoforge-21.1.265-installer.jar</c>.</summary>
    public string InstallerFileName => $"{ArtifactId}-{Version}-installer.jar";

    /// <summary>Repository-relative path of the installer jar.</summary>
    public string InstallerPath => $"{RelativeDirectory}/{InstallerFileName}";

    /// <summary>Maven coordinate as it appears in a version profile.</summary>
    public string LibraryName => $"{GroupId}:{ArtifactId}:{Version}";
}
