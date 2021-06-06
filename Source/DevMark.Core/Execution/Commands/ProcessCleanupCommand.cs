using System;
using System.Collections.Generic;

namespace DevMark
{
    public class ProcessCleanupCommand : CommandBase
    {
        public override void Initialize(CommandConfigurationBase command, string workDir, bool traceLogging)
        {
            base.Initialize(command, workDir, traceLogging);

            if (command is CommandConfiguration)
            {
                var cmd = command as CommandConfiguration;

                if (cmd.Process == null)
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

            parameters.Add("killWorkDirChildren", cmd.Process.KillWorkDirChildren == true);

            return parameters;
        }

        protected virtual string GetScript(CommandConfiguration command)
        {
            return ExpandInlineScripts(GetEmbeddedScript("ProcessCleanup.ps1"));
        }
    }
}
