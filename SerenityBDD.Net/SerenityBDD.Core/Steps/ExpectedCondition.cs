using System;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    //TODO: Migrate to selenium.net version of these objects
    public class ExpectedCondition : ExpectedCondition<bool> {
        public Func<IWebDriver, bool> Func { get; }

        public ExpectedCondition(Func<IWebDriver, bool> func)
        {
            Func = func;
            throw new NotImplementedException();
        }
    }

    public class ExpectedCondition<T>
    {
    }
}