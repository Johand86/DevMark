using DevMark.Core.Engine;
using DevMark.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevMark.Core.TestResult
{
    public class TestResultCompilationProvider
    {
        public TestSuite Convert(List<BenchmarkResult> results)
        {
            TestSuite testSuite = new TestSuite();

            var suiteConfig = results.First().SuiteConfig;
            testSuite.Name = suiteConfig.Name;
            testSuite.Description = suiteConfig.Description;
            testSuite.Author = suiteConfig.Author;
            testSuite.Version = suiteConfig.Version;

            foreach (var depHash in results.Select(x => x.Command as DependencyCommand)
                                            .Where(x => x != null && x.Dependencies != null)
                                            .Select(x => x.Dependencies).ToList())
            {
                foreach (var key in depHash.Keys)
                {
                    testSuite.Dependencies.Add(new Dependency { Name = key?.ToString(), Version = depHash[key]?.ToString()});
                }
            }

            foreach (var result in results.Where(x => x.Scope == BenchmarkScope.Benchmark))
            {
                var test = testSuite.Tests.FirstOrDefault(x => x.Name == result.TestConfig.Name);
                if (test == null)
                {
                    test = new Test();
                    test.Name = result.TestConfig.Name;
                    test.Description = result.TestConfig.Description;
                    test.Category = result.TestConfig.Category;
                    testSuite.Tests.Add(test);
                }
   
                var shellCommand = result.Command as ShellCommand;
                test.Benchmarks.Add(new Benchmark
                {
                    Iteration = result.ScopeIndex,
                    Timestamp = shellCommand?.Timestamp,
                    Duration = result.Command.Duration.HasValue ? result.Command.Duration.Value.TotalSeconds : null,
                    Success = result.Command.Successfull
                });

            }

            return testSuite;
        }
    }
}
