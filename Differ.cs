using System;
using System.Collections.Generic;
using System.IO;

namespace TestParser
{
    internal class Differ
    {
        private readonly FileStream _fileA;
        private readonly FileStream _fileB;

        public Differ(FileStream pFileA, FileStream pFileB)
        {
            _fileA = pFileA;
            _fileB = pFileB;
        }

        internal Dictionary<Tuple<TestResult, TestResult>, List<string>> Diff(out Dictionary<TestResult, List<string>> pResultA, out Dictionary<TestResult, List<string>> pResultB, bool pUseLongName)
        {
            pResultA = new Parser(_fileA).ParseTest(pUseLongName);
            pResultB = new Parser(_fileB).ParseTest(pUseLongName);

            var invertedTestResultsA = new Dictionary<string, TestResult>();
            var invertedTestResultsB = new Dictionary<string, TestResult>();

            // sometimes the tests have the same names -_-
            var duplicateCountA = new Dictionary<string, int>();
            var duplicateCountB = new Dictionary<string, int>();

            var allTests = new HashSet<string>();

            foreach (var key in pResultA.Keys)
            {
                foreach (var testName in pResultA[key])
                {
                    if (invertedTestResultsA.ContainsKey(testName))
                    {
                        if (duplicateCountA.ContainsKey(testName))
                        {
                            duplicateCountA[testName]++;
                        }
                        else
                        {
                            duplicateCountA.Add(testName, 1);
                        }
                        var newTestName = testName + " (" + duplicateCountA[testName] + ")";
                        invertedTestResultsA.Add(newTestName, key);
                        allTests.Add(newTestName);
                    }
                    else
                    {
                        invertedTestResultsA.Add(testName, key);
                        allTests.Add(testName);
                    }
                }
            }
            foreach (var key in pResultB.Keys)
            {
                foreach (var testName in pResultB[key])
                {
                    if (invertedTestResultsB.ContainsKey(testName))
                    {
                        if (duplicateCountB.ContainsKey(testName))
                        {
                            duplicateCountB[testName]++;
                        }
                        else
                        {
                            duplicateCountB.Add(testName, 1);
                        }
                        var newTestName = testName + " (" + duplicateCountB[testName] + ")";
                        invertedTestResultsB.Add(newTestName, key);
                        allTests.Add(newTestName);
                    }
                    else
                    {
                        invertedTestResultsB.Add(testName, key);
                        allTests.Add(testName);
                    }
                }
            }

            var diffResults = new Dictionary<Tuple<TestResult, TestResult>, List<string>>();
            foreach (var test in allTests)
            {
                TestResult resultA;
                if (invertedTestResultsA.ContainsKey(test))
                {
                    resultA = invertedTestResultsA[test];
                }
                else
                {
                    resultA = TestResult.New;
                }

                TestResult resultB;
                if (invertedTestResultsB.ContainsKey(test))
                {
                    resultB = invertedTestResultsB[test];
                }
                else
                {
                    resultB = TestResult.Deleted;
                }

                var key = new Tuple<TestResult, TestResult>(resultA, resultB);
                if (!diffResults.ContainsKey(key))
                {
                    diffResults.Add(key, new List<string>());
                }
                diffResults[key].Add(test);
            }

            foreach (var key in diffResults.Keys)
            {
                diffResults[key].Sort();
            }

            return diffResults;
        }
    }
}
