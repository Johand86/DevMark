namespace DevMark
{
    public class BaseCommandConfiguration
    {
        public string Name { get; set; }
        public bool? SpawnNewShell { get; set; }
        public PlatformConfiguration? Platform { get; set; }
    }
}
