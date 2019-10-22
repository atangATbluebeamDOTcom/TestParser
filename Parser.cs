using System.Collections.Generic;
using System.IO;

namespace TestParser
{
    internal class Parser
    {
        private const int TEST_CLASS_METADATA_LENGTH = 81;
        private const int TEST_METADATA_LENGTH = 36;

        private readonly FileStream _file;

        internal Parser(FileStream pFile)
        {
            _file = pFile;
        }

        internal Dictionary<TestResult, List<string>> ParseTest(bool pUseLongName)
        {
            var testResults = new Dictionary<TestResult, List<string>>();

            using var reader = new StreamReader(_file);
            string testclass = string.Empty;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("Starting:    "))
                {
                    var length = line.Length;
                    testclass = line.Substring(TEST_CLASS_METADATA_LENGTH, line.Length - TEST_CLASS_METADATA_LENGTH);
                }
                if (line.Contains("Passed   "))
                {
                    if (!testResults.ContainsKey(TestResult.Passed))
                    {
                        testResults.Add(TestResult.Passed, new List<string>());
                    }
                    var length = line.Length;
                    var testName = line.Substring(TEST_METADATA_LENGTH, line.Length - TEST_METADATA_LENGTH);
                    if (pUseLongName)
                    {
                        testResults[TestResult.Passed].Add(testclass + testName);
                    }
                    else
                    {
                        testResults[TestResult.Passed].Add(testName);
                    }
                }
                else if (line.Contains("Skipped  "))
                {
                    if (!testResults.ContainsKey(TestResult.Skipped))
                    {
                        testResults.Add(TestResult.Skipped, new List<string>());
                    }
                    var length = line.Length;
                    var testName = line.Substring(TEST_METADATA_LENGTH, line.Length - TEST_METADATA_LENGTH);
                    if (pUseLongName)
                    {
                        testResults[TestResult.Skipped].Add(testclass + testName);
                    }
                    else
                    {
                        testResults[TestResult.Skipped].Add(testName);
                    }
                }
                else if (line.Contains("Failed   "))
                {
                    if (!testResults.ContainsKey(TestResult.Failed))
                    {
                        testResults.Add(TestResult.Failed, new List<string>());
                    }
                    var length = line.Length;
                    var testName = line.Substring(TEST_METADATA_LENGTH, line.Length - TEST_METADATA_LENGTH);
                    if (pUseLongName)
                    {
                        testResults[TestResult.Failed].Add(testclass + testName);
                    }
                    else
                    {
                        testResults[TestResult.Failed].Add(testName);
                    }
                }
            }

            foreach (var key in testResults.Keys)
            {
                testResults[key].Sort();
            }

            return testResults;
        }
    }
}