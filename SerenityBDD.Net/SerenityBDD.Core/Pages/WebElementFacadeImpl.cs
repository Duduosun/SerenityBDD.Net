using System;
using OpenQA.Selenium;
using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.Steps
{
    public class WebElementFacadeImpl
    {
        public static WebElementFacade wrapWebElement(IWebDriver driver, IWebElement element, Duration implicitWaitTimeout, Duration waitForTimeout, string elementName)
        {
            throw new NotImplementedException();
        }

        public static WebElementFacade wrapWebElement(IWebDriver driver, By bySelector, Duration getImplicitWaitTimeout, Duration getWaitForTimeout, string elementName)
        {
            throw new NotImplementedException();
        }
    }
}