using DevMark.Core.Execution;
using DevMark.Model.SystemInformation;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;

namespace DevMark.Core.SystemInformation
{
    public class SysInfoProvider
    {
        private readonly bool _isContainer;
        private readonly CommandFactory _commandFactory;
        private readonly ILogger<SysInfoProvider> _logger;
        private readonly CommandLogger _commandLogger;

        public SysInfoProvider(CommandFactory commandFactory, CommandLogger commandLogger, bool isContainer, ILogger<SysInfoProvider> logger)
        {
            _commandFactory =  commandFactory;
            _commandLogger = commandLogger;
            _isContainer = isContainer;
            _logger = logger;
        }

        public SysInfo Get()
        {
            var command = ExecuteSysInfoCommand(GetOsPlatform(), _isContainer);
           
            if (!command.Successfull)
            {
                throw new ApplicationException("Failed to get SysInfo");
            }
            return command.Result;
        }

        public SysInfoCommand ExecuteSysInfoCommand(string platform, bool isContainer)
        {
            using (ScriptExecuter executer = new ScriptExecuter())
            {
                executer.InitializeShell();
                var sysInfo = _commandFactory.CreateSysInfo(platform, isContainer, new CommandEngineConfiguration { TraceLogging = false });
                executer.RunScript(sysInfo);
                _commandLogger.LogAfterExecution(sysInfo);
                return sysInfo;
            }
        }


        public string GetOsPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return nameof(OSPlatform.Windows);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return nameof(OSPlatform.Linux);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return nameof(OSPlatform.OSX);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                return nameof(OSPlatform.FreeBSD);

            throw new NotSupportedException();
        }

    }
}
