namespace DevMark.CommandLine.Arguments
{
    public class CheckCommandLineArgs : BaseCommandLineArgs
    {
        public string Test { get; set; }
        public string WorkDir { get; set; }
        public bool Docker { get; set; }
    }
}