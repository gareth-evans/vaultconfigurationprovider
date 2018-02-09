namespace VaultConfiguration
{
    public class Options
    {
        
    }

    public interface ISecretsEngine
    {
        string Path { get; }
    }

    public class KeyValueSecretsEngine : ISecretsEngine
    {
        public KeyValueSecretsEngine(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }

    
}
