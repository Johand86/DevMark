namespace DevMark.CommandLine.Arguments
{
    public class UploadCommandLineArgs : BaseCommandLineArgs
    {
        public string Url { get; set; }
        public string Result { get; set; }
        public string ApiKey { get; set; }
    }
}