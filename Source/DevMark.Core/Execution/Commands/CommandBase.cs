using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DevMark
{
    public abstract class CommandBase
    {
        public CommandBase()
        {
            Streams = new List<StreamObject>();
            Pipeline = new List<object>();
        }

        public event EventHandler<StreamObject> StreamDataAdded;

        public virtual void Initialize(CommandConfigurationBase command, string workDir, bool traceLogging)
        {
            WorkDir = workDir;
            TraceLogging = traceLogging;
        }

        public virtual void Complete()
        {
            if (Int32.TryParse(TryGetReturnValue("ExitCode"), out int exitCode))
            {
                ExitCode = exitCode;
            }

            if (Int64.TryParse(TryGetReturnValue("Duration"), out long duration))
            {
                Duration = TimeSpan.FromTicks(duration);
            }

            var returnValueInPipeline = Pipeline.LastOrDefault() as Hashtable;
            if (returnValueInPipeline != null)
            {
                Pipeline.Remove(returnValueInPipeline);
            }
            CommandResult = returnValueInPipeline ?? new Hashtable();

            bool processedReturnValue = ProcessCommandResult();
            bool errorsInStreamOutput = HasErrorsInOutputStream();

            if (Exception == null && IsSuccessExitCode(ExitCode) && processedReturnValue && errorsInStreamOutput == false)
            {
                Successfull = true;
            }
            Executed = true;
        }

        protected void InvokeStreamDataAdded(object source, StreamObject streamObject)
        {
            if (StreamDataAdded != null)
                StreamDataAdded.Invoke(this, streamObject);
        }

        protected virtual bool HasErrorsInOutputStream()
        {
            return Streams.Any(x => x.Severity == StreamSeverty.Error);
        }

        protected virtual bool ProcessCommandResult()
        {
            return true;
        }

        protected string TryGetReturnValue(string key)
        {
            var lastPipelineObject = Pipeline.LastOrDefault() as System.Collections.Hashtable;
            if (lastPipelineObject != null)
            {
                if (lastPipelineObject.ContainsKey(key))
                {
                    return lastPipelineObject[key]?.ToString();
                }
            }
            return null;
        }

        protected virtual bool IsSuccessExitCode(int? exitCode)
        {
            return exitCode == 0 || exitCode == null;
        }

        protected virtual string GetEmbeddedScript(string name)
        {
            var assembly = typeof(GitCommand).GetTypeInfo().Assembly;
            var asmName = Path.GetFileNameWithoutExtension(assembly.Location);
            using (Stream resource = assembly.GetManifestResourceStream($"{asmName}.Scripts.{name.Replace("\\",".").Replace("//",".")}"))
            {
                using (StreamReader sr = new StreamReader(resource))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        protected string ExpandInlineScripts(string script)
        {
            string outputScript = script;
            var inlineScriptRegex = new Regex(@"(?<InlineScript>##<(?<Prefix>(func:)?)(?<InlineScriptName>([:\w|\s|\/|\\|\.]*))>##)");
            foreach (Match match in inlineScriptRegex.Matches(script))
            {
                var scriptFullTag = match.Groups["InlineScript"].Value;
                var scriptName = match.Groups["InlineScriptName"].Value;
                var prefix = match.Groups["Prefix"].Value;
                string embeddedScript = GetEmbeddedScript(scriptName);

                if (string.Compare(prefix, "func:", true) == 0)
                {
                    string funcName = Path.GetFileNameWithoutExtension(scriptName);
                    embeddedScript = WrapScriptInFunction(funcName, embeddedScript);
                }

                outputScript = outputScript.Replace(scriptFullTag, embeddedScript);
            }
            return outputScript;
        }

        private string WrapScriptInFunction(string name, string script)
        {
            return $"function {name} {{\r\n{script}}}\r\n";
        }

        protected virtual Dictionary<string, object> GetParams(CommandConfigurationBase command)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("workDir", WorkDir);
            parameters.Add("trace", TraceLogging);

            return parameters;
        }

        public virtual void AddToStream(StreamSeverty severity, object msg)
        {
            var streamObject = new StreamObject { Timestamp = DateTime.Now, Severity = severity, Message = msg };
            Streams.Add(streamObject);
            StreamDataAdded?.Invoke(this, streamObject);
        }

        public TimeSpan? Duration { get; set; }
        public bool Executed { get; set; }
        public int? ExitCode { get; set; }
        public bool Successfull { get; set; }
        public bool TraceLogging { get; set; }
        [JsonIgnore]
        public List<StreamObject> Streams { get; protected set; }
        [JsonIgnore]
        public List<Object> Pipeline { get; set; }
        public Hashtable CommandResult { get; set; }
        public string WorkDir { get; set; }
        public Dictionary<string, object> Params { get; protected set; }
        public string Script { get; protected set; }
        public RuntimeException Exception { get; set; }
    }
}
