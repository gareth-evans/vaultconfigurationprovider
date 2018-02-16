using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace VaultConfiguration
{
    using System.Linq;

    public class VaultConfigurationProvider : IConfigurationProvider, IConfigurationSource
    {
        private readonly string _rootPath;
        private readonly VaultClient _vaultClient;

        private readonly IDictionary<string,string> _secrets = new Dictionary<string, string>();

        public VaultConfigurationProvider(
            Uri baseAddress, 
            string token,
            string rootPath)
        {
            _rootPath = rootPath;
            _vaultClient = new VaultClient(baseAddress, new TokenAuthenticationProvider(token));
        }

        public bool TryGet(string key, out string value)
        {
            return this._secrets.TryGetValue(key, out value);
        }

        public void Set(string key, string value)
        {
            throw new System.NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            return new NullChangeToken();
        }

        public void Load()
        {
            ReadChildSecrets(_rootPath).Wait();
        }

        private async Task ReadChildSecrets(string parent)
        {
            var keys = _vaultClient.GetList($"{parent}?list=true").Result["data"]["keys"].Select(t => t.Value<string>()).ToList();

            foreach (var key in keys)
            {
                if (key.EndsWith("/"))
                {
                    await ReadChildSecrets($"{parent}/{key}");
                    continue;
                }

                var secret = _vaultClient.ReadSecretAsync(PathBuilder.Combine(parent, key)).Result;

                foreach (var property in secret.Data)
                {
                    this._secrets.Add(PathBuilder.CreateConfigurationKey("vault", parent, key, property.Key), property.Value.Value<string>());
                }
            }
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            throw new System.NotImplementedException();
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }

        private class NullChangeToken : IChangeToken
        {
            public IDisposable RegisterChangeCallback(Action<object> callback, object state)
            {
                return new Disposable();
            }

            public bool HasChanged { get; } = false;
            public bool ActiveChangeCallbacks { get; } = false;

            private class Disposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}