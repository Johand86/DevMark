using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DevMark
{
    public class CommandEngineConfiguration
    {
        public bool TraceLogging { get; set; }
    }

    public class CommandFactory
    {
        private readonly string _baseWorkDir;

        public CommandFactory(string workDir)
        {
            _baseWorkDir = workDir;
        }

        public CommandBase CreateFromConfiguration(TestSuiteConfiguration testSuit, TestConfiguration test, CommandConfigurationBase command, CommandEngineConfiguration engineConfig)
        {
            if (command is CommandConfiguration)
            {
                return GenerateFromConfiguration(testSuit, test, command as CommandConfiguration, engineConfig);
            }

            if (command is DependencyCommandConfiguration)
            {
                return GenerateFromConfiguration(testSuit, test, command as DependencyCommandConfiguration, engineConfig);
            }


            throw new ArgumentException();
        }

        private CommandBase GenerateFromConfiguration(TestSuiteConfiguration testSuit, TestConfiguration test, DependencyCommandConfiguration command, CommandEngineConfiguration engineConfig)
        {
            string workDir = GenerateWorkDirectory(testSuit);
            CommandBase scriptCommand = null;
            if (command.VisualStudio != null || command.NodeJS != null || command.Git != null || command.DevMark != null || command.Dotnet != null || command.DotnetFramework != null || command.Path != null)
            {
                scriptCommand = new DependencyCommand();
            }
            
            if (scriptCommand != null)
            {
                scriptCommand.Initialize(command, workDir, engineConfig.TraceLogging);
                return scriptCommand;
            }
            throw new ArgumentException();
        }

        private CommandBase GenerateFromConfiguration(TestSuiteConfiguration testSuit, TestConfiguration test, CommandConfiguration command, CommandEngineConfiguration engineConfig)
        {
            string workDir = GenerateWorkDirectory(testSuit);
            CommandBase scriptCommand = null;
            if (command.Git != null)
            {
                scriptCommand = new GitCommand();
            }
            if (command.Shell != null)
            {
                scriptCommand = new ShellCommand();
            }
            if (command.Process != null)
            {
                scriptCommand = new ProcessCleanupCommand();
            }
            if (command.ClearShell == true)
            {
                scriptCommand = new NoOpCommand();
            }
            if (command.Write != null)
            {
                scriptCommand = new WriteCommand();
            }

            if (scriptCommand != null)
            {
                scriptCommand.Initialize(command, workDir, engineConfig.TraceLogging);
                return scriptCommand;
            }
            throw new ArgumentException();
        }

        public SysInfoCommand CreateSysInfo(string platform, CommandEngineConfiguration engineConfig)
        {
            var sysInfo = new SysInfoCommand(); 
            sysInfo.Initialize(platform, engineConfig.TraceLogging);
            return sysInfo; 
        }

        private string GenerateWorkDirectory(TestSuiteConfiguration testSuit)
        {
            string name = $"{testSuit.Name}-{testSuit.Version}"; 
            Regex filterExpression = new Regex("[^a-zA-Z0-9 -]");
            string subDir = filterExpression.Replace(name, "");
            string testDir = Path.Combine(_baseWorkDir, subDir);
            return testDir;
        }
    }
}
