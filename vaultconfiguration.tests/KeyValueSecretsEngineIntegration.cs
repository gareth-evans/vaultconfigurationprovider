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
    }
}