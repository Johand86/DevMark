namespace DevMark.CommandLine.Arguments
{
    public class CommandLineResult
    {
        public CommandLineResult(bool showHelp = false, string message = null)
        {
            ShowHelp = showHelp;
            Message = message;
        }
        public bool ShowHelp { get; }
        public string Message { get; }


        public static CommandLineResult Help => new CommandLineResult(showHelp: true, message: null);
        public static CommandLineResult Success => new CommandLineResult(showHelp: false, message: null);
    }
}