using Microsoft.Extensions.Logging;
using Solid.ServiceMonitor.Abstractions;
using Solid.ServiceMonitor.Abstractions.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Solid.ServiceMonitor
{
    class Service : IService
    {
        private string _name;
        private ServiceController _controller;
        private ILogger<Service> _logger;

        public Service(IArgumentsProvider argumentsProvider, ILogger<Service> logger)
        {
            _logger = logger;
            var name = argumentsProvider.ServiceName;
            _name = name;
            _controller = new ServiceController(name);
        }

        public async Task MonitorAsync(ServiceControllerStatus[] invalidStatus, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Monitoring {_name} until it enters state: {string.Join(" or ", invalidStatus.Select(s => s.ToString()))}");
            while(!cancellationToken.IsCancellationRequested)
            {
                _controller.Refresh();
                if (invalidStatus.Contains(_controller.Status))
                {
                    _logger.LogInformation($"Service {_name} entered state {_controller.Status}");
                    return;
                }

                await Task.Delay(2 * 1000, cancellationToken);
            }
        }

        public Task StartAsync(TimeSpan? timeout = null)
        {
            timeout = timeout ?? TimeSpan.FromSeconds(20);

            _controller.Refresh();
            if (_controller.Status != ServiceControllerStatus.Running)
            {
                _logger.LogInformation($"Staring {_name}");
                _controller.Start();
                _controller.WaitForStatus(ServiceControllerStatus.Running, timeout.Value);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(TimeSpan? timeout = null)
        {
            timeout = timeout ?? TimeSpan.FromSeconds(20);

            _controller.Refresh();
            if (_controller.Status != ServiceControllerStatus.Stopped)
            {
                _logger.LogInformation($"Stopping {_name}");
                _controller.Stop();
                _controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout.Value);
            }
            return Task.CompletedTask;
        }
    }
}
