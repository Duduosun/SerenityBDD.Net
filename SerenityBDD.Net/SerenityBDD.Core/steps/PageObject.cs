using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using log4net;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SerenityBDD.Core.steps;

using SerenityBDD.Core.time;

namespace SerenityBDD.Core.steps
{
    public class PageObject
    {

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
            if (driver.instanceof(typeof(ConfigurableTimeouts))) {
                waitForElementTimeout = ((ConfigurableTimeouts)driver).resetTimeouts();
            } else {
                waitForElementTimeout = getDefaultImplicitTimeout();
                driver.Manage().Timeouts().ImplicitWait = waitForElementTimeout;
            }
        }
        private Duration getDefaultImplicitTimeout()
        {
            var configuredTimeout = ThucydidesSystemProperty.WEBDRIVER_TIMEOUTS_IMPLICITLYWAIT.integerFrom(environmentVariables);
            return new Duration(configuredTimeout, TimeUnit.MILLISECONDS);

        }

        private enum OpenMode
        {
            CHECK_URL_PATTERNS,
            IGNORE_URL_PATTERNS
        }

        protected PageObject()
        {
            this.webdriverClock = new SystemClock();
            this.clock = Injectors.getInjector().getInstance<SystemClock>();
            //this.environmentVariables = Injectors.getInjector().getProvider(EnvironmentVariables.class).get();
            this.environmentVariables = Injectors.getInjector().getInstance<EnvironmentVariables>();

            this.sleeper = Sleeper.SYSTEM_SLEEPER;
            setupPageUrls();
        }
/*
        protected PageObject(readonly IWebDriver driver, Predicate<PageObject> callback):this()
        {
            this.driver = driver;
            callback.apply(this);
        }

        public PageObject(readonly IWebDriver driver, readonly int ajaxTimeout)
        {
            this();
            setDriver(driver, ajaxTimeout);
        }

        public PageObject(readonly IWebDriver driver)
        {
            this();
            ThucydidesWebDriverSupport.useDriver(driver);
            setDriver(driver);
        }

        public PageObject(readonly IWebDriver driver, readonly EnvironmentVariables environmentVariables)
        {
            this();
            this.environmentVariables = environmentVariables;
            setDriver(driver);
        }

        protected void setDriver(IWebDriver driver, long timeout)
        {
            this.driver = driver;
            new DefaultPageObjectInitialiser(driver, timeout).apply(this);
        }

        public void setDriver(IWebDriver driver)
        {
            setDriver(driver, getImplicitWaitTimeout().in(TimeUnit.MILLISECONDS));
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
                        .integerFrom(environmentVariables, (int)DefaultTimeouts.DEFAULT_WAIT_FOR_TIMEOUT.in(MILLISECONDS));
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
                        .integerFrom(environmentVariables, (int)DefaultTimeouts.DEFAULT_IMPLICIT_WAIT_TIMEOUT.in(MILLISECONDS));
                waitForElementTimeout = new Duration(configuredWaitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
            }
            return waitForElementTimeout;
        }

        public void setPages(Pages pages)
        {
            this.pages = pages;
        }

        public <T extends PageObject> T switchToPage(readonly Class<T> pageObjectClass)
        {
            if (pages.getDriver() == null)
            {
                pages.setDriver(driver);
            }

            return pages.getPage(pageObjectClass);
        }

        public FileToUpload upload(readonly string filename)
        {
            return new FileToUpload(driver, filename).useRemoteDriver(isDefinedRemoteUrl());
        }

        public FileToUpload uploadData(string data) throws IOException
        {
            Path datafile = Files.createTempFile("upload", "data");
        Files.write(datafile, data.getBytes(StandardCharsets.UTF_8));
        return new FileToUpload(driver, datafile.toAbsolutePath().tostring()).useRemoteDriver(isDefinedRemoteUrl());
    }

    

    internal class JavascriptExecutorFacade
    {
    }



    public FileToUpload uploadData(byte[] data) throws IOException
{
    Path datafile = Files.createTempFile("upload", "data");
    Files.write(datafile, data);
        return new FileToUpload(driver, datafile.toAbsolutePath().tostring()).useRemoteDriver(isDefinedRemoteUrl());
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

        public void setWaitForElementTimeout(
        readonly long waitForTimeoutInMilliseconds
        )
        {
            this.waitForElementTimeout = new Duration(waitForTimeoutInMilliseconds, MILLISECONDS);
        }

        protected RenderedPageObjectView getRenderedView()
        {
            if (renderedView == null)
            {
                renderedView = new RenderedPageObjectView(driver, this, getWaitForTimeout(), true);
            }
            return renderedView;
        }

        protected net.serenitybdd.core.time.SystemClock getClock()
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
            return driver.getTitle();
        }


        public bool matchesAnyUrl()
        {
            return thereAreNoPatternsDefined();
        }

/**
 * Does this page object work for this URL? When matching a URL, we check
 * with and without trailing slashes
 */
        public readonly bool compatibleWithUrl(
        readonly string currentUrl
        )
        {
            return thereAreNoPatternsDefined() || matchUrlAgainstEachPattern(currentUrl);
        }

        private bool matchUrlAgainstEachPattern(
        readonly string currentUrl
        )
        {
            return getMatchingPageExpressions().matchUrlAgainstEachPattern(currentUrl);
        }

        private bool thereAreNoPatternsDefined()
        {
            return getMatchingPageExpressions().isEmpty();
        }

        public PageObject waitForRenderedElements(
        readonly By byElementCriteria
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

        public PageObject waitFor(string xpathOrCssSelector)
        {
            return waitForRenderedElements(xpathOrCssSelector(xpathOrCssSelector));
        }

        public PageObject waitFor(ExpectedCondition expectedCondition)
        {
            getRenderedView().waitFor(expectedCondition);
            return this;
        }

        public PageObject waitForRenderedElementsToBePresent(
        readonly By byElementCriteria
        )
        {
            getRenderedView().waitForPresenceOf(byElementCriteria);

            return this;
        }

        public PageObject waitForPresenceOf(string xpathOrCssSelector)
        {
            return waitForRenderedElementsToBePresent(xpathOrCssSelector(xpathOrCssSelector));
        }


        public PageObject waitForRenderedElementsToDisappear(
        readonly By byElementCriteria
        )
        {
            getRenderedView().waitForElementsToDisappear(byElementCriteria);
            return this;
        }

        public PageObject waitForAbsenceOf(string xpathOrCssSelector)
        {
            return waitForRenderedElementsToDisappear(xpathOrCssSelector(xpathOrCssSelector));
        }

/**
 * Waits for a given text to appear anywhere on the page.
 */
        public PageObject waitForTextToAppear(
        readonly string expectedText
        )
        {
            getRenderedView().waitForText(expectedText);
            return this;
        }

        public PageObject waitForTitleToAppear(
        readonly string expectedTitle
        )
        {
            waitOnPage().until(ExpectedConditions.titleIs(expectedTitle));
            return this;
        }

        private WebDriverWait waitOnPage()
        {
            return new WebDriverWait(driver, getWaitForTimeout().in(TimeUnit.SECONDS))
            ;
//        waitForTimeoutInSecondsWithAMinimumOfOneSecond());
        }

        public PageObject waitForTitleToDisappear(
        readonly string expectedTitle
        )
        {
            getRenderedView().waitForTitleToDisappear(expectedTitle);
            return this;
        }

/**
 * Waits for a given text to appear inside the element.
 */
        public PageObject waitForTextToAppear(
        readonly WebElement element, 
        readonly string expectedText
        )
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
        public PageObject waitForTextToDisappear(
        readonly WebElement element, 
        readonly string expectedText
        )
        {
            if (!driverIsDisabled())
            {
                waitForCondition().until(elementDoesNotContain(element, expectedText));
            }
            return this;
        }


        private ExpectedCondition<bool> elementDoesNotContain(
        readonly WebElement element, 
        readonly string expectedText
        )
        {
            return new ExpectedCondition<bool>()
            {
                public bool apply(IWebDriver driver)
                {
                return !element.getText().contains(expectedText);
            }
            }
            ;
        }

        public PageObject waitForTextToDisappear(
        readonly string expectedText
        )
        {
            return waitForTextToDisappear(expectedText, getWaitForTimeout().in(MILLISECONDS))
            ;
        }

/**
 * Waits for a given text to not be anywhere on the page.
 */
        public PageObject waitForTextToDisappear(
        readonly string expectedText, 
        readonly long timeoutInMilliseconds
        )
        {

            getRenderedView().waitForTextToDisappear(expectedText, timeoutInMilliseconds);
            return this;
        }

/**
 * Waits for a given text to appear anywhere on the page.
 */
        public PageObject waitForTextToAppear(
        readonly string expectedText, 
        readonly long timeout
        )
        {

            getRenderedView().waitForTextToAppear(expectedText, timeout);
            return this;
        }

/**
 * Waits for any of a number of text blocks to appear anywhere on the
 * screen.
 */
        public PageObject waitForAnyTextToAppear(
        readonly
        string.
        ..
        expectedText
        )
        {
            getRenderedView().waitForAnyTextToAppear(expectedText);
            return this;
        }

        public PageObject waitForAnyTextToAppear(
        readonly WebElement element, 
        readonly
        string.
        ..
        expectedText
        )
        {
            getRenderedView().waitForAnyTextToAppear(element, expectedText);
            return this;
        }

/**
 * Waits for all of a number of text blocks to appear on the screen.
 */
        public PageObject waitForAllTextToAppear(
        readonly
        string.
        ..
        expectedTexts
        )
        {
            getRenderedView().waitForAllTextToAppear(expectedTexts);
            return this;
        }

        public PageObject waitForAnyRenderedElementOf(
        readonly
        By.
        ..
        expectedElements
        )
        {
            getRenderedView().waitForAnyRenderedElementOf(expectedElements);
            return this;
        }

        protected void waitABit(
        readonly long timeInMilliseconds
        )
        {
            getClock().pauseFor(timeInMilliseconds);
        }

        public WaitForBuilder<? extends
        PageObject
        >

        waitFor(int duration)
        {
            return new PageObjectStepDelayer(clock, this).waitFor(duration);
        }

        public List<WebElement> thenReturnElementList(
        readonly By byListCriteria
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
        readonly string textValue
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
        readonly
        string.
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
 * [Obsolete] use element(webElement).containsText(textValue)
 */
        [Obsolete]
        public bool containsTextInElement(
        readonly WebElement webElement, 
        readonly string textValue
        )
        {
            return element(webElement).containsText(textValue);
        }

/*
 * Check that the element contains a given text.
 * [Obsolete] use element(webElement).shouldContainText(textValue)
 */
        [Obsolete]
        public void shouldContainTextInElement(
        readonly WebElement webElement, 
        readonly string textValue
        )
        {
            element(webElement).shouldContainText(textValue);
        }

/*
 * Check that the element does not contain a given text.
 * [Obsolete] use element(webElement).shouldNotContainText(textValue)
 */
        [Obsolete]
        public void shouldNotContainTextInElement(
        readonly WebElement webElement, 
        readonly string textValue
        )
        {
            element(webElement).shouldNotContainText(textValue);
        }

/**
 * Clear a field and enter a value into it.
 */
        public void typeInto(
        readonly WebElement field, 
        readonly string value
        )
        {
            element(field).type(value);
        }

/**
 * Clear a field and enter a value into it.
 * This is a more fluent alternative to using the typeInto method.
 */
        public FieldEntry enter(
        readonly string value
        )
        {
            return new FieldEntry(value);
        }

        public void selectFromDropdown(
        readonly WebElement dropdown, 
        readonly string visibleLabel
        )
        {

            Dropdown.forWebElement(dropdown).select(visibleLabel);
            notifyScreenChange();
        }

        public void selectMultipleItemsFromDropdown(
        readonly WebElement dropdown, 
        readonly
        string.
        ..
        selectedLabels
        )
        {
            Dropdown.forWebElement(dropdown).selectMultipleItems(selectedLabels);
            notifyScreenChange();
        }


        public Set<string> getSelectedOptionLabelsFrom(
        readonly WebElement dropdown
        )
        {
            return Dropdown.forWebElement(dropdown).getSelectedOptionLabels();
        }

        public Set<string> getSelectedOptionValuesFrom(
        readonly WebElement dropdown
        )
        {
            return Dropdown.forWebElement(dropdown).getSelectedOptionValues();
        }

        public string getSelectedValueFrom(
        readonly WebElement dropdown
        )
        {
            return Dropdown.forWebElement(dropdown).getSelectedValue();
        }

        public string getSelectedLabelFrom(
        readonly WebElement dropdown
        )
        {
            return Dropdown.forWebElement(dropdown).getSelectedLabel();
        }

        public void setCheckbox(
        readonly WebElement field, 
        readonly bool value
        )
        {
            Checkbox checkbox = new Checkbox(field);
            checkbox.setChecked(value);
            notifyScreenChange();
        }

        public bool containsText(
        readonly string textValue
        )
        {
            return getRenderedView().containsText(textValue);
        }

/**
 * Check that the specified text appears somewhere in the page.
 */
        public bool containsAllText(
        readonly
        string.
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
        readonly WebElement field
        )
        {
            element(field).shouldBeVisible();
        }

        public void shouldBeVisible(
        readonly By byCriteria
        )
        {
            waitOnPage().until(ExpectedConditions.visibilityOfElementLocated(byCriteria));
        }

        public void shouldNotBeVisible(
        readonly WebElement field
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
        readonly By byCriteria
        )
        {
            List<WebElement> matchingElements = getDriver().findElements(byCriteria);
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
        readonly string startingUrl
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
        readonly string starting, 
        readonly string
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
        readonly string protocol, 
        readonly string host, 
        readonly int port
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
            return hostComponent.tostring();
        }

/**
 * Open the IWebDriver browser using a paramaterized URL. Parameters are
 * represented in the URL using {0}, {1}, etc.
 */
        public readonly void open(
        readonly string[] parameterValues
        )
        {
            open(OpenMode.CHECK_URL_PATTERNS, parameterValues);
        }

/**
 * Opens page without checking URL patterns. Same as open(string...)) otherwise.
 */
        public readonly void openUnchecked(
        readonly
        string.
        ..
        parameterValues
        )
        {
            open(OpenMode.IGNORE_URL_PATTERNS, parameterValues);
        }

        private void open(
        readonly OpenMode openMode, 
        readonly
        string.
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

        public readonly void open(
        readonly string urlTemplateName, 
        readonly string[] parameterValues
        )
        {
            open(OpenMode.CHECK_URL_PATTERNS, urlTemplateName, parameterValues);
        }

/**
 * Opens page without checking URL patterns. Same as {@link #open(string, string[])} otherwise.
 */
        public readonly void openUnchecked(
        readonly string urlTemplateName, 
        readonly string[] parameterValues
        )
        {
            open(OpenMode.IGNORE_URL_PATTERNS, urlTemplateName, parameterValues);
        }

        private void open(
        readonly OpenMode openMode, 
        readonly string urlTemplateName, 
        readonly string[] parameterValues
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

        readonly public void open()
        {
            open(OpenMode.CHECK_URL_PATTERNS);
        }

/**
 * Opens page without checking URL patterns. Same as {@link #open()} otherwise.
 */

        readonly public void openUnchecked()
        {
            open(OpenMode.IGNORE_URL_PATTERNS);
        }

        private void open(
        readonly OpenMode openMode
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
        readonly OpenMode openMode
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

        readonly public void openAt(string startingUrl)
        {
            openPageAtUrl(updateUrlWithBaseUrlIfDefined(startingUrl));
            callWhenPageOpensMethods();
        }

/**
 * Override this method
 */

        public void callWhenPageOpensMethods()
        {
            for (Method annotatedMethod :
            methodsAnnotatedWithWhenPageOpens())
            {
                try
                {
                    annotatedMethod.setAccessible(true);
                    annotatedMethod.invoke(this);
                }
                catch (Throwable e)
                {
                    LOGGER.error("Could not execute @WhenPageOpens annotated method: " + e.getMessage());
                    if (e
                    instanceof InvocationTargetException)
                    {
                        e = ((InvocationTargetException) e).getTargetException();
                    }
                    if (AssertionError.class.
                    isAssignableFrom(e.getClass()))
                    {
                        throw (AssertionError) e;
                    }
                    else
                    {
                        throw new UnableToInvokeWhenPageOpensMethods(
                            "Could not execute @WhenPageOpens annotated method: "
                            + e.getMessage(), e);
                    }
                }
            }
        }

        private List<Method> methodsAnnotatedWithWhenPageOpens()
        {
            List<Method> methods = MethodFinder.inClass(this.getClass()).getAllMethods();
            List<Method> annotatedMethods = new ArrayList<>();
            for (Method method :
            methods)
            {
                if (method.getAnnotation(WhenPageOpens.class) !=
                null)
                {
                    if (method.getParameterTypes().length == 0)
                    {
                        annotatedMethods.add(method);
                    }
                    else
                    {
                        throw new UnableToInvokeWhenPageOpensMethods(
                            "Could not execute @WhenPageOpens annotated method: WhenPageOpens method cannot have parameters: " +
                            method);
                    }
                }
            }
            return annotatedMethods;
        }

        public static string[] withParameters(
        readonly
        string.
        ..
        parameterValues
        )
        {
            return parameterValues;
        }

        private void openPageAtUrl(
        readonly string startingUrl
        )
        {
            getDriver().get(startingUrl);
            if (javascriptIsSupportedIn(getDriver()))
            {
                addJQuerySupport();
            }
        }

        public void clickOn(
        readonly WebElement webElement
        )
        {
            element(webElement).click();
        }

/**
 * Returns true if at least one matching element is found on the page and is visible.
 */
        public bool isElementVisible(
        readonly By byCriteria
        )
        {
            return getRenderedView().elementIsDisplayed(byCriteria);
        }

        public void setDefaultBaseUrl(
        readonly string defaultBaseUrl
        )
        {
            pageUrls.overrideDefaultBaseUrl(defaultBaseUrl);
        }

/**
 * Returns true if the specified element has the focus.
 *
 * [Obsolete] Use element(webElement).hasFocus() instead
 */
        public bool hasFocus(
        readonly WebElement webElement
        )
        {
            return element(webElement).hasFocus();
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
        public <
        T extends
        net.serenitybdd.core.pages.WebElementFacade
        >

        T element(WebElement webElement)
        {
            return net.serenitybdd.core.pages.WebElementFacadeImpl.wrapWebElement(driver, webElement,
                getImplicitWaitTimeout().in(MILLISECONDS),
            getWaitForTimeout().in
            (MILLISECONDS),
            nameOf(webElement))
            ;
        }

        private string nameOf(WebElement webElement)
        {
            try
            {
                return webElement.tostring();
            }
            catch (Exception e)
            {
                return "Unknown web element";
            }
        }


        public <
        T extends
        net.serenitybdd.core.pages.WebElementFacade
        >
        T
        $

        (WebElement webElement)
        {
            return element(webElement);
        }

        public <
        T extends
        net.serenitybdd.core.pages.WebElementFacade
        >
        T
        $

        (string xpathOrCssSelector)
        {
            return element(xpathOrCssSelector);
        }

/**
 * Provides a fluent API for querying web elements.
 */
        public <
        T extends
        net.serenitybdd.core.pages.WebElementFacade
        >

        T element(By bySelector)
        {
            return net.serenitybdd.core.pages.WebElementFacadeImpl.wrapWebElement(driver,
                bySelector,
                getImplicitWaitTimeout().in(MILLISECONDS),
            getWaitForTimeout().in
            (MILLISECONDS),
            bySelector.tostring())
            ;
        }

        public <
        T extends
        net.serenitybdd.core.pages.WebElementFacade
        >

        T find(List<By> selectors)
        {
            T element = null;
            for (By selector :
            selectors)
            {
                if (element == null)
                {
                    element = element(selector);
                }
                else
                {
                    element = element.find(selector);
                }
            }
            return element;
        }

        public <
        T extends
        net.serenitybdd.core.pages.WebElementFacade
        >

        T find(By. ..selectors)
        {
            return find(Lists.newArrayList(selectors));
        }

        public List<net.serenitybdd.core.pages.WebElementFacade> findAll(By bySelector)
        {
            List<WebElement> matchingWebElements = driver.findElements(bySelector);
            return convert(matchingWebElements, toWebElementFacades());
        }

        private Converter<WebElement, net.serenitybdd.core.pages.WebElementFacade> toWebElementFacades()
        {
            return new Converter<WebElement, net.serenitybdd.core.pages.WebElementFacade>()
            {
                public net.serenitybdd.core.pages.WebElementFacade convert(WebElement from)
                {
                return element(from);
            }
            }
            ;
        }

/**
 * Provides a fluent API for querying web elements.
 */
        public <
        T extends
        net.serenitybdd.core.pages.WebElementFacade
        >

        T element(string xpathOrCssSelector)
        {
            return element(xpathOrCssSelector(xpathOrCssSelector));
        }

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


        public Object evaluateJavascript(
        readonly string script
        )
        {
            addJQuerySupport();
            JavascriptExecutorFacade js = new JavascriptExecutorFacade(driver);
            return js.executeScript(script);
        }

        public Object evaluateJavascript(
        readonly string script, 
        readonly
        Object.
        .. params)
        {
            addJQuerySupport();
            JavascriptExecutorFacade js = new JavascriptExecutorFacade(driver);
            return js.executeScript(script,  params)
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
                    return SupportedWebDriver.forClass(((WebDriverFacade) getDriver()).getDriverClass())
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
                return ((WebDriverFacade) getDriver()).isEnabled() && ((WebDriverFacade) getDriver()).isInstantiated();
            }
            return true;
        }

        public ThucydidesFluentWait<IWebDriver> waitForWithRefresh()
        {
            return new FluentWaitWithRefresh<>(driver, webdriverClock, sleeper)
                .withTimeout(getWaitForTimeout().in(TimeUnit.MILLISECONDS),
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
                .withTimeout(getWaitForTimeout().in(TimeUnit.MILLISECONDS),
            TimeUnit.MILLISECONDS)
                .
            pollingEvery(WAIT_FOR_ELEMENT_PAUSE_LENGTH, TimeUnit.MILLISECONDS)
                .ignoring(NoSuchElementException.class,
            NoSuchFrameException.class)
            ;
        }

        public WebElementFacade waitFor(WebElement webElement)
        {
            return waitFor($(webElement))
            ;
        }

        public WebElementFacade waitFor(WebElementFacade webElement)
        {
            return getRenderedView().waitFor(webElement);
        }


        public Alert getAlert()
        {
            return driver.switchTo().alert();
        }

        public Actions withAction()
        {
            IWebDriver proxiedDriver = ((WebDriverFacade) getDriver()).getProxiedDriver();
            return new Actions(proxiedDriver);
        }

        public class FieldEntry
        {

            private readonly string value;

            public FieldEntry(
            readonly string value
            )
            {
                this.value = value;
            }

            public void into(
            readonly WebElement field
            )
            {
                element(field).type(value);
            }

            public void into(
            readonly net.serenitybdd.core.pages.WebElementFacade field
            )
            {
                field.type(value);
            }

            public void intoField(
            readonly By bySelector
            )
            {
                WebElement field = getDriver().findElement(bySelector);
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

        public <
        T extends
        WebElementFacade
        >

        T moveTo(By locator)
        {
            if (!driverIsDisabled())
            {
                withAction().moveToElement(find(locator)).perform();
            }
            return find(locator);
        }

        public void waitForAngularRequestsToFinish()
        {
            if ((bool) getJavascriptExecutorFacade().executeScript(
                "return (typeof angular !== 'undefined')? true : false;"))
            {
                getJavascriptExecutorFacade().executeAsyncScript(
                    "var callback = arguments[arguments.length - 1];"
                    +
                    "angular.element(document.body).injector().get('$browser').notifyWhenNoOutstandingRequests(callback);");
            }
        }

        Inflector inflection = Inflector.getInstance();


        public override string tostring()
        {
            return inflection.of(getClass().getSimpleName())
                .inHumanReadableForm().tostring();
        }
        */
    }

    internal class ConfigurableTimeouts
    {
        public void setImplicitTimeout(Duration implicitTimeout)
        {
            throw new NotImplementedException();
        }

        public Duration resetTimeouts()
        {
            throw new NotImplementedException();
        }
    }

    internal class JavascriptExecutorFacade
    {
    }

    public static class ClassExtensions
    {
        public static bool instanceof(this object src, Type tgt)
        {
            return tgt.IsAssignableFrom(src);
        }
    }

    public class PageUrls
{
}

public class RenderedPageObjectView
{
}

}

namespace SerenityBDD.Core.time
{
    public enum TimeUnit{
        MILLISECONDS;
    }

    class Clock : SystemClock
    {
    }

    internal class Sleeper
    {
    }

    internal class Duration
    {
        private int duration;
        private TimeUnit unit;

        public static implicit operator TimeSpan(Duration src)
        {
            return src.TimeSpan;
        }

        public static implicit operator Duration(TimeSpan src)
        {
            return new Duration(src);
        }
        public Duration(TimeSpan timespan)
        {
            this.TimeSpan = timespan;
        }

        public Duration(int duration, TimeUnit unit)
        {
            
            this.duration = duration;
            this.unit = unit;
        }

        public TimeSpan TimeSpan { get; set; }
        
    }

    internal class SystemClock
    {
    }

    public class PropertyBase<T>:PropertyBase
    {
        public T Value { get; set; }

        public PropertyBase(string propertyName) : base(propertyName)
        {
        }

        public PropertyBase(string propertyName, T defaultValue):base(propertyName)
        {
            this.Value = defaultValue;
        }
        public static implicit operator T(PropertyBase<T> src)
        {
            return (T) src.Value;
        }

    }
    public class PropertyBase
    {

        private string propertyName;
        public static readonly int DEFAULT_HEIGHT = 700;
        public static readonly int DEFAULT_WIDTH = 960;

        public static readonly string DEFAULT_HISTORY_DIRECTORY = "history";


    private ILog logger = LogManager.GetLogger(typeof(PropertyBase));

    public PropertyBase(string propertyName)
        {
            this.propertyName = propertyName.Replace("_", ".").ToLowerInvariant();
        }

        public static PropertyBase<T> create<T>(string propertyName)
        {
            return new PropertyBase<T>(propertyName);
        }
        public static PropertyBase<T> withDefault<T>(string propertyName, T defaultValue)
        {
            return new PropertyBase<T>(propertyName, defaultValue);
        }
    
       
        public string getPropertyName()
        {
            return propertyName;
        }
        
    public override string ToString()
        {
            return propertyName;
        }

        public string From(EnvironmentVariables environmentVariables)
        {
            return From(environmentVariables, null);
        }

        private Optional<string> legacyPropertyValueIfPresentIn(EnvironmentVariables environmentVariables)
        {
            string legacyValue = environmentVariables.getProperty(withLegacyPrefix(getPropertyName()));
            if (StringUtils.isNotEmpty(legacyValue))
            {
                logger.Warn("Legacy property format detected for {}, please use the serenity.* format instead.", getPropertyName());
            }
            return Optional.fromNullable(legacyValue);
        }

        private string withLegacyPrefix(string propertyName)
        {
            return propertyName.Replace("serenity.", "thucydides.");
        }

        private string withSerenityPrefix(string propertyName)
        {
            return propertyName.Replace("thucydides.", "serenity.");
        }

        public string preferredName()
        {
            return withSerenityPrefix(getPropertyName());
        }

        public List<string> legacyNames()
        {
            List<string> names = new[] {withLegacyPrefix(getPropertyName())}.ToList();

            return names;
        }

        public string From(EnvironmentVariables environmentVariables, string defaultValue)
        {
            Optional<string> newPropertyValue
                    = Optional.fromNullable(environmentVariables.getProperty(withSerenityPrefix(getPropertyName())));

            if (isDefined(newPropertyValue))
            {
                return newPropertyValue.get();
            }
            else
            {
                Optional<string> legacyValue = legacyPropertyValueIfPresentIn(environmentVariables);
                return (isDefined(legacyValue)) ? legacyValue.get() : defaultValue;
            }
        }

        private bool isDefined(Optional<string> newPropertyValue)
        {
            return newPropertyValue.isPresent() && StringUtils.isNotEmpty(newPropertyValue.get());
        }

        public int integerFrom(EnvironmentVariables environmentVariables)
        {
            return integerFrom(environmentVariables, 0);
        }

        public int integerFrom(EnvironmentVariables environmentVariables, int defaultValue)
        {
            Optional<string> newPropertyValue
                    = Optional.fromNullable(environmentVariables.getProperty(withSerenityPrefix(getPropertyName())));

            if (isDefined(newPropertyValue))
            {
                return int.Parse(newPropertyValue.get());
            }
            else
            {
                Optional<string> legacyValue = legacyPropertyValueIfPresentIn(environmentVariables);
                return (isDefined(legacyValue)) ? int.Parse(legacyValue.get()) : defaultValue;
            }
        }

        public bool booleanFrom(EnvironmentVariables environmentVariables)
        {
            return booleanFrom(environmentVariables, false);
        }

        public bool booleanFrom(EnvironmentVariables environmentVariables, bool defaultValue)
        {
            if (environmentVariables == null) { return defaultValue; }

            Optional<string> newPropertyValue
                    = Optional.fromNullable(environmentVariables.getProperty(withSerenityPrefix(getPropertyName())));

            if (isDefined(newPropertyValue))
            {
                return bool.Parse(newPropertyValue.get());
            }
            else
            {
                Optional<string> legacyValue = legacyPropertyValueIfPresentIn(environmentVariables);
                return (isDefined(legacyValue)) ? bool.Parse(legacyValue.get()) : defaultValue;
            }
        }

        public bool isDefinedIn(EnvironmentVariables environmentVariables)
        {
            return StringUtils.isNotEmpty(From(environmentVariables));
        }

    }

    public static class StringUtils
    {
        public static bool isNotEmpty(string src)
        {
            return !string.IsNullOrEmpty(src);
        }
    }

    /// <summary>
    /// this should be some container configuration like autofac or similar
    /// </summary>
    public interface Injector {
        T getInstance<T>();
    }

    public class Injectors
    {

        private static Injector injector;

        public static Injector getInjector()
        {
            //if (injector == null)
            //{
            //    injector = Guice.createInjector(new ThucydidesModule());
            //}
            //return injector;
            throw new NotImplementedException("Some injection container should be here!");
        }
        
    }
}


