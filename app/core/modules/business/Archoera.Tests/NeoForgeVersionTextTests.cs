using Archoera.Domain.Loaders;

namespace Archoera.Tests;

public sealed class NeoForgeVersionTextTests
{
    [Theory]
    [InlineData("1.20.1-47.1.106", "1.20.1", "47.1.106")]
    [InlineData("1.20.1-forge-47.1.106", "1.20.1", "47.1.106")]
    [InlineData("20.2.93", "1.20.2", "20.2.93")]
    [InlineData("20.4.253", "1.20.4", "20.4.253")]
    [InlineData("21.0.167", "1.21", "21.0.167")]
    [InlineData("21.1.265", "1.21.1", "21.1.265")]
    [InlineData("21.1.99-beta", "1.21.1", "21.1.99-beta")]
    [InlineData("21.3.100", "1.21.3", "21.3.100")]
    [InlineData("26.1.2.126", "26.1.2", "26.1.2.126")]
    [InlineData("26.1.0.5", "26.1", "26.1.0.5")]
    [InlineData("26.3.0.99", "26.3", "26.3.0.99")]
    [InlineData("26.1.0.0-alpha.1+snapshot-1", "26.1-snapshot-1", "26.1.0.0-alpha.1+snapshot-1")]
    [InlineData("0.25w14craftmine.3-beta", "25w14craftmine", "0.25w14craftmine.3-beta")]
    public void Published_versions_decode_to_their_game_version(
        string version,
        string gameVersion,
        string loaderVersion)
    {
        Assert.True(NeoForgeVersionText.TryResolve(version, out var game, out var build));
        Assert.Equal(gameVersion, game);
        Assert.Equal(loaderVersion, build);
    }

    [Theory]
    [InlineData("")]
    [InlineData("47.1.106")]
    [InlineData("1.20.1")]
    [InlineData("not-a-version")]
    public void Unrecognized_versions_are_rejected(string version)
        => Assert.False(NeoForgeVersionText.TryResolve(version, out _, out _));
}
