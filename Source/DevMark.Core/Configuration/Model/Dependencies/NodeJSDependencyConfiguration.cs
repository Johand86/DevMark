using YamlDotNet.Serialization;

namespace DevMark
{
    public class NodeJSDependencyConfiguration : BaseDependencyVersionConfiguration
    {
        [YamlMember(Alias = "NPM", ApplyNamingConventions = false)]
        public NPMDepdendencyConfiguration NPM { get; set; }
        public bool? NativeCppModules { get; set; }
        public bool? NativePythonModules { get; set; }
        
    }
}
