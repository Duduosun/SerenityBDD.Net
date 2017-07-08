using System;
using OpenQA.Selenium;
using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.Steps
{
    public class FluentWaitWithRefresh
    {
        public FluentWaitWithRefresh(IWebDriver driver, Clock webdriverClock, Sleeper sleeper)
        {
            throw new NotImplementedException();
        }

        public FluentWaitWithRefresh withTimeout(Duration getWaitForTimeout, TimeUnit milliseconds)
        {
            throw new NotImplementedException();
        }

        public FluentWaitWithRefresh pollingEvery(int waitForElementPauseLength, TimeUnit milliseconds)
        {
            throw new NotImplementedException();
        }

        public ThucydidesFluentWait<IWebDriver> ignoring(params Type [] exceptionTypes)
        {
            throw new NotImplementedException();
        }
    }
}