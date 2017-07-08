using System;
using OpenQA.Selenium;
using SerenityBDD.Core.External;
using SerenityBDD.Core.time;
using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.Steps
{
    public class RenderedPageObjectView
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

        public void waitFor(ExpectedCondition byElementCriteria)
        {
            throw new NotImplementedException();
        }

        public void waitForPresenceOf(By byElementCriteria)
        {
            throw new NotImplementedException();
        }

        public void waitForElementsToDisappear(By byElementCriteria)
        {
            throw new NotImplementedException();
        }

        public void waitForText(string expectedText)
        {
            throw new NotImplementedException();
        }

        public void waitForTitleToDisappear(string expectedTitle)
        {
            throw new NotImplementedException();
        }

        internal void waitForText(IWebElement element, string expectedText)
        {
            throw new NotImplementedException();
        }

        internal void waitForAnyTextToAppear(IWebElement element, string[] expectedText)
        {
            throw new NotImplementedException();
        }

        public void waitForAllTextToAppear(string[] expectedTexts)
        {
            throw new NotImplementedException();
        }

        public void waitForAnyRenderedElementOf(By[] expectedElements)
        {
            throw new NotImplementedException();
        }

        public bool containsText(string textValue)
        {
            throw new NotImplementedException();
        }

        public WebElementFacade waitFor(WebElementFacade byElementCriteria)
        {
            throw new NotImplementedException();
        }
    }
}