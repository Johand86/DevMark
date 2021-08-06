using DevMark.Core.Container;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog.Sinks.TestCorrelator;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevMark.Tests
{
    [TestClass]
    public class DockerTests
    {
        static string containerName = "devmarkecho";

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            TestSetup.Initialize();
        }

        [TestMethod]
        public async Task BuildContainer_Success()
        {
            using (TestCorrelator.CreateContext())
            {
                using (ServiceProvider serviceProvider = TestSetup.BuildDefaultServiceProvider())
                {
                    var containerProvider = serviceProvider.GetService<DockerContainerProvider>();

                    await BuildEchoContainer(containerProvider);

                    TestCorrelator.GetLogEventsFromCurrentContext().Where(x => x.MessageTemplate.Text.Contains("Successfully built")).Should().ContainSingle();
                }
            }
        }

        [TestMethod]
        public async Task RunContainer()
        {
            using (TestCorrelator.CreateContext())
            {
                using (ServiceProvider serviceProvider = TestSetup.BuildDefaultServiceProvider())
                {
                    var containerProvider = serviceProvider.GetService<DockerContainerProvider>();

                    await BuildEchoContainer(containerProvider);

                    var output = await containerProvider.Run(containerName, new string[] { });

                    TestCorrelator.GetLogEventsFromCurrentContext().Where(x => x.MessageTemplate.Text.Contains(".NET runtimes installed")).Should().ContainSingle();
                    Assert.AreEqual(0, output.ExitCode);
                }
            }
        }

        private static async Task BuildEchoContainer(DockerContainerProvider containerProvider)
        {
            var files = new List<(string sysPath, string tarPath)> { new("Containers\\Dockerfile", "Dockerfile") };

            await containerProvider.BuildImage(containerName, "Dockerfile", files);
        }
    }
}
