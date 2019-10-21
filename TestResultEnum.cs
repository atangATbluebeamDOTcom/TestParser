using System.IO;

namespace TestParser
{
    internal enum TestResult
    {
        Passed, Skipped, Failed, Deleted, New
    }

    internal static class TestResultExtensions
    {
        internal static string ToConsoleString(this TestResult pResult)
        {
            return pResult switch
            {
                TestResult.Passed => "PASSED   ",
                TestResult.Skipped => "SKIPPED  ",
                TestResult.Failed => "FAILED   ",
                TestResult.New => "NEW      ",
                TestResult.Deleted => "DELETED  ",
                _ => throw new InvalidDataException(),
            };
        }
    }
}