using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.ServiceMonitor.Abstractions.Providers
{
    public interface IArgumentsProvider
    {
        string ServiceName { get; }
        string ApplicationPoolName { get; }
    }
}
