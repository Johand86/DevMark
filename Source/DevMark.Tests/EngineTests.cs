using DevMark.Core.Engine;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog.Sinks.TestCorrelator;
using System.Linq;

namespace DevMark.Tests
{
    [TestClass]
    public class EngineTests
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            TestSetup.Initialize();
        }

        [TestMethod]
        public void RunTestSuit()
        {
            using (TestCorrelator.CreateContext())
            {
                using (ServiceProvider serviceProvider = TestSetup.BuildDefaultServiceProvider())
                {
                    var engine = serviceProvider.GetService<BenchmarkEngine>();

                    engine.TraceLogging = true;

                    engine.RunTestSuiteFromFile("TestSuites\\Echo.yaml", new string[] { "default" });

                    TestCorrelator.GetLogEventsFromCurrentContext().Where(x => x.MessageTemplate.Text == "Starting  -- Test suite init -- Echo -- Restore environment").Should().ContainSingle();
                    TestCorrelator.GetLogEventsFromCurrentContext().Where(x => x.MessageTemplate.Text == "Completed -- Test suite cleanup -- Echo -- Test suite cleanup").Should().ContainSingle();
                }
            }
        }
    }
}
