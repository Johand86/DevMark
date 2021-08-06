using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace DevMark.Core.Execution
{
    public class CommandLogger
    {
        protected readonly ILogger<CommandLogger> _logger;

        public CommandLogger(ILogger<CommandLogger> logger)
        {
            _logger = logger;
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

        public void LogAfterExecution(CommandBase command)
        {
            foreach (var streamObj in command.Streams.Where(x => x.Severity == StreamSeverty.Warn || x.Severity == StreamSeverty.Error))
            {
                LogOutputStreamObject(streamObj);

                if (streamObj?.Message?.ToString()?.Contains("error during connect") == true)
                {
                    _logger.LogWarning("Failed to connect to DockerEngine, make sure the service it's running on your local machine.");
                }
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
    }
}
