using YamlDotNet.Serialization;

namespace DevMark
{
    public class DependencyCommandConfiguration : CommandConfigurationBase
    {
        [YamlMember(Alias = "VisualStudio", ApplyNamingConventions = false)]
        public VisualStudioDependencyConfiguration VisualStudio { get; set; }
        [YamlMember(Alias = "NodeJS", ApplyNamingConventions = false)]
        public NodeJSDependencyConfiguration NodeJS { get; set; }
        public GitDependencyConfiguration Git { get; set; }
        [YamlMember(Alias = "DevMark", ApplyNamingConventions = false)]
        public DevMarkDependencyConfiguration DevMark { get; set; }
        public DotnetDependencyConfiguration Dotnet { get; set; }
        public DotnetFrameworkDependencyConfiguration DotnetFramework { get; set; }
        public PathDependencyConfiguration Path { get; set; }
    }
}
