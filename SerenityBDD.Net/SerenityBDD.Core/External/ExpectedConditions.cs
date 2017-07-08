using System;
using OpenQA.Selenium;

namespace SerenityBDD.Core.External
{
    public static class ExpectedConditions
    {
        public static Predicate<IWebDriver> titleIs(string expectedTitle)
        {
            return new Predicate<IWebDriver>((driver => driver.Title.Equals(expectedTitle, StringComparison.InvariantCultureIgnoreCase)));
        }

        public static Predicate<IWebDriver> visibilityOfElementLocated(By byCriteria)
        {
            throw new NotImplementedException();
        }

        public static Predicate<IWebDriver> invisibilityOfElementLocated(By byCriteria)
        {
            throw new NotImplementedException();
        }
    }
}