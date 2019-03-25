using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Solid.ServiceMonitor.Abstractions;
using Solid.ServiceMonitor.Abstractions.Providers;
using Solid.ServiceMonitor.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Solid.ServiceMonitor
{
    class Program
    {        
        static ManualResetEvent _reset = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                var executable = AppDomain.CurrentDomain.FriendlyName;
                Console.WriteLine();
                Console.WriteLine($"USAGE: {executable} [windows service name]");
                Console.WriteLine($"       {executable} w3svc [application pool]");
                Console.WriteLine("Options:");
                Console.WriteLine("  windows service name    Name of the Windows service to monitor");
                Console.WriteLine("  application pool        Name of the application pool to monitor; defaults to DefaultAppPool");
                return;
            }

            var root = InitializeServices(args);
            using (var scope = root.CreateScope())
            {
                var source = new CancellationTokenSource();
                var program = scope.ServiceProvider.GetRequiredService<Program>();
                var task = program.MonitorServiceAsync(source.Token);
                task.ContinueWith(t => _reset.Set());
                Console.CancelKeyPress += (s, a) =>
                {
                    _reset.Set();
                    source.Cancel();
                };
                _reset.WaitOne();
                task.Wait();
            }
        }

        static IServiceProvider InitializeServices(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables(prefix: "SERVICEMONITOR__")
                .Build();
            return new ServiceCollection()
                .AddLogging(builder => builder.AddConsole())
                .AddSingleton<IArgumentsProvider>(p => new ArgumentsProvider(args))
                .AddSingleton<IConfiguration>(configuration)
                .AddScoped<IService, Service>()
                .AddScoped<IApplicationPool, ApplicationPool>()
                .AddScoped<Program>()
                .BuildServiceProvider();
        }

        private IService _service;
        private IApplicationPool _applicationPool;
        private IArgumentsProvider _argumentsProvider;
        private ILogger<Program> _logger;

        public Program(IService service, IApplicationPool applicationPool, IArgumentsProvider argumentsProvider, ILogger<Program> logger)
        {
            _service = service;
            _applicationPool = applicationPool;
            _argumentsProvider = argumentsProvider;
            _logger = logger;
        }

        public async Task MonitorServiceAsync(CancellationToken cancellationToken)
        {
            var name = _argumentsProvider.ServiceName;

            if (_argumentsProvider.ServiceName == "w3svc")
            {
                try
                {
                    await _service.StopAsync();
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Could not stop {name}");
                    return;
                }

                try
                {
                    await _applicationPool.UpdateEnvironmentVariablesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Failed to update IIS configuration");
                    return;
                }
            }

            try
            {
                await _service.StartAsync();
                await _service.MonitorAsync(new[]
                {
                    ServiceControllerStatus.Stopped,
                    ServiceControllerStatus.StopPending,
                    ServiceControllerStatus.Paused,
                    ServiceControllerStatus.PausePending
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                LogError(ex, $"Could not start {name}");
                return;
            }
            finally
            {
                await _service.StopAsync();
            }
        }

        void LogError(Exception ex, string message)
        {
            var error = Marshal.GetLastWin32Error();
            _logger.LogDebug($"Win32 error: {error}");
            _logger.LogCritical(message);
        }
    }
}
