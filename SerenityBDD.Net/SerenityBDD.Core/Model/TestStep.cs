using System;
using System.Collections.Generic;

namespace SerenityBDD.Core.Model
{
    public class TestStep
    {
        public string getConciseErrorMessage()
        {
            throw new NotImplementedException();
        }

        public string getErrorMessage()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ScreenshotAndHtmlSource> getScreenshots()
        {
            throw new NotImplementedException();
        }

        public TestResult getResult()
        {
            throw new NotImplementedException();
        }

        public bool isAGroup()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TestStep> getFlattenedSteps()
        {
            throw new NotImplementedException();
        }
    }
}