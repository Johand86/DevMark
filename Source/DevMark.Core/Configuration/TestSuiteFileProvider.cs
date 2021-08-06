using System;
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

            var testSuiteDefaultPath = GetDefaultPath();
            if (all)
            {
                return Directory.GetFiles(testSuiteDefaultPath, "*.yaml");
            }

            if (testSuite != null && Directory.Exists(testSuite))
            {
                return Directory.GetFiles(testSuite, "*.yaml");
            }


            if (testSuite != null)
            {
                if (!testSuite.Contains(";"))
                {
                    return ResolveTestSuites(testSuiteDefaultPath, testSuite);
                }
                else
                {
                    var suitePaths = testSuite.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> testSuiteCollection = new List<string>();
                    foreach (var suitePath in suitePaths)
                    {
                        testSuiteCollection.AddRange(ResolveTestSuites(testSuiteDefaultPath, suitePath));
                    }
                    return testSuiteCollection.ToArray();
                }
               
            }

            return new string[0];
        }

        private string[] ResolveTestSuites(string testSuiteDefaultPath, string testSuite)
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

            return new string[0];
        }


        public IEnumerable<string> ListTestSuits(string testSuitesDefaultPath)
        {
            return Directory.GetFiles(testSuitesDefaultPath, "*.yaml");
        }
    }
}