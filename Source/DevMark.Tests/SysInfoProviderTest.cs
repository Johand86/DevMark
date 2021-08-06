using DevMark.Core.SystemInformation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog.Sinks.TestCorrelator;
using System.Linq;
using System.Runtime.InteropServices;

namespace DevMark.Tests
{
    [TestClass]
    public class SysInfoProviderTest
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            TestSetup.Initialize();
        }

        [TestMethod]
        public void SysInfoCommand_Success()
        {
            using (TestCorrelator.CreateContext())
            {
                using (ServiceProvider serviceProvider = TestSetup.BuildDefaultServiceProvider())
                {
                    var sysInfoProvider = serviceProvider.GetService<SysInfoProvider>();
                    var platform = sysInfoProvider.GetOsPlatform();
                    var sysInfo = sysInfoProvider.ExecuteSysInfoCommand(platform, false);

                    Assert.IsTrue(sysInfo.Successfull);
                    Assert.AreEqual(0, sysInfo.ExitCode);
                    Assert.IsFalse(sysInfo.Streams.Any(x => x.Severity == StreamSeverty.Error || x.Severity == StreamSeverty.Warn));
                    Assert.IsNull(sysInfo.Exception);

                }
            }
        }

        [TestMethod]
        public void ParseSysInfo_Windows_Success()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (TestCorrelator.CreateContext())
                {
                    using (ServiceProvider serviceProvider = TestSetup.BuildDefaultServiceProvider())
                    {
                        var sysInfoProvider = serviceProvider.GetService<SysInfoProvider>();
                        var result = sysInfoProvider.Get();

                        Assert.IsNotNull(result.OS.Name);
                        Assert.IsNotNull(result.OS.Version);
                        Assert.IsNotNull(result.Motherboard);
                        Assert.IsTrue(result.CPUs.Count >= 1);
                        Assert.IsTrue(result.Disks.Count >= 1);
                        Assert.IsTrue(result.RAM.Count >= 1);
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
