using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;

namespace DevMark
{
    public class ScriptExecuter : IDisposable
    {
        private PowerShell _shell;
        private RunspacePool _pool;
        private CommandBase _currentCommand;

        public void InitializeShell(int minRunspaces = 1, int maxRunspaces = 10, string[] loadModules = null)
        {
            if (_shell != null)
            {
                _shell.Streams.Error.DataAdded -= Error_DataAdded;
                _shell.Streams.Warning.DataAdded -= Warning_DataAdded;
                _shell.Streams.Information.DataAdded -= Information_DataAdded;
                _shell.Dispose();
            }

            if (_pool != null)
            {
                _pool.Dispose();
            }

            var defaultSessionState = InitialSessionState.CreateDefault();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                defaultSessionState.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.RemoteSigned;
            }

            if (loadModules != null)
            {
                foreach (var moduleName in loadModules)
                {
                    defaultSessionState.ImportPSModule(moduleName);
                }
            }

            _pool = RunspaceFactory.CreateRunspacePool(defaultSessionState);
            _pool.SetMinRunspaces(minRunspaces);
            _pool.SetMaxRunspaces(maxRunspaces);
            _pool.ThreadOptions = PSThreadOptions.UseNewThread;
            _pool.Open();

            _shell = PowerShell.Create();
            _shell.RunspacePool = _pool;
            _shell.Streams.Error.DataAdded += Error_DataAdded;
            _shell.Streams.Warning.DataAdded += Warning_DataAdded;
            _shell.Streams.Information.DataAdded += Information_DataAdded;

            EnvironmentVariables = new Hashtable();
        }

        public void RunScript(CommandBase command)
        {
            lock (_shell)
            {
                _currentCommand = command;

                if (command.WorkDir != null || command.Params.ContainsKey("workDir"))
                {
                    if (!Directory.Exists(command.WorkDir))
                    {
                        Directory.CreateDirectory(command.WorkDir);
                    }
                }

                try
                {

                    foreach (var key in EnvironmentVariables.Keys)
                    {
                        string script = $"$env:{key} = '{EnvironmentVariables[key]}';";
                       _shell.AddScript(script);
                    }

                    _shell.AddScript(command.Script);
                    _shell.AddParameters(command.Params);
                    var pipelineObjects = _shell.Invoke();
                    _currentCommand.Pipeline = pipelineObjects.Where(x => x != null).Select(x => x.BaseObject).ToList();
                    _shell.Commands.Clear();
                }
                catch (RuntimeException e)
                {
                    _currentCommand.Exception = e;
                }

                command.Complete();
            }
        }

        public Hashtable EnvironmentVariables { get; set; }

        private void Information_DataAdded(object sender, DataAddedEventArgs e)
        {
            var dataCollection = sender as PSDataCollection<InformationRecord>;
            var record = dataCollection[e.Index];

            _currentCommand?.AddToStream(StreamSeverty.Info, record.MessageData);
        }

        private void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            var dataCollection = sender as PSDataCollection<WarningRecord>;
            var record = dataCollection[e.Index];

            _currentCommand?.AddToStream(StreamSeverty.Warn, record.Message);
        }

        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            var dataCollection = sender as PSDataCollection<ErrorRecord>;
            var record = dataCollection[e.Index];

            _currentCommand?.AddToStream(StreamSeverty.Error, record.Exception);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _shell?.Dispose();
        }
    }
}
