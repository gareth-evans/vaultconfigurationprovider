using System;
using Xunit;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace vault_configuration
{
    public class UnitTest1
    {
        private const string rootTokenPattern = @"Root Token: (?<rootToken>[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})";
        private Regex rootTokenRegex = new Regex(rootTokenPattern);

        [Fact]
        public void Test1()
        {
            var startInfo = new ProcessStartInfo("vault.exe");
            startInfo.Arguments = "server -dev";
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;

            var proc = Process.Start(startInfo);
            Match match = null;
            do
            {
                var line = proc.StandardOutput.ReadLine();
                //if(line == null) continue;

                match = rootTokenRegex.Match(line);
            } while (match?.Success != true);

            Console.WriteLine($"Root token = {match.Groups[1]}");
        }
    }
}
