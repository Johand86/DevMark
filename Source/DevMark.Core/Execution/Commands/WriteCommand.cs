using System.Collections.Generic;

namespace DevMark
{
    public class WriteCommand : CommandBase
    {
        public override void Initialize(CommandConfigurationBase command, string workDir, bool traceLogging)
        {
            var commandConfig = command as CommandConfiguration;

            if (commandConfig != null && commandConfig.Write != null)
            {
                Params = new Dictionary<string, object> { { "Message", commandConfig.Write.Message } };
                Script = GetEmbeddedScript("Write.ps1");
            }
            base.Initialize(command, workDir, traceLogging);
        }
    }
}