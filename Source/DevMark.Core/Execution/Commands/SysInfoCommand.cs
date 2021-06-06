using DevMark.Model.SystemInformation;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DevMark
{
    public class SysInfoCommand : CommandBase
    {
        public void Initialize(string platform, bool traceLogging)
        {
            TraceLogging = traceLogging;
            Params = GetParams();
            switch (platform)
            {
                case nameof(OSPlatform.Windows):

                    Script = ExpandInlineScripts(GetEmbeddedScript("WindowsSysInfo.ps1"));
                    break;
                default:
                    Script = ExpandInlineScripts(GetEmbeddedScript("NotImplementedSysInfo.ps1"));
                    break;
            }
        }

        Dictionary<string, object> GetParams()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("trace", TraceLogging);
            return parameters;
        }

        protected override bool ProcessCommandResult()
        {
            Result = (CommandResult["SysInfo"] as System.Management.Automation.PSObject)?.BaseObject as SysInfo;
            return Result != null;
        }
    
        public SysInfo Result { get; private set; }
    }
}
