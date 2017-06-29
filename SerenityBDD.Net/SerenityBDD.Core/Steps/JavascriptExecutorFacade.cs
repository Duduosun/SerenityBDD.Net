using System;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public class JavascriptExecutorFacade
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

        internal object executeScript(string script, object[] args)
        {
            throw new NotImplementedException();
        }

        public void executeAsyncScript(string javaScript)
        {
            throw new NotImplementedException();
        }
    }
}