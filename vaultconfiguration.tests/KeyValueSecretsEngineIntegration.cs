using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VaultConfiguration;
using Xunit;

namespace vaultconfiguration.tests
{
    public class KeyValueSecretsEngineIntegration : IClassFixture<VaultHarness>
    {
        private readonly VaultHarness _vaultHarness;

        public KeyValueSecretsEngineIntegration(VaultHarness vaultHarness)
        {
            _vaultHarness = vaultHarness;
        }

        [Fact]
        public async Task Should_read_simple_value()
        {
            var client = new VaultClient(_vaultHarness.VaultAddress, new TokenAuthenticationProvider(_vaultHarness.RootTokenId));

            const string secret = "{ \"bar\": \"crux\" }";

            await client.WriteSecretAsync("secret/foo", secret);

            var configuration = new ConfigurationBuilder()
                .Add(new VaultConfigurationProvider(_vaultHarness.VaultAddress, _vaultHarness.RootTokenId, "secret"))
                .Build();

            var result = configuration["vault:secret:foo:bar"];

            Assert.Equal("crux", result);
        }

        [Fact]
        public async Task Should_read_secret_from_two_levels_deep()
        {
            var client = new VaultClient(_vaultHarness.VaultAddress, new TokenAuthenticationProvider(_vaultHarness.RootTokenId));

            const string secret = "{ \"bar\": \"crux\" }";

            await client.WriteSecretAsync("secret/parent/child", secret);

            var configuration = new ConfigurationBuilder()
                .Add(new VaultConfigurationProvider(_vaultHarness.VaultAddress, _vaultHarness.RootTokenId, "secret"))
                .Build();

            var result = configuration["vault:secret:parent:child:bar"];

            Assert.Equal("crux", result);
        }

        [Fact]
        public async Task Should_read_simple_and_nested_secret_on_the_same_path()
        {
            const string secretOnRoot = "{ \"property1\": \"value1\" }";
            const string nestedSecret = "{ \"property2\": \"value2\" }";

            var client = new VaultClient(_vaultHarness.VaultAddress, new TokenAuthenticationProvider(_vaultHarness.RootTokenId));

            await client.WriteSecretAsync("secret/parent", secretOnRoot);
            await client.WriteSecretAsync("secret/parent/child", nestedSecret);

            var configuration = new ConfigurationBuilder()
                .Add(new VaultConfigurationProvider(_vaultHarness.VaultAddress, _vaultHarness.RootTokenId, "secret"))
                .Build();

            var rootValue = configuration["vault:secret:parent:property1"];
            var nestedValue = configuration["vault:secret:parent:child:property2"];

            Assert.Equal("value1", rootValue);
            Assert.Equal("value2", nestedValue);
        }

        [Fact]
        public async Task Should_overwrite_values_when_path_already_exists()
        {
            const string secretOnRoot = "{ \"property1\": \"value1\" }";
            const string nestedSecret = "{ \"property2\": \"value2\" }";

            var client = new VaultClient(_vaultHarness.VaultAddress, new TokenAuthenticationProvider(_vaultHarness.RootTokenId));

            await client.WriteSecretAsync("secret/parent", secretOnRoot);
            await client.WriteSecretAsync("secret/parent/property1", nestedSecret);

            var configuration = new ConfigurationBuilder()
                .Add(new VaultConfigurationProvider(_vaultHarness.VaultAddress, _vaultHarness.RootTokenId, "secret"))
                .Build();

            var result = configuration["vault:secret:parent:property1"];

            Assert.Equal(nestedSecret, result);
        }
    }
}