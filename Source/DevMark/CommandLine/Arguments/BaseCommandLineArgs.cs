namespace DevMark.CommandLine.Arguments
{
    public class BaseCommandLineArgs
    {
        public bool Verbose { get; set; }
        public bool Diagnostics { get; set; }
        public string Log { get; set; }
        public bool All { get; set; }
        public bool Help { get; set; }
    }
}