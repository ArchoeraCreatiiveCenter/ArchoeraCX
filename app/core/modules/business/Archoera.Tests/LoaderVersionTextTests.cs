using Archoera.Application.Loaders;
using Archoera.Domain.Loaders;

namespace Archoera.Tests;

/// <summary>
/// Imported instances store the full loader profile id, while Maven paths and
/// manifest coordinates need the bare build. Repair has to normalize both.
/// </summary>
public sealed class LoaderVersionTextTests
{
    [Theory]
    [InlineData("forge", "1.20.1", "47.4.18", "47.4.18")]
    [InlineData("forge", "1.20.1", "1.20.1-47.4.18", "47.4.18")]
    [InlineData("forge", "1.20.1", "1.20.1-forge-47.4.18", "47.4.18")]
    [InlineData("forge", "1.20.1", "forge-47.4.18", "47.4.18")]
    [InlineData("forge", "1.20.1", "forge-1.20.1-47.4.18", "47.4.18")]
    [InlineData("neoforge", "1.21.1", "21.1.248", "21.1.248")]
    [InlineData("neoforge", "1.21.1", "1.21.1-neoforge-21.1.248", "21.1.248")]
    [InlineData("neoforge", "1.20.1", "1.20.1-forge-47.1.106", "47.1.106")]
    [InlineData("neoforge", "", "1.20.1-47.1.106", "47.1.106")]
    [InlineData("fabric", "1.21.8", "0.18.4", "0.18.4")]
    [InlineData("fabric", "1.21.8", "fabric-loader-0.18.4", "0.18.4")]
    [InlineData("fabric", "1.21.8", "fabric-loader-0.18.4-1.21.8", "0.18.4")]
    [InlineData("quilt", "1.21.1", "quilt-loader-0.27.1-1.21.1", "0.27.1")]
    public void Normalize_strips_profile_and_game_prefixes(
        string loader,
        string game,
        string value,
        string expected)
    {
        Assert.Equal(expected, LoaderVersionText.Normalize(loader, game, value));
    }

    [Theory]
    [InlineData("1.20.1-47.2.0", "1.20.1", "47.2.0")]
    [InlineData("1.20.1-forge-47.1.7", "1.20.1", "47.1.7")]
    [InlineData("1.12.2-14.23.5.2860", "1.12.2", "14.23.5.2860")]
    [InlineData("20.3.8-beta", null, null)]
    [InlineData("21.1.265", null, null)]
    public void Only_the_1x_game_line_is_game_prefixed(
        string version,
        string? game,
        string? build)
    {
        var matched = LoaderVersionText.TrySplitGamePrefixed(
            version,
            out var actualGame,
            out var actualBuild);

        Assert.Equal(game is not null, matched);
        if (game is not null)
        {
            Assert.Equal(game, actualGame);
            Assert.Equal(build, actualBuild);
        }
    }

    [Fact]
    public void Forge_installer_url_accepts_the_profile_id_stored_by_imports()
    {
        var path = ForgeMavenUrls.InstallerPath(
            LoaderKind.Forge,
            "1.20.1",
            "1.20.1-forge-47.4.18");

        Assert.Equal(
            "net/minecraftforge/forge/1.20.1-47.4.18/" +
            "forge-1.20.1-47.4.18-installer.jar",
            path);
    }

    [Fact]
    public void Legacy_neoforge_installer_uses_the_forge_artifact()
    {
        var path = ForgeMavenUrls.InstallerPath(LoaderKind.NeoForge, "1.20.1", "47.1.106");

        Assert.Equal(
            "net/neoforged/forge/1.20.1-47.1.106/" +
            "forge-1.20.1-47.1.106-installer.jar",
            path);
    }

    [Fact]
    public void Modern_neoforge_installer_drops_the_game_prefix()
    {
        var path = ForgeMavenUrls.InstallerPath(LoaderKind.NeoForge, "1.21.1", "21.1.265");

        Assert.Equal(
            "net/neoforged/neoforge/21.1.265/neoforge-21.1.265-installer.jar",
            path);
    }
}
