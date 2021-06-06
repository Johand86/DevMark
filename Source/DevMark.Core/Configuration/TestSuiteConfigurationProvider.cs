using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DevMark
{
    public class TestSuiteConfigurationProvider
    {
        public TestSuiteConfiguration ParseFile(string path)
        {
            string content = File.ReadAllText(path);
            return Parse(content);
        }

        public TestSuiteConfiguration Parse(string yaml)
        {
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var data = deserializer.Deserialize<TestSuiteConfiguration>(yaml);
            return data;
        }
    }
}
