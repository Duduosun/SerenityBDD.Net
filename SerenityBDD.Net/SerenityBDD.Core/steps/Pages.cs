using System;
using System.Drawing.Design;
using System.IO;
using System.Net.Http;
using System.Reflection;
using log4net;
using OpenQA.Selenium;
using SerenityBDD.Core.Time;
using SerenityBDD.Core.Webdriver;

namespace SerenityBDD.Core.Steps
{
    public class Pages
    {
        private static readonly long serialVersionUID = 1L;

        private IWebDriver driver;

        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(Pages));

        private String defaultBaseUrl;

        private readonly Configuration configuration;

        private WebdriverProxyFactory proxyFactory;

        private bool usePreviousPage = false;

        public Pages(Configuration configuration)
        {
            this.configuration = configuration;
            proxyFactory = WebdriverProxyFactory.getFactory();
        }

        public Pages() : this(ConfiguredEnvironment.getConfiguration())
        {

        }


        public Pages(IWebDriver driver) : this(ConfiguredEnvironment.getConfiguration())
        {

            this.driver = driver;
        }

        public Pages(IWebDriver driver, Configuration configuration) : this(configuration)
        {

            this.driver = driver;
        }

        public void setDriver(IWebDriver driver)
        {
            this.driver = driver;
        }

        public IWebDriver getDriver()
        {
            return driver;
        }

        protected WebdriverProxyFactory getProxyFactory()
        {
            return proxyFactory;
        }

        public Configuration getConfiguration()
        {
            return configuration;
        }

        PageObject currentPage = null;

        public PageObject getAt(Type pageObjectClass)
        {
            return getPage(pageObjectClass);
        }

        public PageObject getPage(Type pageObjectClass)
        {
            PageObject pageCandidate = getCurrentPageOfType(pageObjectClass);
            pageCandidate.SetDefaultBaseUrl(getDefaultBaseUrl());
            return pageCandidate;
        }


        public PageObject get(Type pageObjectClass)
        {
            PageObject nextPage;
            if (shouldUsePreviousPage(pageObjectClass))
            {
                nextPage = currentPage;
            }
            else
            {
                PageObject pageCandidate = getCurrentPageOfType(pageObjectClass);
                pageCandidate.SetDefaultBaseUrl(getDefaultBaseUrl());
                cacheCurrentPage(pageCandidate);
                nextPage = pageCandidate;
            }
            usePreviousPage = false;
            return nextPage;
        }


        public PageObject currentPageAt(Type pageObjectClass)
        {
            PageObject nextPage;
            if (shouldUsePreviousPage(pageObjectClass))
            {
                nextPage = currentPage;
            }
            else
            {
                PageObject pageCandidate = getCurrentPageOfType(pageObjectClass);
                pageCandidate.SetDefaultBaseUrl(getDefaultBaseUrl());
                openBrowserIfRequiredFor(pageCandidate);
                checkUrlPatterns(pageObjectClass, pageCandidate);
                cacheCurrentPage(pageCandidate);
                nextPage = pageCandidate;
                nextPage.AddJQuerySupport();
            }
            usePreviousPage = false;
            return nextPage;
        }

        private void openBrowserIfRequiredFor(PageObject pageCandidate)
        {
            if (browserNotOpen())
            {
                openHeadlessDriverIfNotOpen();
                pageCandidate.Open();
            }
        }


        private void openHeadlessDriverIfNotOpen()
        {
            if (browserIsHeadless())
            {
                driver.Navigate().GoToUrl("about:blank");
            }
        }

        private bool browserNotOpen()
        {
            if (getDriver().instanceof(typeof(WebDriverFacade)))
            {
                return !((WebDriverFacade)getDriver()).isInstantiated();
            }
            else
            {
                return StringUtils.isEmpty(getDriver().Url);
            }
        }

        private bool browserIsHeadless()
        {
            if (getDriver().instanceof (typeof(WebDriverFacade))) {
                return ((WebDriverFacade)getDriver()).getProxiedDriver().instanceof (typeof(HtmlUnitDriver));
            } else {
                return getDriver().instanceof(typeof(HtmlUnitDriver));
            }
        }
        private void checkUrlPatterns(Type pageObjectClass, PageObject pageCandidate)
        {
            if (!pageCandidate.MatchesAnyUrl())
            {
                String currentUrl = getDriver().Url;
                if (!pageCandidate.CompatibleWithUrl(currentUrl))
                {
                    thisIsNotThePageYourLookingFor(pageObjectClass);
                }
            }
        }

        private bool shouldUsePreviousPage(Type pageObjectClass)
        {
            if (!usePreviousPage)
            {
                return false;
            }
            else
            {
                return currentPageIsSameTypeAs(pageObjectClass);
            }
        }

        private void cacheCurrentPage(PageObject newPage)
        {
            this.currentPage = newPage;
        }

        private bool currentPageIsSameTypeAs(Type pageObjectClass)
        {
            return (currentPage != null) && (currentPage.GetType().IsInstanceOfType(pageObjectClass));
        }

        public bool isCurrentPageAt(Type pageObjectClass)
        {
            try
            {
                PageObject pageCandidate = getCurrentPageOfType(pageObjectClass);
                String currentUrl = getDriver().Url;
                return pageCandidate.CompatibleWithUrl(currentUrl);
            }
            catch (WrongPageError e)
            {
                return false;
            }
        }


        /**
         * Create a new Page Object of the given type.
         * The Page Object must have a constructor
         *
         * @param pageObjectClass
         * @throws IllegalArgumentException
         */

        private PageObject getCurrentPageOfType(Type pageObjectClass)
        {
            PageObject currentPage = null;
            try
            {
                currentPage = createFromSimpleConstructor(pageObjectClass);
                if (currentPage == null)
                {
                    currentPage = createFromConstructorWithWebdriver(pageObjectClass);
                }
                if (hasPageFactoryProperty(currentPage))
                {
                    setPageFactory(currentPage);
                }

            }
            catch (MissingMethodException e)
            {
                LOGGER.WarnFormat("This page object does not appear have a constructor that takes a WebDriver parameter: {0} ({1})",
                        pageObjectClass, e.Message);
                thisPageObjectLooksDodgy(pageObjectClass, "This page object does not appear have a constructor that takes a WebDriver parameter");
            }
            catch (InvocationTargetException e)
            {
                // Unwrap the underlying exception
                LOGGER.WarnFormat("Failed to instantiate page of type {0} ({1})", pageObjectClass, e.getTargetException());
                thisPageObjectLooksDodgy(pageObjectClass, "Failed to instantiate page (" + e.getTargetException() + ")");
            }
            catch (Exception e)
            {
                //shouldn't even get here
                LOGGER.WarnFormat("Failed to instantiate page of type {0} ({1})", pageObjectClass, e);
                thisPageObjectLooksDodgy(pageObjectClass, "Failed to instantiate page (" + e + ")");
            }
            return currentPage;
        }

        private PageObject createFromSimpleConstructor(Type pageObjectClass)
        {
            PageObject newPage = null;

            var constructorArgs = new Type[] { };
            var constructor = pageObjectClass.GetConstructor(constructorArgs);
            newPage = (PageObject)constructor.Invoke(new object[] { });
            newPage.SetDriver(driver);

            return newPage;
        }

        private PageObject createFromConstructorWithWebdriver(Type pageObjectClass)
        {
            var constructorArgs = new[] { typeof(IWebDriver) };

            var constructor = pageObjectClass.GetConstructor(constructorArgs);

            return (PageObject)constructor.Invoke(new object[] { driver });
        }

        private bool hasPageFactoryProperty(Object pageObject)
        {
            Optional<FieldInfo> pagesField = Fields.of(pageObject.GetType()).withName("pages");
            return ((pagesField.isPresent()) && (pagesField.get().GetType().IsAssignableFrom(typeof(Pages))));
        }

        private void setPageFactory(Object pageObject)
        {
            Optional<FieldInfo> pagesField = Fields.of(pageObject.GetType()).withName("pages");
            if (pagesField.isPresent())
            {
                pagesField.get().SetValue(pageObject, this);
            }
        }


        private void thisPageObjectLooksDodgy(Type pageObjectClass, String message)
        {

            String errorDetails = "The page object " + pageObjectClass + " looks dodgy:\n" + message;
            throw new WrongPageError(errorDetails);
        }

        private void thisIsNotThePageYourLookingFor(Type pageObjectClass)
        {

            String errorDetails = "This is not the page you're looking for: "
                    + "I was looking for a page compatible with " + pageObjectClass + " but "
                    + "I was at the URL " + getDriver().Url;

            throw new WrongPageError(errorDetails);
        }

        /**
         * The default URL for this set of tests, or the system default URL if undefined.
         */
        public String getDefaultBaseUrl()
        {

            String baseUrl = defaultBaseUrl;
            if (!string.IsNullOrEmpty(getConfiguration().getBaseUrl()))
            {
                baseUrl = getConfiguration().getBaseUrl();
            }
            return baseUrl;
        }

        /**
         * Set a default base URL for a specific set of tests.
         */
        public void setDefaultBaseUrl(string defaultBaseUrl)
        {
            this.defaultBaseUrl = defaultBaseUrl;
        }

        public Pages onSamePage()
        {
            usePreviousPage = true;
            return this;
        }

        public static PageObject instrumentedPageObjectUsing(Type pageObjectClass, IWebDriver driver)
        {
            return (PageObject) PageObjects.usingDriver(driver).ofType(pageObjectClass);
        }
    }

    
}