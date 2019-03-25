using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Solid.ServiceMonitor.Abstractions
{
    public interface IService
    {
        Task StartAsync(TimeSpan? timeout = null);
        Task StopAsync(TimeSpan? timeout = null);
        Task MonitorAsync(ServiceControllerStatus[] invalidStatus, CancellationToken cancellationToken);
    }
}
