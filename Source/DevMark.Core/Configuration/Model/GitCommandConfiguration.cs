namespace DevMark
{
    public class GitCommandConfiguration : BaseCommandConfiguration
    {
        public string Repository { get; set; }
        public string Branch { get; set; }
        public bool? Recursive { get; set; }
        public bool? Clean { get; set; }
        public bool? AllowLockedFiles { get; set; }
    }
}
