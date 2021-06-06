namespace DevMark
{
    public class CommandConfiguration : CommandConfigurationBase
    {
        public ShellCommandConfiguration Shell { get; set; }
        public GitCommandConfiguration Git { get; set; }
        public ProcessCleanupCommandConfiguration Process { get; set; }
        public WriteCommandConfiguration Write { get; set; }
    }

    public class CommandConfigurationBase
    {
        public bool? ClearShell { get; set; }
    }
}
