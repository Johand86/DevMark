
using System.Collections.Generic;

namespace DevMark
{
    public class DockerInfoCommand : DependencyCommand
    {
        public void Initialize(bool traceLogging)
        {
            TraceLogging = traceLogging;
            Params = GetParams();
            Script = ExpandInlineScripts(GetEmbeddedScript("Dependencies\\Docker.ps1"));  
        }

        Dictionary<string, object> GetParams()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            // For the info command we resolve it regardless of version, and then make a decision about compatibility.
            parameters.Add("MinVersion", 0);
            parameters.Add("MaxVersion", 100);
            parameters.Add("trace", TraceLogging);
            return parameters;
        }

        protected override bool ProcessCommandResult()
        {
            string context = (CommandResult["Context"] as string ?? "").ToLower();

            if (context.Contains("windows"))
            {
                Platform = PlatformConfiguration.Windows;
            }
            else if (context.Contains("linux"))
            {
                Platform = PlatformConfiguration.Linux;
            }
            else
            {
                // In newer versions context may contain the string "default". We'll use Engine config platform instead:

                string engineOs = (CommandResult["EngineOs"] as string ?? "").ToLower();
                if (engineOs.Contains("windows"))
                {
                    Platform = PlatformConfiguration.Windows;
                }
                else if (engineOs.Contains("linux"))
                {
                    Platform = PlatformConfiguration.Linux;
                }
            }

            return base.ProcessCommandResult();
        }
    
        public PlatformConfiguration Platform { get; private set; }

        public string Version {  get { return Dependencies["docker"] as string; } }
    }
}
