using System;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public static class ExpectedConditions
    {
        public static Predicate<IWebDriver> titleIs(string expectedTitle)
        {
            return new Predicate<IWebDriver>((driver => driver.Title.Equals(expectedTitle, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}