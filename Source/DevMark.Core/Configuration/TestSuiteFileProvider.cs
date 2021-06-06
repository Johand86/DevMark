using System.Collections.Generic;
using System.IO;

namespace DevMark
{
    public class TestSuiteFileProvider
    {
        public string GetDefaultPath()
        {
            string exeLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string testSuitesDefaultPath = Path.Combine(exeLocation, "TestSuites");
            return testSuitesDefaultPath;
        }

        public string[] ResolveTestSuites(string testSuite, bool all)
        {
            if (testSuite != null && Directory.Exists(testSuite))
            {
                return Directory.GetFiles(testSuite, "*.yaml");
            }

            var testSuiteDefaultPath = GetDefaultPath();

            if (testSuite != null)
            {
                if (File.Exists(testSuite))
                {
                    return new string[] { testSuite };
                }
                else if (testSuite.ToLower().EndsWith(".yaml") != true)
                {
                    string suitePath = Path.Combine(testSuiteDefaultPath, $"{testSuite}.yaml");
                    if (File.Exists(suitePath))
                        return new string[] { suitePath };
                }
            }

            if (all)
            {
                return Directory.GetFiles(testSuiteDefaultPath, "*.yaml");
            }

            return new string[0];
        }

        public IEnumerable<string> ListTestSuits(string testSuitesDefaultPath)
        {
            return Directory.GetFiles(testSuitesDefaultPath, "*.yaml");
        }
    }
}