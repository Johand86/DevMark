using DevMark.Model;
using DevMark.Model.SystemInformation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace DevMark.Core.TestResult
{
    public class HashCalculator
    {
        ILogger<HashCalculator> _logger;

        JsonSerializerSettings settings = new JsonSerializerSettings {
            Culture = CultureInfo.InvariantCulture,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Formatting = Formatting.None,
            Converters = new List<JsonConverter> { new StringEnumConverter() },
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
        };

        public HashCalculator(ILogger<HashCalculator> logger)
        {
            _logger = logger;
        }
    
        public string Calculate(SysInfo sysInfo)
        {
            return ComputeHash(sysInfo);
        }

        public bool Verify(TestRun testRun)
        {
            var actualSysInfoHash = ComputeHash(testRun.SysInfo);
            for (int i = 0; i < testRun.TestSuites.Count; i++)
            {
                var suiteHash = Calculate(testRun.TestSuites[i]);

                var expected = testRun.Verification.TestSuites[i];

                if (suiteHash != expected)
                {
                    return false;
                }
            }

            if (actualSysInfoHash != testRun.Verification.SysInfo)
                return false;
            return true;
        }

        public string ComputeHash<T>(T obj)
        {
            if (obj == null)
                return null;

            var val = JsonConvert.SerializeObject(obj, settings);
            return ComputeSha256Hash(Encoding.UTF8.GetBytes($"A minor dash of salt - {val}"));
        }

        static string ComputeSha256Hash(byte[] bytes)
        {
            using (SHA256 alg = SHA256.Create())
            {
                var sb = new StringBuilder();
                var hashBytes = alg.ComputeHash(bytes);
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public string Calculate(TestSuiteConfiguration testSuiteConfig)
        {
            // Allow modification to anything that doesnt change the outcome of the tests.

            var testSuiteConfigClone = JsonClone(testSuiteConfig);
            testSuiteConfigClone.Description = null;
            testSuiteConfigClone.Name = null;
            testSuiteConfigClone.Version = null;
            testSuiteConfigClone.Author = null;
            foreach (var test in testSuiteConfigClone.Tests)
            {
                test.Name = null;
                test.Category = null;
                test.Description = null;
            }

            var configHash = ComputeHash(testSuiteConfigClone);

            return configHash;
        }

        public string Calculate(TestSuite testSuite)
        {
            var testSuiteClone = JsonClone(testSuite);

            // Allow modification to anything that doesnt change the outcome of the tests.
            // The hash of the test suite config is what actually connects the result with a suite.

            testSuiteClone.Name = null;
            testSuiteClone.Description = null;
            testSuiteClone.Author = null;
            foreach (var test in testSuiteClone.Tests)
            {
                test.Name = null;
                test.Category = null;
                test.Description = null;
            }

            return ComputeHash(testSuiteClone);
        }

        private T JsonClone<T>(T resultTestSuite)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(resultTestSuite));
        }
    }
}
