namespace VaultConfiguration
{
    public class TokenAuthenticationProvider
    {
        private readonly string _token;

        public TokenAuthenticationProvider(string token)
        {
            _token = token;
        }

        public string GetToken()
        {
            return _token;
        }
    }
}