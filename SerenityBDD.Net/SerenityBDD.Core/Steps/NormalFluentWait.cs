using OpenQA.Selenium;
using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.Steps
{
    public class NormalFluentWait : FluentWaitWithRefresh
    {
        public NormalFluentWait(IWebDriver driver, Clock webdriverClock, Sleeper sleeper) : base(driver, webdriverClock, sleeper)
        {
        }


        Inflector inflection = Inflector.getInstance();


        public override string ToString()
        {
            return inflection.Of(GetType().Name)
                .inHumanReadableForm().ToString();
        }



    }
}