namespace vaultconfiguration.tests
{
    public class ImmutablePath
    {
        private readonly string _path;

        public ImmutablePath(string path)
        {
            _path = path;
        }

        public ImmutablePath AppendChild(string path)
        {
            var newPath = $"{_path.Trim('/')}/{path.Trim('/')}";
            return new ImmutablePath(newPath);
        }

        public string ToVaultPath()
        {
            return _path;
        }

        public string ToConfigurationPath()
        {
            return _path.Replace('/', ':');
        }
    }
}