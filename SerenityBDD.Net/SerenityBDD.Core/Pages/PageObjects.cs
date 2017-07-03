using System;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public class PageObjects
    {
        public const String NO_WEBDRIVER_CONSTRUCTOR_MESSAGE = "This page object does not appear have a constructor that takes a WebDriver parameter";

        private readonly IWebDriver driver;

        public PageObjects(IWebDriver driver)
        {
            this.driver = driver;
        }

        public static PageObjects usingDriver(IWebDriver driver)
        {
            return new PageObjects(driver);
        }

        public object ofType(Type pageObjectClass)
            
        {
            try
            {
                Optional<PageObject> simplePageObject = newPageObjectWithSimpleConstructor(pageObjectClass);
                return (simplePageObject.isPresent() ? simplePageObject.get() : newPageObjectWithDriver(pageObjectClass));
            }
            catch (Exception somethingWentWrong)
            {
                throw pageLooksDodgyExceptionBasedOn(somethingWentWrong, pageObjectClass);
            }
        }

        private PageLooksDodgyException pageLooksDodgyExceptionBasedOn(Exception  somethingWentWrong, Type pageObjectClass)
        {
            if (somethingWentWrong.GetType().IsAssignableFrom(typeof( MissingMethodException))) {
                return thisPageObjectLooksDodgy(pageObjectClass, NO_WEBDRIVER_CONSTRUCTOR_MESSAGE, somethingWentWrong);
            }
            if (somethingWentWrong.GetType().IsAssignableFrom( typeof(InvocationTargetException))) {
                return thisPageObjectLooksDodgy(pageObjectClass, "Failed to instantiate page",
                    ((InvocationTargetException)somethingWentWrong).getTargetException());
            }
            return thisPageObjectLooksDodgy(pageObjectClass, "Failed to instantiate page", somethingWentWrong);
        }
        private PageLooksDodgyException thisPageObjectLooksDodgy(Type pageObjectClass,
            String message,
            Exception  e)
        {
            return new PageLooksDodgyException("The page object " + pageObjectClass + " looks dodgy:\n" + message, e);
        }



        private Optional<PageObject> newPageObjectWithSimpleConstructor(Type pageObjectClass)
        { 
            try
            {
                var ctor =pageObjectClass.GetConstructor(null );
                var newPage =(PageObject) ctor.Invoke(new[] {this.driver});

                newPage.SetDriver(driver);
                return Optional.of(newPage);

            } catch (Exception e) {
                // Try a different constructor
            }
            return (Optional<PageObject>) Optional.absent();
        }

        private Optional<PageObject> newPageObjectWithDriver(Type pageObjectClass) { 
            try
            {
                var ctor = pageObjectClass.GetConstructor(new[] { typeof(IWebDriver) });
                PageObject newPage = (PageObject) ctor.Invoke(new[] { this.driver });

                newPage.SetDriver(driver);
                return Optional.of(newPage);

            } catch (Exception e) {
                // Try a different constructor
            }
            return (Optional<PageObject>) Optional.absent();
        }

        private PageLooksDodgyException thisPageObjectLooksDodgy(PageObject pageObjectClass,
            String message,
            Exception e)
        {
            return new PageLooksDodgyException("The page object " + pageObjectClass + " looks dodgy:\n" + message, e);
        }
    }
}