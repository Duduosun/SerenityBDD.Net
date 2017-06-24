using System;
using OpenQA.Selenium;
using SerenityBDD.Core.time;

namespace SerenityBDD.Core.Steps
{
    internal class RenderedPageObjectView
    {
        private IWebDriver driver;
        private PageObject pageObject;
        private Duration duration;
        private bool v;

        public RenderedPageObjectView(IWebDriver driver, PageObject pageObject, Duration duration, bool v)
        {
            this.driver = driver;
            this.pageObject = pageObject;
            this.duration = duration;
            this.v = v;
        }

        internal void setWaitForTimeout(Duration waitForTimeout)
        {
            throw new NotImplementedException();
        }

        internal void waitFor(By byElementCriteria)
        {
            throw new NotImplementedException();
        }

        internal void waitForAnyTextToAppear(string[] expectedText)
        {
            throw new NotImplementedException();
        }

        internal void waitForTextToAppear(string expectedText, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        internal void waitForTextToDisappear(string expectedText, TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }

        internal bool elementIsDisplayed(By byCriteria)
        {
            throw new NotImplementedException();
        }
    }
}