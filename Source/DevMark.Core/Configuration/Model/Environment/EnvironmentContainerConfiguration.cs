namespace DevMark
{
    public class EnvironmentContainerConfiguration : BaseDependencyVersionConfiguration
    {
        public EnvironmentContainerImageConfiguration Image { get; set; }
        public PlatformConfiguration Platform { get; set; }
    }
}
