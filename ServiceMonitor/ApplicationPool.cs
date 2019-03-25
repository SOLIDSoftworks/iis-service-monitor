using Microsoft.Extensions.Logging;
using Solid.ServiceMonitor.Abstractions;
using Solid.ServiceMonitor.Abstractions.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Solid.ServiceMonitor
{
    public class ApplicationPool : IApplicationPool
    {
        private string _appPool;
        private ILogger<ApplicationPool> _logger;
        private Dictionary<string, string> _ignored = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "TMP", "C:\\Users\\ContainerAdministrator\\AppData\\Local\\Temp" },
            { "TEMP", "C:\\Users\\ContainerAdministrator\\AppData\\Local\\Temp" },
            { "USERNAME", "ContainerAdministrator" },
            { "USERPROFILE", "C:\\Users\\ContainerAdministrator" },
            { "APPDATA", "C:\\Users\\ContainerAdministrator\\AppData\\Roaming" },
            { "LOCALAPPDATA", "C:\\Users\\ContainerAdministrator\\AppData\\Local" },
            { "PROGRAMDATA", "C:\\ProgramData" },
            { "PSMODULEPATH", "%ProgramFiles%\\WindowsPowerShell\\Modules;C:\\Windows\\system32\\WindowsPowerShell\\v1.0\\Modules" },
            { "PUBLIC", "C:\\Users\\Public" },
            { "USERDOMAIN", "User Manager" },
            { "ALLUSERSPROFILE", "C:\\ProgramData" },
            { "PATHEXT", ".COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC" },
            { "PATH", null },
            { "COMPUTERNAME", null },
            { "COMSPEC", null },
            { "OS", null },
            { "PROCESSOR_IDENTIFIER", null },
            { "PROCESSOR_LEVEL", null },
            { "PROCESSOR_REVISION", null },
            { "PROGRAMFILES", null },
            { "PROGRAMFILES(X86)", null },
            { "PROGRAMW6432", null },
            { "SYSTEMDRIVE", null },
            { "WINDIR", null },
            { "NUMBER_OF_PROCESSORS", null },
            { "PROCESSOR_ARCHITECTURE", null },
            { "SYSTEMROOT", null },
            { "COMMONPROGRAMFILES", null },
            { "COMMONPROGRAMFILES(X86)", null },
            { "COMMONPROGRAMW6432", null },
            { "DRIVERDATA", null },
        };

        public ApplicationPool(IArgumentsProvider argumentsProvider, ILogger<ApplicationPool> logger)
        {
            _appPool = argumentsProvider.ApplicationPoolName;
            _logger = logger;
        }

        public async Task UpdateEnvironmentVariablesAsync(CancellationToken cancellationToken)
        {
            var variables = GetEnvironmentVariables();
            var remove = BuildAppCommandArguments(variables, CommandType.Remove);
            _logger.LogInformation($"Removing environment variables from {_appPool}");
            await RunCommandAsync(remove, allowFailure: true);
            var add = BuildAppCommandArguments(variables, CommandType.Add);
            _logger.LogInformation($"Adding environment variables to {_appPool}");
            await RunCommandAsync(add);
        }

        private async Task RunCommandAsync(string arguments, bool allowFailure = false)
        {
            var file = Path.Combine(Environment.SystemDirectory, @"inetsrv\appcmd.exe");
            var info = new ProcessStartInfo
            {
                FileName = file,
                Arguments = arguments,
                UseShellExecute = false                   
            };
            _logger.LogTrace("Starting appcmd process");
            await Task.Run(() =>
            {
                var process = Process.Start(info);
                try
                {
                    process.WaitForExit(1000);
                    if (process.ExitCode != 0 && !allowFailure)
                        throw new Exception("Error running command");
                    _logger.LogTrace("appcmd process finished");
                }
                catch
                {
                    if (!allowFailure) throw;
                }
            });
        }

        private string BuildAppCommandArguments(Dictionary<string, string> variables, CommandType type)
        {
            var builder = new StringBuilder();
            builder.Append("set config -section:system.applicationHost/applicationPools ");
            foreach(var variable in variables)
            {
                if (type == CommandType.Add)
                    builder.Append("/+");
                else
                    builder.Append("/-");

                builder.Append($"\"[name='{_appPool}'].environmentVariables.[name='{variable.Key}'");
                if (type == CommandType.Add)
                    builder.Append($",value='{variable.Value}'");
                builder.Append("]\" ");
            }
            builder.Append("/commit:apphost");
            var arguments = builder.ToString();
            _logger.LogDebug($"Arguments generated: {arguments}");
            return builder.ToString();
        }

        private Dictionary<string, string> GetEnvironmentVariables()
        {
            var variables = Environment.GetEnvironmentVariables();
            return variables
                .Keys                
                .Cast<string>()
                .Where(key => !_ignored.ContainsKey(key))
                .ToDictionary(k => k, k => variables[k]?.ToString(), StringComparer.OrdinalIgnoreCase);
        }

        enum CommandType
        {
            Add,
            Remove
        }
    }
}
