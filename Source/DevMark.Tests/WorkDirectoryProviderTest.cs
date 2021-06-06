using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog.Sinks.TestCorrelator;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DevMark.Tests
{
    [TestClass]
    public class WorkDirectoryProviderTest
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            TestSetup.Initialize();
        }

        [TestMethod]
        public void UsingParameter()
        {
            using (TestCorrelator.CreateContext())
            {
                using (ServiceProvider serviceProvider = TestSetup.BuildDefaultServiceProvider())
                {
                    var specificFolder = Path.Combine(Path.GetTempPath(), "DevMarkTests");

                    var provider = serviceProvider.GetRequiredService<WorkDirectoryProvider>();
                    string actual = provider.Setup(specificFolder);

                    Assert.AreEqual(specificFolder, actual);
                }
            }
        }

        [TestMethod]
        public void NotInSourceCodeFolder()
        {
            using (TestCorrelator.CreateContext())
            {
                using (ServiceProvider serviceProvider = TestSetup.BuildDefaultServiceProvider())
                {
                    var provider = serviceProvider.GetRequiredService<WorkDirectoryProvider>();
                    string actual = provider.Setup(null);

                    Assert.IsFalse(actual.Contains(AppDomain.CurrentDomain.BaseDirectory));
                }
            }
        }

        [TestMethod]
        public void NotInWriteProtectedFolder()
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (TestCorrelator.CreateContext())
                {
                    using (ServiceProvider serviceProvider = TestSetup.BuildDefaultServiceProvider())
                    {
                        var specificFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "DevMarkShouldNotHaveWriteAccess");

                        var provider = serviceProvider.GetRequiredService<WorkDirectoryProvider>();
                        string actual = provider.Setup(specificFolder);

                        Assert.IsFalse(actual.Contains(AppDomain.CurrentDomain.BaseDirectory));
                    }
                }
            }

            else
            {
                Assert.Inconclusive("Test requires Windows.");
            }
        }
    }
}
