using Solid.ServiceMonitor.Abstractions.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.ServiceMonitor.Providers
{
    public class ArgumentsProvider : IArgumentsProvider
    {
        private string[] _arguments;

        public ArgumentsProvider(string[] arguments)
        {
            _arguments = arguments;
        }
        public string ServiceName => _arguments.ElementAtOrDefault(0);

        public string ApplicationPoolName => _arguments.ElementAtOrDefault(1) ?? "DefaultAppPool";
    }
}
