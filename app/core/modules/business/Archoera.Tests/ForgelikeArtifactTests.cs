using Archoera.Domain.Loaders;

namespace Archoera.Tests;

public sealed class ForgelikeArtifactTests
{
    [Theory]
    [InlineData(LoaderKind.Forge, "1.20.1", "47.2.0", "net.minecraftforge", "forge", "1.20.1-47.2.0")]
    [InlineData(LoaderKind.Forge, "1.20.1", "1.20.1-47.2.0", "net.minecraftforge", "forge", "1.20.1-47.2.0")]
    [InlineData(LoaderKind.NeoForge, "1.20.1", "47.1.106", "net.neoforged", "forge", "1.20.1-47.1.106")]
    [InlineData(LoaderKind.NeoForge, "1.20.1", "1.20.1-47.1.106", "net.neoforged", "forge", "1.20.1-47.1.106")]
    [InlineData(LoaderKind.NeoForge, "1.20.1", "1.20.1-forge-47.1.106", "net.neoforged", "forge", "1.20.1-47.1.106")]
    [InlineData(LoaderKind.NeoForge, "1.21.1", "21.1.265", "net.neoforged", "neoforge", "21.1.265")]
    [InlineData(LoaderKind.NeoForge, "26.1.2", "26.1.2.126", "net.neoforged", "neoforge", "26.1.2.126")]
    public void Loader_builds_map_to_their_maven_artifact(
        LoaderKind kind,
        string gameVersion,
        string loaderVersion,
        string group,
        string artifactId,
        string version)
    {
        var artifact = ForgelikeArtifacts.For(kind, gameVersion, loaderVersion);

        Assert.Equal(group, artifact.GroupId);
        Assert.Equal(artifactId, artifact.ArtifactId);
        Assert.Equal(version, artifact.Version);
        Assert.Equal($"{group}:{artifactId}:{version}", artifact.LibraryName);

        var directory = $"{group.Replace('.', '/')}/{artifactId}/{version}";
        Assert.Equal($"{directory}/{artifactId}-{version}.jar", artifact.RelativePath);
        Assert.Equal($"{directory}/{artifactId}-{version}-installer.jar", artifact.InstallerPath);
    }

    [Theory]
    [InlineData("net.minecraftforge:forge:1.20.1-47.2.0", LoaderKind.Forge, "1.20.1-47.2.0")]
    [InlineData("net.minecraftforge:forge:1.20.1-47.2.0:universal", LoaderKind.Forge, "1.20.1-47.2.0")]
    [InlineData("net.neoforged:forge:1.20.1-47.1.106:universal", LoaderKind.NeoForge, "1.20.1-47.1.106")]
    [InlineData("net.neoforged:neoforge:21.1.265", LoaderKind.NeoForge, "21.1.265")]
    public void Library_coordinates_round_trip(
        string name,
        LoaderKind kind,
        string version)
    {
        Assert.True(ForgelikeArtifacts.TryParseLibrary(name, out var artifact));
        Assert.Equal(kind, artifact.Kind);
        Assert.Equal(version, artifact.Version);
    }

    [Theory]
    [InlineData("")]
    [InlineData("com.google.guava:guava:33.0")]
    [InlineData("net.neoforged.fancymodloader:loader:4.0.39")]
    [InlineData("net.neoforged:neoforge")]
    public void Unrelated_libraries_are_not_loaders(string name)
        => Assert.False(ForgelikeArtifacts.TryParseLibrary(name, out _));

    [Theory]
    [InlineData("1.20.1")]
    [InlineData("1.21.1")]
    public void Legacy_neoforge_is_only_1_20_1(string gameVersion)
        => Assert.Equal(gameVersion == "1.20.1", ForgelikeArtifacts.IsLegacyNeoForge(gameVersion));
}
