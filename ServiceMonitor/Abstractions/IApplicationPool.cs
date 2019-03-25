using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Solid.ServiceMonitor.Abstractions
{
    public interface IApplicationPool
    {
        Task UpdateEnvironmentVariablesAsync(CancellationToken cancellationToken);
    }
}
