using System;
using System.Drawing.Design;
using System.IO;
using System.Net.Http;
using OpenQA.Selenium;

namespace SerenityBDD.Core.steps
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
            var ret =(PageObject) ctor.Invoke(new[] {this.driver});
                
        ret.setDriver(driver);
            return Optional.of(newPage);

        } catch (NoSuchMethodException e) {
            // Try a different constructor
        }
        return Optional.absent();
    }

    private Optional<PageObject> newPageObjectWithDriver(Type pageObjectClass) { 
            try
        {
            var ctor = pageObjectClass.GetConstructor(new[] { typeof(IWebDriver) });
        PageObject ret =(PageObject) ctor.Invoke(new[] { this.driver });

        ret.setDriver(driver);
            return Optional.of(newPage);

        } catch (NoSuchMethodException e) {
            // Try a different constructor
        }
        return Optional.absent();
    }

    private PageLooksDodgyException thisPageObjectLooksDodgy(readonly Class<? extends PageObject> pageObjectClass,
                                               String message,
                                               Throwable e)
{
    return new PageLooksDodgyException("The page object " + pageObjectClass + " looks dodgy:\n" + message, e);
}
    }

internal class PageLooksDodgyException : Exception
{
    
    public PageLooksDodgyException(string message, Exception e):base(message, e)
    {
    
    }
}
internal class InvocationTargetException : Exception
    {
        public Exception getTargetException()
        {
            return base.InnerException;
        }
    }

    public class Optional
    {
        protected  object _value;

    public bool isPresent()
    {
        return _value != null;
    }

        public Optional(object value)
        {
            _value = value;
        }
        public static Optional<T> of<T>(T src)
        where T : class 
        {
            return new Optional<T>(src);
        }

        
        public static Optional absent()
        {
            return new Optional(null );
        }

    public static Optional<T2> fromNullable<T2>(T2 src)
        where T2 : class
    {
        if (src != null) return new Optional<T2>(src);

        return (Optional<T2>)Optional.absent();
    }

}

public class Optional<T> : Optional
        where T : class
    {
    
        public static implicit operator T(Optional<T> myinstance)
        {
            return myinstance.get();
        }

        public T get()
        {
            return (T) _value;
        }

        public Optional(T value) : base(value)
        {

        }

    }

    public class Pages
    {
        public PageObject getPage(Type pageObjectClass)
        {
            throw new NotImplementedException();
        }
    }

    internal class MatchingPageExpressions
    {
    }


}