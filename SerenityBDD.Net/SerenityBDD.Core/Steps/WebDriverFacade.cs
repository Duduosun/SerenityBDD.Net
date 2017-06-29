using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public interface WebDriverFacade {
        WebDriverFacade getDriverClass();
        bool isEnabled();
        bool isInstantiated();
        IWebDriver getProxiedDriver();
    }
}