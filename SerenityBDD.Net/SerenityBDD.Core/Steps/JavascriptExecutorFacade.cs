using System;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    internal class JavascriptExecutorFacade
    {
        private IWebDriver driver;

        public JavascriptExecutorFacade(IWebDriver driver)
        {
            this.driver = driver;
        }

        public object executeScript(string script)
        {
            throw new NotImplementedException();
        }
    }
}