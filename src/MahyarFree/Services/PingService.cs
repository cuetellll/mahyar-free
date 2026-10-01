using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MahyarFree.Models;

namespace MahyarFree.Services
{
    public class PingService
    {
        public event EventHandler<string>? Log;

        public async Task<long> TcpPingAsync(string host, int port, int timeoutMs = 3000)
        {
            if (string.IsNullOrEmpty(host) || port <= 0) return -1;

            var sw = Stopwatch.StartNew();
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                var timeoutTask = Task.Delay(timeoutMs);

                var completed = await Task.WhenAny(connectTask, timeoutTask);
                if (completed == connectTask && client.Connected)
                {
                    sw.Stop();
                    return sw.ElapsedMilliseconds;
                }
                return -1;
            }
            catch (Exception ex)
            {
                Log?.Invoke(this, $"TCP ping failed for {host}:{port} - {ex.Message}");
                return -1;
            }
        }

        public async Task PingAllAsync(
            List<ProxyConfig> configs,
            Action<ProxyConfig>? onConfigUpdated = null,
            int maxParallel = 10,
            int pingTimeoutMs = 3000,
            CancellationToken cancellationToken = default)
        {
            var semaphore = new SemaphoreSlim(maxParallel);
            var tasks = new List<Task>();

            foreach (var config in configs)
            {
                if (cancellationToken.IsCancellationRequested) break;

                config.IsTesting = true;
                onConfigUpdated?.Invoke(config);

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await semaphore.WaitAsync(cancellationToken);
                        try
                        {
                            var ping = await TcpPingAsync(config.Address, config.Port, pingTimeoutMs);
                            config.Ping = ping;
                        }
                        finally { semaphore.Release(); }
                    }
                    finally
                    {
                        config.IsTesting = false;
                        onConfigUpdated?.Invoke(config);
                    }
                }, cancellationToken));
            }

            try { await Task.WhenAll(tasks); }
            catch (OperationCanceledException) { }
        }

        public ProxyConfig? GetBestConfig(List<ProxyConfig> configs)
        {
            return configs
                .Where(c => c.Ping > 0)
                .OrderBy(c => c.Ping)
                .FirstOrDefault();
        }
    }
}
