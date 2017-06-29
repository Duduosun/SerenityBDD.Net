using System;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public class SupportedWebDriver
    {
        public static SupportedWebDriver ForClass(WebDriverFacade driver)
        {
            throw new NotImplementedException();
        }

        public bool supportsJavascriptInjection()
        {
            throw new NotImplementedException();
        }

        public static SupportedWebDriver ForClass(IWebDriver getDriver)
        {
            throw new NotImplementedException();
        }
    }
}