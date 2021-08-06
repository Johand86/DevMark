namespace DevMark.Core.Container
{
    public class ContainerRunResult
    {
        public string ResultPath { get; set; }
        public long ExitCode { get; set; }
        public string Image { get; set; }
        public long CPUs { get; set; }
        public long Memory { get; set; }
    }
}
