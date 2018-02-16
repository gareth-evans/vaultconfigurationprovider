using System.Linq;
using System.Text;

namespace VaultConfiguration
{
    public static class PathBuilder
    {
        public static string Combine(params string[] args)
        {
            var sb = new StringBuilder();

            foreach (var arg in args)
            {
                sb.Append(arg);
                if (!arg.EndsWith("/")) sb.Append("/");
            }

            return sb.ToString().TrimEnd('/');
        }

        public static string CreateConfigurationKey(params string[] args)
        {
            var tokens = args.SelectMany(x => x.Trim('/').Split('/'));

            return string.Join(":", tokens);
        }
    }
}