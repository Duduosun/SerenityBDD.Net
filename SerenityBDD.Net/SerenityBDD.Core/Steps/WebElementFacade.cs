using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public interface WebElementFacade
    {
        WebElementFacade find(By selector);
    }
}