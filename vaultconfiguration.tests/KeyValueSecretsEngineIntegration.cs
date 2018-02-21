using System.Text;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.Configuration;
using VaultConfiguration;
using Xunit;

namespace vaultconfiguration.tests
{
    public class KeyValueSecretsEngineIntegration : IClassFixture<VaultHarness>
    {
        private static Faker _faker = new Faker();
        private readonly VaultHarness _vaultHarness;

        public KeyValueSecretsEngineIntegration(VaultHarness vaultHarness)
        {
            _vaultHarness = vaultHarness;
        }

        [Fact]
        public async Task Should_read_simple_value()
        {
            var client = new VaultClient(_vaultHarness.VaultAddress, new TokenAuthenticationProvider(_vaultHarness.RootTokenId));

            var secret = new
            {
                property = _faker.Random.AlphaNumeric(10)
            };

            var path = CreateRandomPath();

            await client.WriteSecretAsync(path, secret);

            var configuration = new ConfigurationBuilder()
                .Add(new VaultConfigurationProvider(_vaultHarness.VaultAddress, _vaultHarness.RootTokenId, "secret"))
                .Build();

            var result = configuration.GetValue(path, "property");

            Assert.Equal(secret.property, result);
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
            var secretOnRoot = new { property1 = _faker.Random.AlphaNumeric(10) };
            var nestedSecret = new { property2 = _faker.Random.AlphaNumeric(10) };

            var client = new VaultClient(_vaultHarness.VaultAddress, new TokenAuthenticationProvider(_vaultHarness.RootTokenId));

            var parentPath = CreateRandomPath();
            var childPath = parentPath.AppendChild(_faker.Random.AlphaNumeric(10));
            
            await client.WriteSecretAsync(parentPath.ToVaultPath(), secretOnRoot);
            await client.WriteSecretAsync(childPath.ToVaultPath(), nestedSecret);

            var configuration = new ConfigurationBuilder()
                .Add(new VaultConfigurationProvider(_vaultHarness.VaultAddress, _vaultHarness.RootTokenId, "secret"))
                .Build();

            var rootValue = configuration.GetValue(parentPath, "property1");
            var nestedValue = configuration.GetValue(childPath, "property2");

            Assert.Equal(secretOnRoot.property1, rootValue);
            Assert.Equal(nestedSecret.property2, nestedValue);
        }

        [Fact]
        public async Task Should_support_combining_values_when_path_already_exists()
        {
            var secretOnRoot = new { property1 = "value1" };
            var nestedSecret = new { property2 = "value2" };

            var client = new VaultClient(_vaultHarness.VaultAddress, new TokenAuthenticationProvider(_vaultHarness.RootTokenId));

            var parentPath = CreateRandomPath();
            var childPath = parentPath.AppendChild("property1");

            await client.WriteSecretAsync(parentPath, secretOnRoot);
            await client.WriteSecretAsync(childPath, nestedSecret);

            var configuration = new ConfigurationBuilder()
                .Add(new VaultConfigurationProvider(_vaultHarness.VaultAddress, _vaultHarness.RootTokenId, "secret"))
                .Build();

            var secretOnRootValue = configuration.GetValue(parentPath, "property1");
            var nestedSecretValue = configuration.GetValue(childPath, "property2");

            Assert.Equal(secretOnRoot.property1, secretOnRootValue);
            Assert.Equal(nestedSecret.property2, nestedSecretValue);
        }

        private static ImmutablePath CreateRandomPath(string prefix = "secret")
        {
            return new ImmutablePath($"{prefix}/{_faker.Random.AlphaNumeric(10)}");
        }
    }

    public static class ImmutablePathExtensions
    {
        public static async Task WriteSecretAsync<T>(this VaultClient client, ImmutablePath path, T value)
        {
            await client.WriteSecretAsync(path.ToVaultPath(), value);
        }

        public static string GetValue(this IConfiguration configuration, ImmutablePath path, string keySuffix = null, string keyPrefix = "vault")
        {
            var sb = new StringBuilder();
            if (keyPrefix != null) sb.Append($"{keyPrefix}:");
            sb.Append(path.ToConfigurationPath());
            if (keyPrefix != null) sb.Append($":{keySuffix}");

            var key = sb.ToString();

            return configuration[key];
        }
    }
}