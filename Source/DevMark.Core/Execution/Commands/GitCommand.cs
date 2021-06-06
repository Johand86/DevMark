using System;
using System.Collections.Generic;

namespace DevMark
{
    public class GitCommand : CommandBase
    {
        public override void Initialize(CommandConfigurationBase command, string workDir, bool traceLogging)
        {
            base.Initialize(command, workDir, traceLogging);

            if (command is CommandConfiguration)
            {
                var cmd = command as CommandConfiguration;

                if (cmd.Git == null)
                    throw new ArgumentException();

                Params = GetParams(cmd);
                Script = GetScript(cmd);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        protected override Dictionary<string, object> GetParams(CommandConfigurationBase command)
        {
            Dictionary<string, object> parameters = base.GetParams(command);

            var cmd = command as CommandConfiguration;

            parameters.Add("repository", cmd.Git.Repository);
            parameters.Add("branch", cmd.Git.Branch);
            parameters.Add("clean", cmd.Git.Clean != false);
            parameters.Add("allowLockedFiles", cmd.Git.AllowLockedFiles == true);
            parameters.Add("recursive", cmd.Git.Recursive != false);

            return parameters;
        }

        protected virtual string GetScript(CommandConfiguration command)
        {
            return ExpandInlineScripts(GetEmbeddedScript("Git.ps1"));
        }

        public override void AddToStream(StreamSeverty severity, object msg)
        {
            StreamSeverty gitSeverity = StreamSeverty.Info;
            string m = msg?.ToString() ?? "";
            if (m.StartsWith("warning:"))
                gitSeverity = StreamSeverty.Warn;
            if (m.StartsWith("error:"))
                gitSeverity = StreamSeverty.Error;

            var streamObject = new StreamObject { Timestamp = DateTime.Now, Severity = gitSeverity, Message = m };
            Streams.Add(streamObject);
            InvokeStreamDataAdded(this, streamObject);
        }

    }
}
