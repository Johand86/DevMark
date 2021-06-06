using DevMark.Model.SystemInformation;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DevMark.Core.SystemInformation
{
    public class SysInfoProvider
    {
        private readonly CommandFactory _commandFactory;
        private readonly ILogger<SysInfoProvider> _logger;

        public SysInfoProvider(CommandFactory commandFactory, ILogger<SysInfoProvider> logger)
        {
            _commandFactory =  commandFactory;
            _logger = logger;
        }

        public SysInfo Get()
        {
            var command = ExecuteSysInfoCommand(GetOsPlatform());
           
            if (!command.Successfull)
            {
                throw new ApplicationException("Failed to get SysInfo");
            }
            return command.Result;
        }

        public SysInfoCommand ExecuteSysInfoCommand(string platform)
        {
            using (ScriptExecuter executer = new ScriptExecuter())
            {
                executer.InitializeShell();
                var sysInfo = _commandFactory.CreateSysInfo(platform, new CommandEngineConfiguration { TraceLogging = false });
                executer.RunScript(sysInfo); 
                LogAfterExecution(sysInfo);
                return sysInfo;
            }
        }

        public void LogAfterExecution(CommandBase command)
        {
            foreach (var streamObj in command.Streams.Where(x => x.Severity == StreamSeverty.Warn || x.Severity == StreamSeverty.Error))
            {
                LogOutputStreamObject(streamObj);
            }
            
            foreach (var streamObj in command.Pipeline)
            {
                LogOutputStreamObject(new StreamObject { Timestamp = DateTime.Now, Severity = StreamSeverty.Info, Message = streamObj });
            }

            if (command.Exception != null)
            {
                _logger.LogError(command.Exception, "An exception occured while running the command.");
            }
        }

        public void LogOutputStreamObject(StreamObject streamObj)
        {
            switch (streamObj.Severity)
            {
                case StreamSeverty.Info:
                    _logger.LogInformation(streamObj.Message?.ToString());
                    break;
                case StreamSeverty.Warn:
                    _logger.LogWarning(streamObj.Message?.ToString());
                    break;
                case StreamSeverty.Error:
                    _logger.LogError(streamObj.Message?.ToString());
                    break;
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
