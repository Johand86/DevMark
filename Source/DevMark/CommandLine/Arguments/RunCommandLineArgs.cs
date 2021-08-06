namespace DevMark.CommandLine.Arguments
{
    public class RunCommandLineArgs : BaseCommandLineArgs
    {
        public string Test { get; set; }
        public string Categories { get; set; }
        public bool List { get; set; }
        public string WorkDir { get; set; }
        public string Result { get; set; }
        public string ApiKey { get; set; }

        public bool Docker { get; set; }
        public string Dockerfile { get; set; }

        public bool DockerSkipBuild { get; set; }
        public bool DockerDevContainer { get; set; }
        public string DockerDevSourcePath { get; set; }
    }
}