using System;
using System.Collections.Generic;

namespace DevMark
{
    public class ShellCommand : CommandBase
    {
        public override void Initialize(CommandConfigurationBase command, string workDir, bool traceLogging)
        {
            base.Initialize(command, workDir, traceLogging);

            if (command is CommandConfiguration)
            {
                var cmd = command as CommandConfiguration;

                if (cmd.Shell == null)
                    throw new ArgumentException();

                Params = GetParams(cmd);
                Script = ExpandInlineScripts(GetEmbeddedScript("ShellCommand.ps1"));
            }
            else
            {
                throw new ArgumentException();
            }
        }

        protected override Dictionary<string, object> GetParams(CommandConfigurationBase command)
        {
            Dictionary<string, object> parameters = base.GetParams(command);

            parameters.Add("cmd", (command as CommandConfiguration).Shell.Cmd);

            return parameters;
        }

        protected override bool ProcessCommandResult()
        {
            if (CommandResult["Timestamp"] is DateTime)
            {
                Timestamp = (DateTime)CommandResult["Timestamp"];
            }
            return Timestamp != null;
        }


        public override void AddToStream(StreamSeverty severity, object msg)
        {
            StreamSeverty actualSeverity = severity;
            if (TraceLogging)
            {
                //NPM and git writes to std err. We dont want it to crash when verbose logging is enabled.
                actualSeverity = StreamSeverty.Info;
            }

            var streamObject = new StreamObject { Timestamp = DateTime.Now, Severity = actualSeverity, Message = msg };
            Streams.Add(streamObject);
            InvokeStreamDataAdded(this, streamObject);
        }

        public DateTime? Timestamp { get; private set; }
    }
}
