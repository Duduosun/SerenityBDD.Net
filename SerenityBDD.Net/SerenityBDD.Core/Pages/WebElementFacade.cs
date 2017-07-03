using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public interface WebElementFacade : IWebElement
    {
        WebElementFacade find(By selector);
        void type(string value);
        void shouldBeVisible();
        void shouldNotBeVisible();
    }
}