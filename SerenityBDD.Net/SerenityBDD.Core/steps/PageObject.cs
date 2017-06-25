using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using log4net;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SerenityBDD.Core.Steps;

using SerenityBDD.Core.time;
using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.Steps
{
    public class PageObject

    {

        public IWebDriver WebDriver { get; set; }

        private static readonly int WAIT_FOR_ELEMENT_PAUSE_LENGTH = 250;

        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(PageObject));

        private IWebDriver driver;

        private Pages pages;

        private MatchingPageExpressions matchingPageExpressions;

        private RenderedPageObjectView renderedView;

        private PageUrls pageUrls;

        private SystemClock clock;

        private Duration waitForTimeout;
        private Duration waitForElementTimeout;

        private readonly Sleeper sleeper;
        private readonly Clock webdriverClock;
        private JavascriptExecutorFacade javascriptExecutorFacade;

        private EnvironmentVariables environmentVariables;

        public void setImplicitTimeout(int duration, TimeUnit unit)
        {
            waitForElementTimeout = new Duration(duration, unit);
            setDriverImplicitTimeout(waitForElementTimeout);
        }

        private void setDriverImplicitTimeout(Duration implicitTimeout)
        {
            if (driver.instanceof(typeof(ConfigurableTimeouts)))
            {
                ((ConfigurableTimeouts) driver).setImplicitTimeout(implicitTimeout);
            }
            else
            {
                driver.Manage().Timeouts().ImplicitWait = implicitTimeout.TimeSpan;

            }
        }


        public void resetImplicitTimeout()
        {
            if (driver.instanceof(typeof(ConfigurableTimeouts)))
            {
                waitForElementTimeout = ((ConfigurableTimeouts) driver).resetTimeouts();
            }
            else
            {
                waitForElementTimeout = getDefaultImplicitTimeout();
                driver.Manage().Timeouts().ImplicitWait = waitForElementTimeout;
            }
        }

        private Duration getDefaultImplicitTimeout()
        {
            var configuredTimeout =
                ThucydidesSystemProperty.WEBDRIVER_TIMEOUTS_IMPLICITLYWAIT.integerFrom(environmentVariables);
            return new Duration(configuredTimeout, TimeUnit.MILLISECONDS);

        }

        private enum OpenMode
        {
            CHECK_URL_PATTERNS,
            IGNORE_URL_PATTERNS
        }

        protected PageObject()
        {
            webdriverClock = new SystemClock();
            //TODO: Replace injectors
            //this.clock = Injectors.getInjector().getInstance<SystemClock>();
            //this.environmentVariables = Injectors.getInjector().getProvider(EnvironmentVariables.class).get();
            //this.environmentVariables = Injectors.getInjector().getInstance<EnvironmentVariables>();

            this.sleeper = Sleeper.SYSTEM_SLEEPER;
            setupPageUrls();
        }


        protected PageObject(IWebDriver driver, Action<PageObject> callback) : this()
        {
            setDriver(driver);

            callback(this);
        }


        public PageObject(IWebDriver driver, EnvironmentVariables environmentVariables) : this()
        {

            this.environmentVariables = environmentVariables;
            setDriver(driver);
        }

        protected void setDriver(IWebDriver driver, TimeSpan timeout)
        {
            this.driver = driver;
            new DefaultPageObjectInitialiser(driver, timeout).apply(this);
        }

        public void setDriver(IWebDriver driver)
        {
            setDriver(driver, getImplicitWaitTimeout());

        }

        public PageObject withDriver(IWebDriver driver)
        {
            setDriver(driver);
            return this;
        }

        public Duration getWaitForTimeout()
        {

            if (waitForTimeout == null)
            {
                int configuredWaitForTimeoutInMilliseconds =
                        ThucydidesSystemProperty.WEBDRIVER_WAIT_FOR_TIMEOUT
                            .integerFrom(environmentVariables, (int) DefaultTimeouts.DEFAULT_WAIT_FOR_TIMEOUT)
                    ;
                waitForTimeout = new Duration(configuredWaitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
            }
            return waitForTimeout;
        }

        [Obsolete]
        public Duration getWaitForElementTimeout()
        {
            return getImplicitWaitTimeout();
        }

        public Duration getImplicitWaitTimeout()
        {

            if (waitForElementTimeout == null)
            {
                int configuredWaitForTimeoutInMilliseconds =
                    ThucydidesSystemProperty.WEBDRIVER_TIMEOUTS_IMPLICITLYWAIT
                        .integerFrom(environmentVariables, (int) DefaultTimeouts.DEFAULT_IMPLICIT_WAIT_TIMEOUT);
                
                waitForElementTimeout = new Duration(configuredWaitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
            }
            return waitForElementTimeout;
        }

        public void setPages(Pages pages)
        {
            this.pages = pages;
        }

        public T switchToPage<T>(Type pageObjectClass)
            where T : PageObject
        {
            if (pages.getDriver() == null)
            {
                pages.setDriver(driver);
            }

            return (T) pages.getPage(pageObjectClass);
        }

        public FileToUpload upload(string filename)
        {
            return new FileToUpload(driver, filename).useRemoteDriver(isDefinedRemoteUrl());
        }


        public FileToUpload uploadData(string data)
        {
            var datafile = Files.createTempFile("upload", "data");
            Files.write(datafile, System.Text.Encoding.UTF8.GetBytes(data));
            return new FileToUpload(driver, datafile.toAbsolutePath().ToString()).useRemoteDriver(isDefinedRemoteUrl());
        }

        public FileToUpload uploadData(byte[] data)
        {
            var datafile = Files.createTempFile("upload", "data");
            Files.write(datafile, data);
            return new FileToUpload(driver, datafile.toAbsolutePath().ToString()).useRemoteDriver(isDefinedRemoteUrl());
        }

        private bool isDefinedRemoteUrl()
        {
            bool isRemoteUrl = ThucydidesSystemProperty.WEBDRIVER_REMOTE_URL.isDefinedIn(environmentVariables);
            bool isSaucelabsUrl = ThucydidesSystemProperty.SAUCELABS_URL.isDefinedIn(environmentVariables);
            bool isBrowserStack = ThucydidesSystemProperty.BROWSERSTACK_URL.isDefinedIn(environmentVariables);
            return isRemoteUrl || isSaucelabsUrl || isBrowserStack;
        }

        private void setupPageUrls()
        {
            setPageUrls(new PageUrls(this));
        }

        /**
         * Only for testing purposes.
         */

        public void setPageUrls(PageUrls pageUrls)
        {
            this.pageUrls = pageUrls;
        }

        public void setWaitForTimeout(long waitForTimeoutInMilliseconds)
        {
            this.waitForTimeout = new Duration(waitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
            getRenderedView().setWaitForTimeout(this.waitForTimeout);
        }

        public void setWaitForElementTimeout(long waitForTimeoutInMilliseconds)
        {
            this.waitForElementTimeout = new Duration(waitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
        }

        protected RenderedPageObjectView getRenderedView()
        {
            if (renderedView == null)
            {
                renderedView = new RenderedPageObjectView(driver, this, getWaitForTimeout(), true);
            }
            return renderedView;
        }

        protected SystemClock getClock()
        {
            return clock;
        }

        private MatchingPageExpressions getMatchingPageExpressions()
        {
            if (matchingPageExpressions == null)
            {
                matchingPageExpressions = new MatchingPageExpressions(this);
            }
            return matchingPageExpressions;
        }

        public IWebDriver getDriver()
        {
            return driver;
        }

        public string getTitle()
        {
            return driver.Title;
        }


        public bool matchesAnyUrl()
        {
            return thereAreNoPatternsDefined();
        }

        /**
         * Does this page object work for this URL? When matching a URL, we check
         * with and without trailing slashes
         */

        public bool compatibleWithUrl(string currentUrl)
        {
            return thereAreNoPatternsDefined() || matchUrlAgainstEachPattern(currentUrl);
        }

        private bool matchUrlAgainstEachPattern(string currentUrl)
        {
            return getMatchingPageExpressions().matchUrlAgainstEachPattern(currentUrl);
        }

        private bool thereAreNoPatternsDefined()
        {
            return getMatchingPageExpressions().isEmpty();
        }

        public PageObject waitForRenderedElements(
            By byElementCriteria
        )
        {
            getRenderedView().waitFor(byElementCriteria);
            return this;
        }

        public RenderedPageObjectView withTimeoutOf(int timeout, TimeUnit units)
        {
            return withTimeoutOf(new Duration(timeout, units));
        }

        public RenderedPageObjectView withTimeoutOf(Duration timeout)
        {
            return new RenderedPageObjectView(driver, this, timeout, false);
        }

        public By xpathOrCssSelector(string src)
        {
            throw new NotImplementedException();
        }

        public PageObject waitFor(string xpathOrCssSelector)
        {
            return waitForRenderedElements(this.xpathOrCssSelector(xpathOrCssSelector));
        }

        public PageObject waitFor(ExpectedCondition expectedCondition)
        {
            getRenderedView().waitFor(expectedCondition);
            return this;
        }

        public PageObject waitForRenderedElementsToBePresent(
            By byElementCriteria
        )
        {
            getRenderedView().waitForPresenceOf(byElementCriteria);

            return this;
        }

        public PageObject waitForPresenceOf(string xpathOrCssSelector)
        {
            return waitForRenderedElementsToBePresent(this.xpathOrCssSelector(xpathOrCssSelector));
        }


        public PageObject waitForRenderedElementsToDisappear(
            By byElementCriteria
        )
        {
            getRenderedView().waitForElementsToDisappear(byElementCriteria);
            return this;
        }

        public PageObject waitForAbsenceOf(string xpathOrCssSelector)
        {
            return waitForRenderedElementsToDisappear(this.xpathOrCssSelector(xpathOrCssSelector));
        }

        /**
         * Waits for a given text to appear anywhere on the page.
         */

        public PageObject waitForTextToAppear(string expectedText)
        {
            getRenderedView().waitForText(expectedText);
            return this;
        }

        public PageObject waitForTitleToAppear(string expectedTitle)
        {
            waitOnPage().until(ExpectedConditions.titleIs(expectedTitle));
            return this;
        }

        private WebDriverWait waitOnPage()
        {
            return new WebDriverWait(driver, getWaitForTimeout());
            //        waitForTimeoutInSecondsWithAMinimumOfOneSecond());
        }

        public PageObject waitForTitleToDisappear(
            string expectedTitle
        )
        {
            getRenderedView().waitForTitleToDisappear(expectedTitle);
            return this;
        }

        /**
         * Waits for a given text to appear inside the element.
         */

        public PageObject waitForTextToAppear(IWebElement element, string expectedText)
        {
            getRenderedView().waitForText(element, expectedText);
            return this;
        }

        private bool driverIsDisabled()
        {
            return StepEventBus.getEventBus().webdriverCallsAreSuspended();
        }

        /**
         * Waits for a given text to disappear from the element.
         */

        public PageObject waitForTextToDisappear(IWebElement element,string expectedText)
        {
            if (!driverIsDisabled())
            {
                waitForCondition()?.until(elementDoesNotContain(element, expectedText));
            }
            return this;
        }

        private Wait waitForCondition()
        {
            throw new NotImplementedException();
        }


        private ExpectedCondition elementDoesNotContain(IWebElement element, string expectedText)
        {

            var wt = new WebDriverWait(this.WebDriver, getImplicitWaitTimeout());
            return wt.until(driver => element.Text.Contains(expectedText));
            
        }

        public PageObject waitForTextToDisappear(string expectedText)
        {
            return waitForTextToDisappear(expectedText,getWaitForTimeout());
        }

        /**
         * Waits for a given text to not be anywhere on the page.
         */
        public
        PageObject
        waitForTextToDisappear
        (
        string
        expectedText
        ,
        TimeSpan
        timespan
        )
        {
            getRenderedView
            (
            )
            .
            waitForTextToDisappear
            (
            expectedText
            ,
            timespan
            )
            ;
            return
            this
            ;
        }
        public
        PageObject
        waitForTextToDisappear
        (
        string
        expectedText
        ,
        long
        timeoutInMilliseconds
        )
        {

            getRenderedView
            (
            )
            .
            waitForTextToDisappear
            (
            expectedText
            ,
            TimeSpan
            .
            FromMilliseconds
            (
            timeoutInMilliseconds
            )
            )
            ;
            return
            this
            ;
        }

        /**
         * Waits for a given text to appear anywhere on the page.
         */

        public
        PageObject
        waitForTextToAppear
        (
        string
        expectedText
        ,
        TimeSpan
        timeout
        )
        {

            getRenderedView
            (
            )
            .
            waitForTextToAppear
            (
            expectedText
            ,
            timeout
            )
            ;
            return
            this
            ;
        }

        /**
         * Waits for any of a number of text blocks to appear anywhere on the
         * screen.
         */

        public
        PageObject
        waitForAnyTextToAppear
        (
        params
        string
        [
        ]
        expectedText
        )
        {
            getRenderedView
            (
            )
            .
            waitForAnyTextToAppear
            (
            expectedText
            )
            ;
            return
            this
            ;
        }

        public
        PageObject
        waitForAnyTextToAppear
        (
        IWebElement
        element
        ,

        string
        .
        .
        .

        expectedText

        )
        {
            getRenderedView
            (
            )
            .
            waitForAnyTextToAppear
            (
            element
            ,
            expectedText
            )
            ;
            return
            this
            ;
        }

        /**
         * Waits for all of a number of text blocks to appear on the screen.
         */

        public
        PageObject
        waitForAllTextToAppear
        (

        string
        .
        .
        .

        expectedTexts

        )
        {
            getRenderedView
            (
            )
            .
            waitForAllTextToAppear
            (
            expectedTexts
            )
            ;
            return
            this
            ;
        }

        public
        PageObject
        waitForAnyRenderedElementOf
        (

        By
        .

        .
        .
        expectedElements

        )
        {
            getRenderedView
            (
            )
            .
            waitForAnyRenderedElementOf
            (
            expectedElements
            )
            ;
            return
            this
            ;
        }

        protected
        void
        waitABit
        (
        long
        timeInMilliseconds
        )
        {
            getClock
            (
            )
            .
            pauseFor
            (
            timeInMilliseconds
            )
            ;
        }

        public WaitForBuilder<? extends

        PageObject
        >

        waitFor(int duration)
        {
            return new PageObjectStepDelayer(clock, this).waitFor(duration);
        }

        public List<IWebElement> thenReturnElementList(
            By byListCriteria
        )
        {
            return driver.findElements(byListCriteria);
        }

        public <
        T extends
        PageObject
        >

        T foo()
        {
            return (T) this;
        }

        /**
         * Check that the specified text appears somewhere in the page.
         */

        public void shouldContainText(
            string textValue
        )
        {
            if (!containsText(textValue))
            {
                string errorMessage = string.format(
                    "The text '%s' was not found in the page", textValue);
                throw new NoSuchElementException(errorMessage);
            }
        }

        /**
         * Check that all of the specified texts appears somewhere in the page.
         */

        public void shouldContainAllText(

            string .
                ..

                textValues

        )
        {
            if (!containsAllText(textValues))
            {
                string errorMessage = string.format(
                    "One of the text elements in '%s' was not found in the page", (Object[]) textValues);
                throw new NoSuchElementException(errorMessage);
            }
        }

        /**
         * Does the specified web element contain a given text value. Useful for dropdowns and so on.
         *
         * [Obsolete] use element(IWebElement).containsText(textValue)
         */

        [Obsolete]
        public bool containsTextInElement(
            IWebElement element,
            string textValue
        )
        {
            return element(IWebElement).containsText(textValue);
        }

        /*
         * Check that the element contains a given text.
         * [Obsolete] use element(IWebElement).shouldContainText(textValue)
         */

        [Obsolete]
        public void shouldContainTextInElement(
            IWebElement element,
            string textValue
        )
        {
            element(IWebElement).shouldContainText(textValue);
        }

        /*
         * Check that the element does not contain a given text.
         * [Obsolete] use element(IWebElement).shouldNotContainText(textValue)
         */

        [Obsolete]
        public void shouldNotContainTextInElement(
            IWebElement element,
            string textValue
        )
        {
            element(IWebElement).shouldNotContainText(textValue);
        }

        /**
         * Clear a field and enter a value into it.
         */

        public void typeInto(
            IWebElement field,
            string value
        )
        {
            element(field).type(value);
        }

        /**
         * Clear a field and enter a value into it.
         * This is a more fluent alternative to using the typeInto method.
         */

        public FieldEntry enter(
            string value
        )
        {
            return new FieldEntry(value);
        }

        public void selectFromDropdown(
            IWebElement dropdown,
            string visibleLabel
        )
        {

            Dropdown.forWebElement(dropdown).select(visibleLabel);
            notifyScreenChange();
        }

        public void selectMultipleItemsFromDropdown(
            IWebElement dropdown,

            string .
                ..

                selectedLabels

        )
        {
            Dropdown.forWebElement(dropdown).selectMultipleItems(selectedLabels);
            notifyScreenChange();
        }


        public Set<string> getSelectedOptionLabelsFrom(
            IWebElement dropdown
        )
        {
            return Dropdown.forWebElement(dropdown).getSelectedOptionLabels();
        }

        public Set<string> getSelectedOptionValuesFrom(
            IWebElement dropdown
        )
        {
            return Dropdown.forWebElement(dropdown).getSelectedOptionValues();
        }

        public string getSelectedValueFrom(
            IWebElement dropdown
        )
        {
            return Dropdown.forWebElement(dropdown).getSelectedValue();
        }

        public string getSelectedLabelFrom(
            IWebElement dropdown
        )
        {
            return Dropdown.forWebElement(dropdown).getSelectedLabel();
        }

        public void setCheckbox(
            IWebElement field,
            bool value
        )
        {
            Checkbox checkbox = new Checkbox(field);
            checkbox.setChecked(value);
            notifyScreenChange();
        }

        public bool containsText(
            string textValue
        )
        {
            return getRenderedView().containsText(textValue);
        }

        /**
         * Check that the specified text appears somewhere in the page.
         */

        public bool containsAllText(

            string .
                ..

                textValues

        )
        {
            for (string textValue :
            textValues)
            {
                if (!getRenderedView().containsText(textValue))
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * Fail the test if this element is not displayed (rendered) on the screen.
         */

        public void shouldBeVisible(
            IWebElement field
        )
        {
            element(field).shouldBeVisible();
        }

        public void shouldBeVisible(
            By byCriteria
        )
        {
            waitOnPage().until(ExpectedConditions.visibilityOfElementLocated(byCriteria));
        }

        public void shouldNotBeVisible(
            IWebElement field
        )
        {
            try
            {
                element(field).shouldNotBeVisible();
            }
            catch (NoSuchElementException e)
            {
                // A non-existant element is not visible
            }
        }

        public void shouldNotBeVisible(
            By byCriteria
        )
        {
            List<IWebElement> matchingElements = getDriver().findElements(byCriteria);
            if (!matchingElements.isEmpty())
            {
                waitOnPage().until(ExpectedConditions.invisibilityOfElementLocated(byCriteria));
            }
        }

        private long waitForTimeoutInSecondsWithAMinimumOfOneSecond()
        {
            return (getWaitForTimeout().in
            (TimeUnit.SECONDS) < 1) ?
            1 :
            (getWaitForTimeout().in
            (TimeUnit.SECONDS))
            ;
        }

        public long waitForTimeoutInMilliseconds()
        {
            return getWaitForTimeout().in
            (MILLISECONDS);
        }

        public long implicitTimoutMilliseconds()
        {
            return getImplicitWaitTimeout().in
            (MILLISECONDS);
        }

        public string updateUrlWithBaseUrlIfDefined(
            string startingUrl
        )
        {

            string baseUrl = pageUrls.getSystemBaseUrl();
            if ((baseUrl != null) && (!StringUtils.isEmpty(baseUrl)))
            {
                return replaceHost(startingUrl, baseUrl);
            }
            else
            {
                return startingUrl;
            }
        }

        private string replaceHost(
            string starting,
            string
                base)
        {

            string updatedUrl = starting;
            try
            {
                URL startingUrl = new URL(starting);
                URL baseUrl = new URL(base);

                string startingHostComponent = hostComponentFrom(startingUrl.getProtocol(),
                    startingUrl.getHost(),
                    startingUrl.getPort());
                string baseHostComponent = hostComponentFrom(baseUrl.getProtocol(),
                    baseUrl.getHost(),
                    baseUrl.getPort());
                updatedUrl = starting.replaceFirst(startingHostComponent, baseHostComponent);
            }
            catch (MalformedURLException e)
            {
                LOGGER.error("Failed to analyse default page URL: Starting URL: {}, Base URL: {}", starting, base);
                LOGGER.error("URL analysis failed with exception:", e);
            }

            return updatedUrl;
        }

        private string hostComponentFrom(
            string protocol,
            string host,
            int port
        )
        {
            stringBuilder hostComponent = new stringBuilder(protocol);
            hostComponent.append("://");
            hostComponent.append(host);
            if (port > 0)
            {
                hostComponent.append(":");
                hostComponent.append(port);
            }
            return hostComponent.ToString();
        }

        /**
         * Open the IWebDriver browser using a paramaterized URL. Parameters are
         * represented in the URL using {0}, {1}, etc.
         */

        public void open(
            string[] parameterValues
        )
        {
            open(OpenMode.CHECK_URL_PATTERNS, parameterValues);
        }

        /**
         * Opens page without checking URL patterns. Same as open(string...)) otherwise.
         */

        public void openUnchecked(

            string .
                ..

                parameterValues

        )
        {
            open(OpenMode.IGNORE_URL_PATTERNS, parameterValues);
        }

        private void open(
            OpenMode openMode,

            string .
                ..

                parameterValues

        )
        {
            string startingUrl = pageUrls.getStartingUrl(parameterValues);
            LOGGER.debug("Opening page at url {}", startingUrl);
            openPageAtUrl(startingUrl);
            checkUrlPatterns(openMode);
            initializePage();
            LOGGER.debug("Page opened");
        }

        public void open(
            string urlTemplateName,
            string[] parameterValues
        )
        {
            open(OpenMode.CHECK_URL_PATTERNS, urlTemplateName, parameterValues);
        }

        /**
         * Opens page without checking URL patterns. Same as {@link #open(string, string[])} otherwise.
         */

        public void openUnchecked(
            string urlTemplateName,
            string[] parameterValues
        )
        {
            open(OpenMode.IGNORE_URL_PATTERNS, urlTemplateName, parameterValues);
        }

        private void open(
            OpenMode openMode,
            string urlTemplateName,
            string[] parameterValues
        )
        {
            string startingUrl = pageUrls.getNamedUrl(urlTemplateName,
                parameterValues);
            LOGGER.debug("Opening page at url {}", startingUrl);
            openPageAtUrl(startingUrl);
            checkUrlPatterns(openMode);
            initializePage();
            LOGGER.debug("Page opened");
        }

        /**
         * Open the IWebDriver browser to the base URL, determined by the DefaultUrl
         * annotation if present. If the DefaultUrl annotation is not present, the
         * default base URL will be used. If the DefaultUrl annotation is present, a
         * URL based on the current base url from the system-wide default url
         * and the relative path provided in the DefaultUrl annotation will be used to
         * determine the URL to open. For example, consider the following class:
         * <pre>
         *     <code>
         *         &#064;DefaultUrl("http://localhost:8080/client/list")
         *         public class ClientList extends PageObject {
         *             ...
         *
         *             &#064;WhenPageOpens
         *             public void waitUntilTitleAppears() {...}
         *         }
         *     </code>
         * </pre>
         * Suppose you are using a base URL of http://stage.acme.com. When you call open() for this class,
         * it will open http://stage.acme.com/client/list. It will then invoke the waitUntilTitleAppears() method.
         */

        public void open()
        {
            open(OpenMode.CHECK_URL_PATTERNS);
        }

        /**
         * Opens page without checking URL patterns. Same as {@link #open()} otherwise.
         */

        public void openUnchecked()
        {
            open(OpenMode.IGNORE_URL_PATTERNS);
        }

        private void open(
            OpenMode openMode
        )
        {
            string startingUrl = updateUrlWithBaseUrlIfDefined(pageUrls.getStartingUrl());
            openPageAtUrl(startingUrl);
            checkUrlPatterns(openMode);
            initializePage();
        }

        private void initializePage()
        {
            addJQuerySupport();
            callWhenPageOpensMethods();
        }

        private void checkUrlPatterns(
            OpenMode openMode
        )
        {
            if (openMode == OpenMode.CHECK_URL_PATTERNS)
            {
                ensurePageIsOnAMatchingUrl();
            }
        }

        private void ensurePageIsOnAMatchingUrl()
        {
            if (!matchesAnyUrl())
            {
                string currentUrl = getDriver().getCurrentUrl();
                if (!compatibleWithUrl(currentUrl))
                {
                    thisIsNotThePageYourLookingFor();
                }
            }
        }

        /**
         * Use the @At annotation (if present) to check that a page object is displaying the correct page.
         * Will throw an exception if the current URL does not match the expected one.
         */

        public void shouldBeDisplayed()
        {
            ensurePageIsOnAMatchingUrl();
        }

        private void thisIsNotThePageYourLookingFor()
        {

            string errorDetails = "This is not the page you're looking for: "
                                  + "I was looking for a page compatible with " + this.getClass() + " but "
                                  + "I was at the URL " + getDriver().getCurrentUrl();

            throw new WrongPageError(errorDetails);
        }

        public void openAt(string startingUrl)
        {
            openPageAtUrl(updateUrlWithBaseUrlIfDefined(startingUrl));
            callWhenPageOpensMethods();
        }

        /**
         * Override this method
         */

        public void callWhenPageOpensMethods()
        {
            foreach (var annotatedMethod in methodsAnnotatedWithWhenPageOpens())
            {
                try
                {
                    annotatedMethod.Invoke(this, new object[] {});
                }
                catch (AssertionException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    LOGGER.Error("Could not execute @WhenPageOpens annotated method: ", e);
                    if (e is InvocationTargetException)
                    {
                        e = ((InvocationTargetException) e).getTargetException();
                    }

                    throw new UnableToInvokeWhenPageOpensMethods(annotatedMethod, e);

                }
            }
        }

        private IEnumerable<MethodInfo> methodsAnnotatedWithWhenPageOpens()
        {
            var methods = MethodFinder.inClass(this.GetType()).getAllMethods();
            var annotatedMethods = new List<MethodInfo>();
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<WhenPageOpens>();
                if (attr != null)
                {
                    if (method.GetParameters().Length == 0)
                    {
                        annotatedMethods.Add(method);
                    }
                    else
                    {
                        throw new PageOpenMethodCannotHaveParametersException(method);
                    }
                }
            }
            return annotatedMethods;
        }

        public static string[] withParameters(params string[] parameterValues)
        {
            return parameterValues;
        }

        private void openPageAtUrl(string startingUrl)
        {
            getDriver().Navigate().GoToUrl(startingUrl);
            if (javascriptIsSupportedIn(getDriver()))
            {
                addJQuerySupport();
            }
        }


        private bool javascriptIsSupportedIn(IWebDriver webDriver)
        {
            throw new NotImplementedException();
        }

        public void clickOn(IWebElement element)
        {

            element.Click();
        }

        /**
         * Returns true if at least one matching element is found on the page and is visible.
         */

        public bool isElementVisible(By byCriteria)
        {
            return getRenderedView().elementIsDisplayed(byCriteria);
        }

        public void setDefaultBaseUrl(string defaultBaseUrl)
        {
            pageUrls.overrideDefaultBaseUrl(defaultBaseUrl);
        }

        /**
         * Returns true if the specified element has the focus.
         *
         * [Obsolete] Use element(IWebElement).hasFocus() instead
         */

        public bool hasFocus(IWebElement element)
        {
            return element.Equals(driver.SwitchTo().ActiveElement());

        }

        public void blurActiveElement()
        {
            getJavascriptExecutorFacade().executeScript("document.activeElement.blur();");
        }

        protected JavascriptExecutorFacade getJavascriptExecutorFacade()
        {
            if (javascriptExecutorFacade == null)
            {
                javascriptExecutorFacade = new JavascriptExecutorFacade(driver);
            }
            return javascriptExecutorFacade;
        }

        /**
         * Provides a fluent API for querying web elements.
         */

        public WebElementFacade element(IWebElement element)
        {
            return WebElementFacadeImpl.wrapWebElement(driver, element, getImplicitWaitTimeout(), getWaitForTimeout(),
                nameOf(element));
        }

        private string nameOf(IWebElement element)
        {
            try
            {
                return element.ToString();
            }
            catch (Exception e)
            {
                return "Unknown web element";
            }
        }


        public WebElementFacade JQuery(IWebElement element)
        {
            return this.element(element);
        }

        public WebElementFacade JQuery(string xpathOrCssSelector)
        {
            return this.element(xpathOrCssSelector);
        }

        /**
         * Provides a fluent API for querying web elements.
         */

        public WebElementFacade element(By bySelector)
        {
            return WebElementFacadeImpl.wrapWebElement(driver, bySelector, getImplicitWaitTimeout(), getWaitForTimeout(),
                bySelector.ToString());
        }

        public WebElementFacade find(IEnumerable<By> selectors)
        {
            WebElementFacade e = null;
            foreach (var selector in selectors)
            {
                if (e == null)
                {
                    e = this.element(selector);
                }
                else
                {
                    e = e.find(selector);
                }
            }
            return e;
        }

        public WebElementFacade find(params By[] selectors)
        {
            return find(selectors.ToList());
        }

        public List<WebElementFacade> findAll(By bySelector)
        {
            var matchingWebElements = driver.FindElements(bySelector);
            return convert(matchingWebElements, toWebElementFacades());
        }

        private List<TTGT> convert<TSRC, TTGT>(ReadOnlyCollection<TSRC> matchingWebElements, Converter<TSRC, TTGT> converter)
        {
            return converter.Convert(matchingWebElements).ToList();
        }

        private Converter<IWebElement, WebElementFacade> toWebElementFacades()
        {
            return new Converter<IWebElement, WebElementFacade>();

        }

        /**
* Provides a fluent API for querying web elements.
*/
        public WebElementFacade element(string xpathOrCssSelector)
        {
            return element(this.xpathOrCssSelector(xpathOrCssSelector));
        }

    }
}

/*
 * 

public <
        T extends
        net.serenitybdd.core.pages.WebElementFacade
        >

        T findBy(string xpathOrCssSelector)
{
    return element(xpathOrCssSelector);
}

public List<net.serenitybdd.core.pages.WebElementFacade> findAll(string xpathOrCssSelector)
{
    return findAll(xpathOrCssSelector(xpathOrCssSelector));
}

public bool containsElements(By bySelector)
{
    return !findAll(bySelector).isEmpty();
}

public bool containsElements(string xpathOrCssSelector)
{
    return !findAll(xpathOrCssSelector).isEmpty();
}


public Object evaluateJavascript(string script)
{
    addJQuerySupport();
    JavascriptExecutorFacade js = new JavascriptExecutorFacade(driver);
    return js.executeScript(script);
}

public Object evaluateJavascript(string script, params Object[] args)
{
    addJQuerySupport();
    JavascriptExecutorFacade js = new JavascriptExecutorFacade(driver);
    return js.executeScript(script, args)
    ;
}

public void addJQuerySupport()
{
    if (pageIsLoaded() && jqueryIntegrationIsActivated() && driverIsJQueryCompatible())
    {
        JQueryEnabledPage jQueryEnabledPage = JQueryEnabledPage.withDriver(getDriver());
        jQueryEnabledPage.activateJQuery();
    }
}

protected bool driverIsJQueryCompatible()
{
    try
    {
        if (getDriver()
                    instanceof WebDriverFacade)
                {
            return SupportedWebDriver.forClass(((WebDriverFacade)getDriver()).getDriverClass())
                .supportsJavascriptInjection();
        }
        return SupportedWebDriver.forClass(getDriver().getClass()).supportsJavascriptInjection();
    }
    catch (IllegalArgumentException probablyAMockedDriver)
    {
        return false;
    }
}

private bool jqueryIntegrationIsActivated()
{
    return THUCYDIDES_JQUERY_INTEGRATION.booleanFrom(environmentVariables, true);
}

public RadioButtonGroup inRadioButtonGroup(string name)
{
    return new RadioButtonGroup(getDriver().findElements(By.name(name)));
}

private bool pageIsLoaded()
{
    try
    {
        return (driverIsInstantiated() && getDriver().getCurrentUrl() != null);
    }
    catch (WebDriverException e)
    {
        return false;
    }
}

protected bool driverIsInstantiated()
{
    if (getDriver()
                instanceof WebDriverFacade)
            {
        return ((WebDriverFacade)getDriver()).isEnabled() && ((WebDriverFacade)getDriver()).isInstantiated();
    }
    return true;
}

public ThucydidesFluentWait<IWebDriver> waitForWithRefresh()
{
    return new FluentWaitWithRefresh<>(driver, webdriverClock, sleeper)
        .withTimeout(getWaitForTimeout(),
    TimeUnit.MILLISECONDS)
        .
    pollingEvery(WAIT_FOR_ELEMENT_PAUSE_LENGTH, TimeUnit.MILLISECONDS)
        .ignoring(NoSuchElementException.class,
            NoSuchFrameException.class)
            ;
        }

public ThucydidesFluentWait<IWebDriver> waitForCondition()
{
    return new NormalFluentWait<>(driver, webdriverClock, sleeper)
        .withTimeout(getWaitForTimeout(),
    TimeUnit.MILLISECONDS)
        .
    pollingEvery(WAIT_FOR_ELEMENT_PAUSE_LENGTH, TimeUnit.MILLISECONDS)
        .ignoring(NoSuchElementException.class,
            NoSuchFrameException.class)
            ;
        }

        public WebElementFacade waitFor(IWebElement element)
{
    return waitFor($(IWebElement))
    ;
}

public WebElementFacade waitFor(WebElementFacade IWebElement)
{
    return getRenderedView().waitFor(IWebElement);
}


public Alert getAlert()
{
    return driver.switchTo().alert();
}

public Actions withAction()
{
    IWebDriver proxiedDriver = ((WebDriverFacade)getDriver()).getProxiedDriver();
    return new Actions(proxiedDriver);
}

public class FieldEntry
{

    private string value;

    public FieldEntry(
        string value
    )
    {
        this.value = value;
    }

    public void into(
        IWebElement field
    )
    {
        element(field).type(value);
    }

    public void into(net.serenitybdd.core.pages.WebElementFacade field
    )
    {
        field.type(value);
    }

    public void intoField(
        By bySelector
    )
    {
        IWebElement field = getDriver().findElement(bySelector);
        into(field);
    }
}

private void notifyScreenChange()
{
    StepEventBus.getEventBus().notifyScreenChange();
}

protected ThucydidesFluentAdapter fluent()
{
    return new ThucydidesFluentAdapter(getDriver());
}

public T moveTo<T>(string xpathOrCssSelector)
    where T : WebElementFacade
{
    if (!driverIsDisabled())
    {
        withAction().moveToElement(findBy(xpathOrCssSelector)).perform();
    }
    return findBy(xpathOrCssSelector);
}

public WebElementFacade moveTo(By locator)
{
    if (!driverIsDisabled())
    {
        withAction().moveToElement(find(locator)).perform();
    }
    return find(locator);
}

public void waitForAngularRequestsToFinish()
{
    if ((bool)getJavascriptExecutorFacade().executeScript(
        "return (typeof angular !== 'undefined')? true : false;"))
    {
        getJavascriptExecutorFacade().executeAsyncScript(
            "var callback = arguments[arguments.length - 1];"
            +
            "angular.element(document.body).injector().get('$browser').notifyWhenNoOutstandingRequests(callback);");
    }
}

Inflector inflection = Inflector.getInstance();


public override string ToString()
{
    return inflection.of(getClass().getSimpleName())
        .inHumanReadableForm().ToString();
}



    

}


*/


