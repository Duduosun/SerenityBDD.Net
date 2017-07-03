using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Attributes
{
    public class By : OpenQA.Selenium.By
    {
        public static By scLocator(string sclocator)
        {
            return new ByScLocator(sclocator);
        }
        public static By jquery(string jQuerySelector)
        {

            return new ByjQuerySelector(jQuerySelector);
        }

        public static By buttonText(string text)
        {

            return new ByButtonTextSelector(text);
        }


        public class ByScLocator : By
        {
            private readonly string scLocator;

            public ByScLocator(string scLocator)
            {
                this.scLocator = scLocator;
            }

            public IWebElement FindElement(ISearchContext context)
            {
                try
                {
                    IWebElement element = (IWebElement)((IJavaScriptExecutor)context)
                            .ExecuteScript("return isc.AutoTest.getElement(arguments[0]);", scLocator);
                    if (element != null)
                    {
                        return element;
                    }
                }
                catch (WebDriverException e)
                {
                    if ((bool)((IJavaScriptExecutor)context)
                    .ExecuteScript("return (typeof isc == 'undefined')"))
                    {
                        throw new NoSuchElementException("Not a SmartGWT page. Cannot locate element using SmartGTW locator " + ToString());
                    }
                }
                throw new NoSuchElementException("Cannot locate element using " + ToString());
            }


            public override string ToString()
            {
                return "By.sclocator: " + scLocator;
            }
        }


        public class ByButtonTextSelector : By
        {

            string buttonLabel;

            public ByButtonTextSelector(string buttonLabel)
            {
                this.buttonLabel = buttonLabel;
            }


            public IReadOnlyCollection<IWebElement> FindElements(ISearchContext context)
            {
                return context.FindElements(byXpathforButtonWithLabel(buttonLabel));
            }


            public IWebElement FindElement(ISearchContext context)
            {

                IWebElement element = context.FindElement(byXpathforButtonWithLabel(buttonLabel));
                if (element != null)
                {
                    return element;
                }
                throw new NoSuchElementException("Cannot locate element using " + ToString());
            }

            private OpenQA.Selenium.By byXpathforButtonWithLabel(string buttonLabel)
            {
                return OpenQA.Selenium.By.XPath($"//*[normalize-space(.)=\"{buttonLabel}\"]");
            }


            public override string ToString()
            {
                return "By.buttonText: " + buttonLabel;
            }
        }


        public class ByjQuerySelector : By
        {
            private readonly string jQuerySelector;

            public ByjQuerySelector(string jQuerySelector)
            {
                this.jQuerySelector = jQuerySelector;
            }


            public List<IWebElement> FindElements(ISearchContext context)
            {
                List<IWebElement> elements = (List<IWebElement>)((IJavaScriptExecutor)context)
                    .ExecuteScript("var elements = $(arguments[0]).get(); return ((elements.length) ? elements : null)",
                        jQuerySelector);
                if (elements != null)
                {
                    return elements;
                }
                throw new NoSuchElementException("Cannot locate elements using " + ToString());

            }


            public IWebElement FindElement(ISearchContext context)
            {
                IWebElement element = (IWebElement)((IJavaScriptExecutor)context)
                        .ExecuteScript("return $(arguments[0]).get(0)", jQuerySelector);
                if (element != null)
                {
                    return element;
                }
                throw new NoSuchElementException("Cannot locate element using " + ToString());
            }


            public override string ToString()
            {
                return "By.jQuerySelector: " + jQuerySelector;
            }
        }
    }



}
