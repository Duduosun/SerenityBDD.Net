using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public class ThucydidesFluentAdapter
    {
        private IWebDriver webDriver;

        public ThucydidesFluentAdapter(IWebDriver webDriver)
        {
            this.webDriver = webDriver;
        }
    }
}