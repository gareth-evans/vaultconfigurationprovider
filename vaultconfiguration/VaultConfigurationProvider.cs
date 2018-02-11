﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
        private Regex _regex;

        private IDictionary<string,string> _secrets = new Dictionary<string, string>();

        public VaultConfigurationProvider(
            Uri baseAddress, 
            string token,
            string rootPath)
        {
            _rootPath = rootPath;
            _vaultClient = new VaultClient(baseAddress, new TokenAuthenticationProvider(token));
            _regex = new Regex($"^vault:{rootPath}:(?<path>.*):(?<key>.*)$");
        }

        public bool TryGet(string key, out string value)
        {
            //var path = ResolvePathAndKey(key);

            //var result = _vaultClient.ReadSecretAsync(path.path).Result;

            //var jvalue = result.Data[path.key] as JValue;

            //value = jvalue.Value as string;

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
            // get list of keys
            var keys = _vaultClient.GetList("secret?list=true").Result["data"]["keys"].Select(t => t.Value<string>()).ToList();

            foreach (var key in keys)
            {
                var secret = _vaultClient.ReadSecretAsync($"secret/{key}").Result;

                foreach (var property in secret.Data)
                {
                    this._secrets.Add($"vault:secret:{key}:{property.Key}", property.Value.Value<string>());
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

        private (string path, string key) ResolvePathAndKey(string path)
        {
            var match = _regex.Match(path);
            var extractedPath = match.Groups[1].Value;

            var x = $"/{_rootPath.Trim('/')}/{extractedPath.Replace(':', '/')}";

            return (x, match.Groups[2].Value);
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