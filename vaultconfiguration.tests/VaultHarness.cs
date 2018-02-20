using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace vaultconfiguration.tests
{
    public class VaultHarness : IDisposable
    {
        private Process _vaultProcess;
        private bool _disposed;
        private StringBuilder _outputStringBuilder = new StringBuilder();

        public VaultHarness()
        {
            if (_vaultProcess != null) throw new InvalidOperationException("Vault is already started");

            var port = GetFreeTcpPort();
            var hostname = "127.0.0.1";

            VaultAddress = new Uri($"http://{hostname}:{port}");

            RootTokenId = Guid.NewGuid().ToString();

            var startInfo =
                new ProcessStartInfo("c:\\tools\\vault\\vault.exe")
                {
                    Arguments = $"server -dev -dev-root-token-id={RootTokenId} -dev-listen-address={hostname}:{port}",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false                
                };



            _vaultProcess = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };
            _vaultProcess.Exited += ThrowOnExit;
            _vaultProcess.ErrorDataReceived += VaultProcess_DataReceived;
            _vaultProcess.OutputDataReceived += VaultProcess_DataReceived;

            _vaultProcess.Start();
            _vaultProcess.BeginOutputReadLine();
            _vaultProcess.BeginErrorReadLine();

            StartMonitoringProcess();
        }

        private void VaultProcess_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;

            _outputStringBuilder.AppendLine(e.Data);
        }

        private void ThrowOnExit(object sender, EventArgs eventArgs)
        {
            var errorOutput = _outputStringBuilder.ToString();

            throw new ProcessExitedException(
                $"Vault process ended unexpectedly with exit code {_vaultProcess.ExitCode}:{Environment.NewLine}{errorOutput}");
        }

        public Uri VaultAddress { get; }

        public string RootTokenId { get; }

        public void Dispose()
        {
            if (_disposed) return;

            if (_vaultProcess != null)
            {
                _vaultProcess.Exited -= ThrowOnExit;
                _vaultProcess.ErrorDataReceived -= VaultProcess_DataReceived;
                _vaultProcess.OutputDataReceived -= VaultProcess_DataReceived;

                _vaultProcess.Kill();
                _vaultProcess.Dispose();
                _vaultProcess = null;
            }

            _disposed = true;
        }

        private void StartMonitoringProcess()
        {
            var currentProcess = Process.GetCurrentProcess();

            Process.Start("powershell", $"wait-process -id {currentProcess.Id}; get-process -id {_vaultProcess.Id} | stop-process");
        }

        private static int GetFreeTcpPort()
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;

            tcpListener.Stop();
            return port;
        }
    }
}
