using DevMark.Model.SystemInformation;
using System;
using System.Collections.Generic;

namespace DevMark.Model
{
    public class TestRun
    {
        public TestRun()
        {
            SysInfo = new SysInfo();
            TestSuites = new List<TestSuite>();
            Verification = new TestRunHash();
        }
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Version { get; set; }
        public SysInfo SysInfo { get; set; }

        public List<TestSuite> TestSuites { get; set; }

        public TestRunHash Verification { get; set; }
        
    }


    public class TestRunHash
    {
        public TestRunHash()
        {
            TestSuites = new List<string>();
        }

        public string SysInfo { get; set; }
        public List<string> TestSuites { get; set; }
    }

    public class TestSuiteHash
    {
        public string Configuration { get; set; }
        public string Tests { get; set; }
    }

}
