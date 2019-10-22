using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace TestParser
{
    internal class Program
    {
        private static bool _printPassing;
        private static bool _printSkipped;
        private static bool _printFailed;
        private static bool _useLongName;
        private static bool _showHelp;

        private static readonly List<Tuple<TestResult, TestResult>> DIFF_PRINT_ORDER = new List<Tuple<TestResult, TestResult>>
        {
            new Tuple<TestResult, TestResult>(TestResult.Passed, TestResult.Failed),
            new Tuple<TestResult, TestResult>(TestResult.Skipped, TestResult.Failed),
            new Tuple<TestResult, TestResult>(TestResult.New, TestResult.Failed),
            new Tuple<TestResult, TestResult>(TestResult.Failed, TestResult.Passed),
            new Tuple<TestResult, TestResult>(TestResult.Skipped, TestResult.Passed),
            new Tuple<TestResult, TestResult>(TestResult.New, TestResult.Passed),
            new Tuple<TestResult, TestResult>(TestResult.Passed, TestResult.Skipped),
            new Tuple<TestResult, TestResult>(TestResult.Failed, TestResult.Skipped),
            new Tuple<TestResult, TestResult>(TestResult.New, TestResult.Skipped),
            new Tuple<TestResult, TestResult>(TestResult.Passed, TestResult.Deleted),
            new Tuple<TestResult, TestResult>(TestResult.Skipped, TestResult.Deleted),
            new Tuple<TestResult, TestResult>(TestResult.Failed, TestResult.Deleted)
        };

        public static void Main(string[] args)
        {
            var options = new OptionSet()
            {
                {
                    "p", "print the names of the passing tests",
                    v => _printPassing = v != null
                },
                {
                    "s", "print the names of the skipped tests",
                    v => _printSkipped = v != null
                },
                {
                    "f", "print the names of the failed tests",
                    v => _printFailed = v != null
                },
                {
                    "l", "print the namespace of the tests",
                    v => _useLongName = v != null
                },
                {
                    "h|help", "show this message and exit",
                    v => _showHelp = v != null
                },
            };

            List<string> extra = null;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("parsetest: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `parsetest --help` for more information.");
                Environment.Exit(-1);
            }

            if (args.Length == 0 || _showHelp)
            {
                PrintHelp(options);
                return;
            }

            if (extra.Count == 0 || extra.Count > 2)
            {
                Console.WriteLine("Invalid number of arguments.");
                PrintHelp(options);
                Environment.Exit(-1);
            }

            if (extra.Count == 1)
            {
                try
                {
                    using var stream = File.Open(extra[0], FileMode.Open);
                    Parse(stream, extra[0]);
                }
                catch
                {
                    Console.WriteLine("Error opening file: "+extra[0]);
                    Console.WriteLine("Make sure the file is in the same directory as parsetest.");
                    Environment.Exit(-1);
                }
            }
            else if (extra.Count == 2)
            {
                try
                {
                    using var streamA = File.Open(extra[0], FileMode.Open);
                    using var streamB = File.Open(extra[1], FileMode.Open);
                    Diff(streamA, extra[0], streamB, extra[1]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error opening files: "+extra[0]+", "+extra[1]);
                    Console.WriteLine("Make sure the files are in the same directory as parsetest.");
                    Console.WriteLine(e.Message);
                    Environment.Exit(-1);
                }
            }
        }

        private static void Parse(FileStream pStream, string pLogName)
        {
            var testResults = new Parser(pStream).ParseTest(_useLongName);

            if (!testResults.TryGetValue(TestResult.Passed, out List<string> passedTests))
            {
                passedTests = new List<string>();
            }
            if (!testResults.TryGetValue(TestResult.Skipped, out List<string> skippedTests))
            {
                skippedTests = new List<string>();
            }
            if (!testResults.TryGetValue(TestResult.Failed, out List<string> failedTests))
            {
                failedTests = new List<string>();
            }

            Console.WriteLine(">>> Results of "+pLogName);
            Console.WriteLine("Passed: " + passedTests.Count + ", Skipped: " + skippedTests.Count + ", Failed: " + failedTests.Count);

            if (_printFailed)
            {
                if (failedTests.Count != 0)
                {
                    Console.WriteLine("");
                }
                foreach (var failedTest in failedTests)
                {
                    Console.WriteLine(TestResult.Failed.ToConsoleString() + failedTest);
                }
            }
            if (_printSkipped)
            {
                if (skippedTests.Count != 0)
                {
                    Console.WriteLine("");
                }
                foreach (var skippedTest in skippedTests)
                {
                    Console.WriteLine(TestResult.Skipped.ToConsoleString() + skippedTest);
                }
            }
            if (_printPassing)
            {
                if (passedTests.Count != 0)
                {
                    Console.WriteLine("");
                }
                foreach (var passedTest in passedTests)
                {
                    Console.WriteLine(TestResult.Passed.ToConsoleString() + passedTest);
                }
            }
        }

        private static void Diff(FileStream pStreamA, string pLogNameA, FileStream pStreamB, string pLogNameB)
        {
            var diffResults = new Differ(pStreamA, pStreamB).Diff(out var testResultsA, out var testResultsB, _useLongName);
            if (!testResultsA.TryGetValue(TestResult.Passed, out List<string> passedTestsA))
            {
                passedTestsA = new List<string>();
            }
            if (!testResultsA.TryGetValue(TestResult.Skipped, out List<string> skippedTestsA))
            {
                skippedTestsA = new List<string>();
            }
            if (!testResultsA.TryGetValue(TestResult.Failed, out List<string> failedTestsA))
            {
                failedTestsA = new List<string>();
            }
            if (!testResultsB.TryGetValue(TestResult.Passed, out List<string> passedTestsB))
            {
                passedTestsB = new List<string>();
            }
            if (!testResultsB.TryGetValue(TestResult.Skipped, out List<string> skippedTestsB))
            {
                skippedTestsB = new List<string>();
            }
            if (!testResultsB.TryGetValue(TestResult.Failed, out List<string> failedTestsB))
            {
                failedTestsB = new List<string>();
            }

            Console.WriteLine(">>> Results of " + pLogNameA);
            Console.WriteLine("Passed: " + passedTestsA.Count + ", Skipped: " + skippedTestsA.Count + ", Failed: " + failedTestsA.Count);
            Console.WriteLine(">>> Results of " + pLogNameB);
            Console.WriteLine("Passed: " + passedTestsB.Count + ", Skipped: " + skippedTestsB.Count + ", Failed: " + failedTestsB.Count);
            Console.WriteLine("|==================== DIFF ====================|");

            var empty = true;
            foreach (var key in DIFF_PRINT_ORDER)
            {
                if (!diffResults.ContainsKey(key))
                    continue;

                foreach (var test in diffResults[key])
                {
                    empty = false;
                    Console.WriteLine(key.Item1.ToConsoleString() + "=> " + key.Item2.ToConsoleString() + test);
                }
            }
            if (empty)
            {
                Console.WriteLine("None");
            }

            if (_printFailed)
            {
                var key = new Tuple<TestResult, TestResult>(TestResult.Failed, TestResult.Failed);
                if (diffResults.TryGetValue(key, out var failedTests))
                {
                    Console.WriteLine("");
                    foreach (var failedTest in failedTests)
                    {
                        Console.WriteLine(TestResult.Failed.ToConsoleString() + failedTest);
                    }
                }
            }
            if (_printSkipped)
            {
                var key = new Tuple<TestResult, TestResult>(TestResult.Skipped, TestResult.Skipped);
                if (diffResults.TryGetValue(key, out var skippedTests))
                {
                    Console.WriteLine("");
                    foreach (var skippedTest in skippedTests)
                    {
                        Console.WriteLine(TestResult.Skipped.ToConsoleString() + skippedTest);
                    }
                }
            }
            if (_printPassing)
            {
                var key = new Tuple<TestResult, TestResult>(TestResult.Passed, TestResult.Passed);
                if (diffResults.TryGetValue(key, out var passedTests))
                {
                    if (passedTests.Count != 0)
                    {
                        Console.WriteLine("");
                        foreach (var passedTest in passedTests)
                        {
                            Console.WriteLine(TestResult.Passed.ToConsoleString() + passedTest);
                        }
                    }
                }
            }
        }

        private static void PrintHelp(OptionSet pOptions)
        {
            Console.WriteLine("Usage: parsetest [-h|help] [-p] [-s] [-f] [-l] <logA> <logB>");
            Console.WriteLine("Parses Bitbucket build logs and prints the test counts.");
            Console.WriteLine("If one log is specified, the counts of passed, skipped, and failed tests are printed.");
            Console.WriteLine("If two logs are specified, the test differences are also printed.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            pOptions.WriteOptionDescriptions(Console.Out);
        }
    }
}