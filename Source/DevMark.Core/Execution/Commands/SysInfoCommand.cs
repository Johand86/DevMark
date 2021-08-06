using DevMark.Model.SystemInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DevMark
{
    public class SysInfoCommand : CommandBase
    {
        public void Initialize(string platform, bool isContainer, bool traceLogging)
        {
            TraceLogging = traceLogging;
            Params = GetParams();
            if (isContainer)
            {
                Script = ExpandInlineScripts(GetEmbeddedScript("ContainerSysInfo.ps1"));
            }
            else
            {
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

            var containerResult = CommandResult["ContainerInfo"] as Hashtable;
            if (containerResult != null)
            {
                var osInfo = containerResult["OS"] as Hashtable ?? new Hashtable();
                Result = new SysInfo
                {
                    Containers = new List<ContainerInfo> {
                        new ContainerInfo
                        {
                            OS = new OsInfo
                            {
                                Architecture = osInfo["Architecture"] as string,
                                Name = osInfo["Name"] as string,
                                Platform = osInfo["Platform"] as string,
                                Version = osInfo["Version"] as string
                            },
                            LogicalProcessors = (int)containerResult["LogicalProcessors"],
                            Memory = (long)containerResult["Memory"]
                        }
                    }
                };
            }

            return Result != null;
        }
    
        public SysInfo Result { get; private set; }
    }
}
