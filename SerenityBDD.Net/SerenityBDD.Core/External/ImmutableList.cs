using System;
using System.Collections.Generic;
using SerenityBDD.Core.Model;

namespace SerenityBDD.Core.External
{
    public class ImmutableList
    {
        public static List<T> copyOf<T>(List<T> allIssues)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<TestResult> Of(TestResult testResultFromSteps, TestResult annotatedResult)
        {
            throw new NotImplementedException();
        }
    }
}