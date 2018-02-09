using System.Threading;
using System.Threading.Tasks;
using VaultConfiguration;
using Xunit;

namespace vaultconfiguration.tests
{
    public class BasicVaultInteraction : IClassFixture<VaultHarness>
    {
        private readonly VaultHarness _vaultHarness;

        public BasicVaultInteraction(VaultHarness vaultHarness)
        {
            _vaultHarness = vaultHarness;
        }

        [Fact]
        public async Task WritingAndReadValuesToVault()
        {
            var vaultClient = new VaultClient(_vaultHarness.VaultAddress,
                new TokenAuthenticationProvider(_vaultHarness.RootTokenId));

            const string foo = "{\"foo\":\"value\"}";
            await vaultClient.WriteSecretAsync("/secret/foo", foo);
            var response = await vaultClient.ReadSecretAsync("/secret/foo");
            dynamic data = response.Data;

            Assert.Equal("value", (string) data.foo);
        }
    }
}
