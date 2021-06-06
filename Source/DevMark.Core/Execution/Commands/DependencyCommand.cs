using System;
using System.Collections;
using System.Collections.Generic;

namespace DevMark
{
    public class DependencyCommand : CommandBase
    {
        public override void Initialize(CommandConfigurationBase command, string workDir, bool traceLogging)
        {
            base.Initialize(command, workDir, traceLogging);

            if (command is DependencyCommandConfiguration)
            {
                var cmd = command as DependencyCommandConfiguration;
                Script = GetScript(cmd);
                Params = GetParams(cmd);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        protected virtual string GetScript(DependencyCommandConfiguration cmd)
        {
            string script;

            if (cmd.VisualStudio != null)
            {
                script = GetEmbeddedScript("Dependencies\\VisualStudio.ps1");
            }
            else if (cmd.NodeJS != null)
            {
                script = GetEmbeddedScript("Dependencies\\NodeJS.ps1"); 
            }
            else if (cmd.Git != null)
            {
                script = GetEmbeddedScript("Dependencies\\Git.ps1");
            }
            else if (cmd.DevMark != null)
            {
                script = GetEmbeddedScript("Dependencies\\DevMark.ps1");
            }
            else if (cmd.Dotnet != null)
            {
                script = GetEmbeddedScript("Dependencies\\Dotnet.ps1");
            }
            else if (cmd.DotnetFramework != null)
            {
                script = GetEmbeddedScript("Dependencies\\DotnetFramework.ps1");
            }
            else if (cmd.Path != null)
            {
                script = GetEmbeddedScript("Dependencies\\Path.ps1");
            }
            else
            {
                throw new ArgumentException();
            }

            return ExpandInlineScripts(script);
        }

        protected override Dictionary<string, object> GetParams(CommandConfigurationBase command)
        {
            Dictionary<string, object> parameters = base.GetParams(command);

            var cmd = command as DependencyCommandConfiguration;
            var vs = cmd.VisualStudio;

            var path = cmd.Path;
            if (path != null)
            {
                parameters.Add("RequiredLength", path.RequiredLength);

            }

            if (vs != null)
            {
                parameters.Add("MinVersion", vs.MinVersion);
                parameters.Add("MaxVersion", vs.MaxVersion);
                parameters.Add("RequiredComponents", vs.Components);
            }

            var git = cmd.Git;
            if (git != null)
            {

                parameters.Add("MinVersion", git.MinVersion);
                parameters.Add("MaxVersion", git.MaxVersion);
            }

            var node = cmd.NodeJS;
            if (node != null)
            {
                parameters.Add("npm", node.NPM);
                parameters.Add("NativeCppModules", node.NativeCppModules == true);
                parameters.Add("NativePythonModules", node.NativePythonModules == true);
                parameters.Add("MinVersion", node.MinVersion);
                parameters.Add("MaxVersion", node.MaxVersion);
                
            }

            var devMark = cmd.DevMark;
            if (devMark != null)
            {
                parameters.Add("Version", GetType().Assembly.GetName().Version.ToString());
                parameters.Add("MinVersion", devMark.MinVersion);
                parameters.Add("MaxVersion", devMark.MaxVersion);

            }

            var dotnet = cmd.Dotnet;
            if (dotnet != null)
            {
                parameters.Add("config", dotnet);

            }

            var dotnetFramework = cmd.DotnetFramework;
            if (dotnetFramework != null)
            {
                parameters.Add("MinVersion", dotnetFramework.MinVersion);
                parameters.Add("MaxVersion", dotnetFramework.MaxVersion);
            }

            return parameters;
        }

        protected override bool ProcessCommandResult()
        {
            EnvironmentVariables = CommandResult["Environment"] as Hashtable ?? new Hashtable();
            Dependencies = CommandResult["Dependencies"] as Hashtable ?? new Hashtable();
            return true;
        }

        public Hashtable EnvironmentVariables { get; set; }
        public Hashtable Dependencies { get; private set; }
        public DateTime? Timestamp { get; private set; }
    }
}
