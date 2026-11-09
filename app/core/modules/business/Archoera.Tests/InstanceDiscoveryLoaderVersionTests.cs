using Archoera.Infrastructure.Instances;

namespace Archoera.Tests;

public sealed class InstanceDiscoveryLoaderVersionTests
{
    [Fact]
    public void Sjmcl_profile_id_is_reduced_to_the_loader_build()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "archoera-discovery-loader",
            Guid.NewGuid().ToString("N"));
        try
        {
            var instance = Path.Combine(root, "versions", "幻梦境");
            Directory.CreateDirectory(Path.Combine(instance, "mods"));
            File.WriteAllText(
                Path.Combine(instance, "sjmclcfg.json"),
                """
                {
                  "name": "幻梦境",
                  "version": "1.20.1",
                  "modLoader": {
                    "loaderType": "Forge",
                    "version": "1.20.1-forge-47.4.18"
                  }
                }
                """);

            var record = Assert.Single(InstanceDiscovery.Discover(root));

            Assert.Equal("forge", record.Loader);
            Assert.Equal("1.20.1", record.GameVersion);
            Assert.Equal("47.4.18", record.LoaderVersion);
        }
        finally
        {
            try
            {
                Directory.Delete(root, recursive: true);
            }
            catch
            {
                // best effort
            }
        }
    }

    [Fact]
    public void Legacy_neoforge_profile_is_not_misreported_as_forge()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "archoera-discovery-loader",
            Guid.NewGuid().ToString("N"));
        try
        {
            var instance = Path.Combine(root, "versions", "legacy-neoforge");
            Directory.CreateDirectory(instance);
            File.WriteAllText(
                Path.Combine(instance, "legacy-neoforge.json"),
                """
                {
                  "id": "1.20.1-forge-47.1.106",
                  "inheritsFrom": "1.20.1",
                  "mainClass": "cpw.mods.bootstraplauncher.BootstrapLauncher",
                  "libraries": [
                    { "name": "net.neoforged:forge:1.20.1-47.1.106:universal" }
                  ]
                }
                """);

            var record = Assert.Single(InstanceDiscovery.Discover(root));

            // net.neoforged:forge is the 1.20.1 NeoForge line, not Forge.
            Assert.Equal("neoforge", record.Loader);
            Assert.Equal("1.20.1", record.GameVersion);
            Assert.Equal("47.1.106", record.LoaderVersion);
        }
        finally
        {
            try
            {
                Directory.Delete(root, recursive: true);
            }
            catch
            {
                // best effort
            }
        }
    }
}
