using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Text;
using log4net;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SerenityBDD.Core.steps.SerenityBDD.Core.time;
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
/*
        protected PageObject()
        {
            this.webdriverClock = new SystemClock();
            this.clock = Injectors.getInjector().getInstance(net.serenitybdd.core.time.SystemClock.class);
            this.environmentVariables = Injectors.getInjector().getProvider(EnvironmentVariables.class).get();
            this.sleeper = Sleeper.SYSTEM_SLEEPER;
            setupPageUrls();
        }

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

        public FileToUpload upload(readonly String filename)
        {
            return new FileToUpload(driver, filename).useRemoteDriver(isDefinedRemoteUrl());
        }

        public FileToUpload uploadData(String data) throws IOException
        {
            Path datafile = Files.createTempFile("upload", "data");
        Files.write(datafile, data.getBytes(StandardCharsets.UTF_8));
        return new FileToUpload(driver, datafile.toAbsolutePath().toString()).useRemoteDriver(isDefinedRemoteUrl());
    }

    

    internal class JavascriptExecutorFacade
    {
    }



    public FileToUpload uploadData(byte[] data) throws IOException
{
    Path datafile = Files.createTempFile("upload", "data");
    Files.write(datafile, data);
        return new FileToUpload(driver, datafile.toAbsolutePath().toString()).useRemoteDriver(isDefinedRemoteUrl());
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

        public String getTitle()
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
        readonly String currentUrl
        )
        {
            return thereAreNoPatternsDefined() || matchUrlAgainstEachPattern(currentUrl);
        }

        private bool matchUrlAgainstEachPattern(
        readonly String currentUrl
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

        public PageObject waitFor(String xpathOrCssSelector)
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

        public PageObject waitForPresenceOf(String xpathOrCssSelector)
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

        public PageObject waitForAbsenceOf(String xpathOrCssSelector)
        {
            return waitForRenderedElementsToDisappear(xpathOrCssSelector(xpathOrCssSelector));
        }

/**
 * Waits for a given text to appear anywhere on the page.
 */
        public PageObject waitForTextToAppear(
        readonly String expectedText
        )
        {
            getRenderedView().waitForText(expectedText);
            return this;
        }

        public PageObject waitForTitleToAppear(
        readonly String expectedTitle
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
        readonly String expectedTitle
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
        readonly String expectedText
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
        readonly String expectedText
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
        readonly String expectedText
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
        readonly String expectedText
        )
        {
            return waitForTextToDisappear(expectedText, getWaitForTimeout().in(MILLISECONDS))
            ;
        }

/**
 * Waits for a given text to not be anywhere on the page.
 */
        public PageObject waitForTextToDisappear(
        readonly String expectedText, 
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
        readonly String expectedText, 
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
        String.
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
        String.
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
        String.
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
        readonly String textValue
        )
        {
            if (!containsText(textValue))
            {
                String errorMessage = String.format(
                    "The text '%s' was not found in the page", textValue);
                throw new NoSuchElementException(errorMessage);
            }
        }

/**
 * Check that all of the specified texts appears somewhere in the page.
 */
        public void shouldContainAllText(
        readonly
        String.
        ..
        textValues
        )
        {
            if (!containsAllText(textValues))
            {
                String errorMessage = String.format(
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
        readonly String textValue
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
        readonly String textValue
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
        readonly String textValue
        )
        {
            element(webElement).shouldNotContainText(textValue);
        }

/**
 * Clear a field and enter a value into it.
 */
        public void typeInto(
        readonly WebElement field, 
        readonly String value
        )
        {
            element(field).type(value);
        }

/**
 * Clear a field and enter a value into it.
 * This is a more fluent alternative to using the typeInto method.
 */
        public FieldEntry enter(
        readonly String value
        )
        {
            return new FieldEntry(value);
        }

        public void selectFromDropdown(
        readonly WebElement dropdown, 
        readonly String visibleLabel
        )
        {

            Dropdown.forWebElement(dropdown).select(visibleLabel);
            notifyScreenChange();
        }

        public void selectMultipleItemsFromDropdown(
        readonly WebElement dropdown, 
        readonly
        String.
        ..
        selectedLabels
        )
        {
            Dropdown.forWebElement(dropdown).selectMultipleItems(selectedLabels);
            notifyScreenChange();
        }


        public Set<String> getSelectedOptionLabelsFrom(
        readonly WebElement dropdown
        )
        {
            return Dropdown.forWebElement(dropdown).getSelectedOptionLabels();
        }

        public Set<String> getSelectedOptionValuesFrom(
        readonly WebElement dropdown
        )
        {
            return Dropdown.forWebElement(dropdown).getSelectedOptionValues();
        }

        public String getSelectedValueFrom(
        readonly WebElement dropdown
        )
        {
            return Dropdown.forWebElement(dropdown).getSelectedValue();
        }

        public String getSelectedLabelFrom(
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
        readonly String textValue
        )
        {
            return getRenderedView().containsText(textValue);
        }

/**
 * Check that the specified text appears somewhere in the page.
 */
        public bool containsAllText(
        readonly
        String.
        ..
        textValues
        )
        {
            for (String textValue :
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

        public String updateUrlWithBaseUrlIfDefined(
        readonly String startingUrl
        )
        {

            String baseUrl = pageUrls.getSystemBaseUrl();
            if ((baseUrl != null) && (!StringUtils.isEmpty(baseUrl)))
            {
                return replaceHost(startingUrl, baseUrl);
            }
            else
            {
                return startingUrl;
            }
        }

        private String replaceHost(
        readonly String starting, 
        readonly String
        base)
        {

            String updatedUrl = starting;
            try
            {
                URL startingUrl = new URL(starting);
                URL baseUrl = new URL(base);

                String startingHostComponent = hostComponentFrom(startingUrl.getProtocol(),
                    startingUrl.getHost(),
                    startingUrl.getPort());
                String baseHostComponent = hostComponentFrom(baseUrl.getProtocol(),
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

        private String hostComponentFrom(
        readonly String protocol, 
        readonly String host, 
        readonly int port
        )
        {
            StringBuilder hostComponent = new StringBuilder(protocol);
            hostComponent.append("://");
            hostComponent.append(host);
            if (port > 0)
            {
                hostComponent.append(":");
                hostComponent.append(port);
            }
            return hostComponent.toString();
        }

/**
 * Open the IWebDriver browser using a paramaterized URL. Parameters are
 * represented in the URL using {0}, {1}, etc.
 */
        public readonly void open(
        readonly String[] parameterValues
        )
        {
            open(OpenMode.CHECK_URL_PATTERNS, parameterValues);
        }

/**
 * Opens page without checking URL patterns. Same as open(String...)) otherwise.
 */
        public readonly void openUnchecked(
        readonly
        String.
        ..
        parameterValues
        )
        {
            open(OpenMode.IGNORE_URL_PATTERNS, parameterValues);
        }

        private void open(
        readonly OpenMode openMode, 
        readonly
        String.
        ..
        parameterValues
        )
        {
            String startingUrl = pageUrls.getStartingUrl(parameterValues);
            LOGGER.debug("Opening page at url {}", startingUrl);
            openPageAtUrl(startingUrl);
            checkUrlPatterns(openMode);
            initializePage();
            LOGGER.debug("Page opened");
        }

        public readonly void open(
        readonly String urlTemplateName, 
        readonly String[] parameterValues
        )
        {
            open(OpenMode.CHECK_URL_PATTERNS, urlTemplateName, parameterValues);
        }

/**
 * Opens page without checking URL patterns. Same as {@link #open(String, String[])} otherwise.
 */
        public readonly void openUnchecked(
        readonly String urlTemplateName, 
        readonly String[] parameterValues
        )
        {
            open(OpenMode.IGNORE_URL_PATTERNS, urlTemplateName, parameterValues);
        }

        private void open(
        readonly OpenMode openMode, 
        readonly String urlTemplateName, 
        readonly String[] parameterValues
        )
        {
            String startingUrl = pageUrls.getNamedUrl(urlTemplateName,
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
            String startingUrl = updateUrlWithBaseUrlIfDefined(pageUrls.getStartingUrl());
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
                String currentUrl = getDriver().getCurrentUrl();
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

            String errorDetails = "This is not the page you're looking for: "
                                  + "I was looking for a page compatible with " + this.getClass() + " but "
                                  + "I was at the URL " + getDriver().getCurrentUrl();

            throw new WrongPageError(errorDetails);
        }

        readonly public void openAt(String startingUrl)
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

        public static String[] withParameters(
        readonly
        String.
        ..
        parameterValues
        )
        {
            return parameterValues;
        }

        private void openPageAtUrl(
        readonly String startingUrl
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
        readonly String defaultBaseUrl
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

        private String nameOf(WebElement webElement)
        {
            try
            {
                return webElement.toString();
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

        (String xpathOrCssSelector)
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
            bySelector.toString())
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

        T element(String xpathOrCssSelector)
        {
            return element(xpathOrCssSelector(xpathOrCssSelector));
        }

        public <
        T extends
        net.serenitybdd.core.pages.WebElementFacade
        >

        T findBy(String xpathOrCssSelector)
        {
            return element(xpathOrCssSelector);
        }

        public List<net.serenitybdd.core.pages.WebElementFacade> findAll(String xpathOrCssSelector)
        {
            return findAll(xpathOrCssSelector(xpathOrCssSelector));
        }

        public bool containsElements(By bySelector)
        {
            return !findAll(bySelector).isEmpty();
        }

        public bool containsElements(String xpathOrCssSelector)
        {
            return !findAll(xpathOrCssSelector).isEmpty();
        }


        public Object evaluateJavascript(
        readonly String script
        )
        {
            addJQuerySupport();
            JavascriptExecutorFacade js = new JavascriptExecutorFacade(driver);
            return js.executeScript(script);
        }

        public Object evaluateJavascript(
        readonly String script, 
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

        public RadioButtonGroup inRadioButtonGroup(String name)
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

            private readonly String value;

            public FieldEntry(
            readonly String value
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

        public T moveTo<T>(String xpathOrCssSelector)
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


        public override String toString()
        {
            return inflection.of(getClass().getSimpleName())
                .inHumanReadableForm().toString();
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
    public class ThucydidesSystemProperty
    {

        /**
         * The WebDriver driver - firefox, chrome, iexplorer, htmlunit, safari.
         */
        public static string WEBDRIVER_DRIVER = "WebDriver_Driver";


        /** A shortcut for 'webdriver.driver'. */
        public static string DRIVER = "webdriver.driver";

        /**
         * If using a provided driver, what type is it.
         * The implementation class needs to be defined in the webdriver.provided.{type} system property.
        */
        public static string WEBDRIVER_PROVIDED_TYPE = "webdriver.provided.type";

        /**
         * The default starting URL for the application, and base URL for relative paths.
         */
        public static string WEBDRIVER_BASE_URL = "webdriver.base.url";

        /**
         * The URL to be used for remote drivers (including a selenium grid hub)
         */
        public static string WEBDRIVER_REMOTE_URL = "webdriver.remote.url";

        /**
         * What port to run PhantomJS on (used in conjunction with webdriver.remote.url to
         * register with a Selenium hub, e.g. -Dphantomjs.webdriver=5555 -Dwebdriver.remote.url=http://localhost:4444
         */
        public static string PHANTOMJS_WEBDRIVER_PORT = "phantomjs.webdriver.port";

        /**
         * The driver to be used for remote drivers
         */
        public static string WEBDRIVER_REMOTE_DRIVER = "webdriver.remote.driver";

        public static string WEBDRIVER_REMOTE_BROWSER_VERSION = "webdriver.remote.browser.version";

        public static string WEBDRIVER_REMOTE_OS = "webdriver.remote.os";

        /**
         * Path to the Internet Explorer driver, if it is not on the system path.
         */
        public static string WEBDRIVER_IE_DRIVER = "webdriver.ie.driver";

        /**
         * Path to the Edge driver, if it is not on the system path.
         */
        public static string WEBDRIVER_EDGE_DRIVER = "webdriver.edge.driver";

        /**
         * Path to the Chrome driver, if it is not on the system path.
         */
        public static string WEBDRIVER_CHROME_DRIVER = "webdriver.chrome.driver";

        /**
         * Path to the Chrome binary, if it is not on the system path.
         */
        public static string WEBDRIVER_CHROME_BINARY = "webdriver.chrome.binary";

        [Obsolete] public static string THUCYDIDES_PROJECT_KEY = "thucydides.project.key";

        /**
         * A unique identifier for the project under test, used to record test statistics.
         */
        public static string SERENITY_PROJECT_KEY = "serenity.project.key";

        [Obsolete]
        public static string THUCYDIDES_PROJECT_NAME = "thucydides.project.name";

        /**
         * What name should appear on the reports
         */
        public static string SERENITY_PROJECT_NAME = "serenity.project.name";


        [Obsolete] public static string THUCYDIDES_HOME = "thucydides.project.home";

        /**
         * The home directory for Thucydides output and data files - by default, $USER_HOME/.thucydides
         */
        public static string SERENITY_HOME = "serenity.home";

        [Obsolete] public static string THUCYDIDES_REPORT_RESOURCES = "thucydides.report.resources";

        /**
         * Indicates a directory from which the resources for the HTML reports should be copied.
         * This directory currently needs to be provided in a JAR file.
         */
        public static string SERENITY_REPORT_RESOURCES = "serenity.report.resources";

        /**
         * Encoding for reports output
         */
        [Obsolete] public static string THUCYDIDES_REPORT_ENCODING = "thucydides.report.encoding";

        /**
         * Encoding for reports output
         */
        public static string SERENITY_REPORT_ENCODING = "serenity.report.encoding";

        [Obsolete] public static string THUCYDIDES_OUTPUT_DIRECTORY = "thucydides.outputDirectory";

        /**
         * Where should reports be generated (use the system property 'serenity.outputDirectory').
         */
        public static string SERENITY_OUTPUT_DIRECTORY = "serenity.outputDirectory";

        /**
         * Default name of report with configurations. It will contains some values that was used during generation of reports
         */
        [Obsolete] public static string THUCYDIDES_CONFIGURATION_REPORT = "thucydides.configuration.json";

        /**
         * Default name of report with configurations. It will contains some values that was used during generation of reports
         */
        public static string SERENITY_CONFIGURATION_REPORT = "serenity.configuration.json";

        [Obsolete] public static string THUCYDIDES_FLOW_REPORTS_DIR = "flow";

        /**
         * Default name of folder, with reports about test flow and aggregation report generation
         */
        public static string SERENITY_FLOW_REPORTS_DIR = "flow";

        /**
         * Should Thucydides only store screenshots for failing steps?
         * This can save disk space and speed up the tests somewhat. Useful for data-driven testing.
         * [Obsolete] This property is still supported, but thucydides.take.screenshots provides more fine-grained control.
         */

        [Obsolete] public static string THUCYDIDES_ONLY_SAVE_FAILING_SCREENSHOTS =
            "thucydies.only.save.failing.screenshots";

        [Obsolete] public static string THUCYDIDES_DRIVER_CAPABILITIES = "thucydides.driver.capabilities";

        /**
         * A set of user-defined capabilities to be used to configure the WebDriver driver.
         * Capabilities should be passed in as a space or semi-colon-separated list of key:value pairs, e.g.
         * "build:build-1234; max-duration:300; single-window:true; tags:[tag1,tag2,tag3]"
         */
        public static string SERENITY_DRIVER_CAPABILITIES = "serenity.driver.capabilities";

        /**
         * Should Thucydides take screenshots for every clicked button and every selected link?
         * By default, a screenshot will be stored at the start and end of each step.
         * If this option is set to true, Thucydides will record screenshots for any action performed
         * on a WebElementFacade, i.e. any time you use an expression like element(...).click(),
         * findBy(...).click() and so on.
         * This will be overridden if the THUCYDIDES_ONLY_SAVE_FAILING_SCREENSHOTS option is set to true.
         * [Obsolete] This property is still supported, but thucydides.take.screenshots provides more fine-grained control.
         */
        [Obsolete] public static string THUCYDIDES_VERBOSE_SCREENSHOTS = "thucydides.verbose.screenshots";

        [Obsolete] public static string THUCYDIDES_VERBOSE_STEPS = "thucydides.verbose.steps";

        /**
         * If set to true, WebElementFacade events and other step actions will be logged to the console.
         */
        public static string SERENITY_VERBOSE_STEPS = "serenity.verbose.steps";


        [Obsolete] public static string THUCYDIDES_TAKE_SCREENSHOTS = "thucydides.take.screenshots";

        /**
         *  Fine-grained control over when screenshots are to be taken.
         *  This property accepts the following values:
         *  <ul>
         *      <li>FOR_EACH_ACTION</li>
         *      <li>BEFORE_AND_AFTER_EACH_STEP</li>
         *      <li>AFTER_EACH_STEP</li>
         *      <li>FOR_FAILURES</li>
         *  </ul>
         */

        public enum TakeScreenshotsWhenEnum
        {
            ForEachAction,
            BeforeAndAfterEachStep,
            AfterEachStep,
            ForFailures
        }
        public static TakeScreenshotsWhenEnum SERENITY_TAKE_SCREENSHOTS;

        [Obsolete]
        public static bool THUCYDIDES_REPORTS_SHOW_STEP_DETAILS = false;

        /**
         * Should Thucydides display detailed information in the test result tables.
         * If this is set to true, test result tables will display a breakdown of the steps by result.
         * This is false by default.
         */
        public static bool SERENITY_REPORTS_SHOW_STEP_DETAILS = false;

        [Obsolete] public static bool THUCYDIDES_REPORT_SHOW_MANUAL_TESTS = false;

        /**
         * Show statistics for manual tests in the test reports.
         */
        public static bool SERENITY_REPORT_SHOW_MANUAL_TESTS = false;

        [Obsolete]
        public static bool THUCYDIDES_REPORT_SHOW_RELEASES = false;

        /**
         * Report on releases
         */
        public static bool SERENITY_REPORT_SHOW_RELEASES = false;

        [Obsolete]
        public static bool THUCYDIDES_REPORT_SHOW_PROGRESS = false;

        public static bool SERENITY_REPORT_SHOW_PROGRESS = false;

        [Obsolete] public static bool THUCYDIDES_REPORT_SHOW_HISTORY = false;

        public static bool SERENITY_REPORT_SHOW_HISTORY = false;

        [Obsolete] public static bool THUCYDIDES_REPORT_SHOW_TAG_MENUS = false;

        public static bool SERENITY_REPORT_SHOW_TAG_MENUS = false;

        [Obsolete] public static bool THUCYDIDES_REPORT_TAG_MENUS = false;

        public static bool SERENITY_REPORT_TAG_MENUS = false;

        [Obsolete] public static bool THUCYDIDES_EXCLUDE_UNRELATED_REQUIREMENTS_OF_TYPE = false;

        public static bool SERENITY_EXCLUDE_UNRELATED_REQUIREMENTS_OF_TYPE = false;

        [Obsolete] public static int THUCYDIDES_RESTART_BROWSER_FREQUENCY = 3;

        /**
         * Restart the browser every so often during data-driven tests.
         */
        public static int SERENITY_RESTART_BROWSER_FREQUENCY = 3;

        [Obsolete] public static bool THUCYDIDES_RESTART_BROWSER_FOR_EACH = true ;

        /**
         * Indicate when a browser should be restarted during a test run.
         * Can be one of: example, scenario, story, feature, never
         *
         */

        public enum RestartBrowserWhenEnum
        {
            example,
            scenario,
            story,
            feature,
            never
        }

        public static RestartBrowserWhenEnum SERENITY_RESTART_BROWSER_FOR_EACH;

        [Obsolete]
        public static bool THUCYDIDES_DIFFERENT_BROWSER_FOR_EACH_ACTOR = false;

        /**
         * When multiple actors are used with the Screenplay pattern, a separate browser is configured for each actor.
         * Set this property to false if you want actors use a common browser.
         * This can be useful if actors are used to illustrate the intent of a test, but no tests use more than one actor simultaneously
         */
        public static bool SERENITY_DIFFERENT_BROWSER_FOR_EACH_ACTOR = true;

        [Obsolete] public static double THUCYDIDES_STEP_DELAY;

        /**
         * Pause (in ms) between each test step.
         */
        public static double SERENITY_STEP_DELAY;

        [Obsolete] public static double THUCYDIDES_TIMEOUT;

        /**
         * How long should the driver wait for elements not immediately visible, in seconds.
         */
        public static double SERENITY_TIMEOUT = 10;

        /**
         * Don't accept sites using untrusted certificates.
         * By default, Thucydides accepts untrusted certificates - use this to change this behaviour.
         */
        public static bool REFUSE_UNTRUSTED_CERTIFICATES = false;

        /**
         * Use the same browser for all tests (the "Highlander" rule)
         * [Obsolete]: Use THUCYDIDES_RESTART_BROWSER_FOR_EACH instead.
         */
        [Obsolete]
        public static bool THUCYDIDES_USE_UNIQUE_BROWSER = true;

        [Obsolete] public static int THUCYDIDES_ESTIMATED_AVERAGE_STEP_COUNT;

        /**
         * The estimated number of steps in a pending scenario.
         * This is used for stories where no scenarios have been defined.
         */
        public static int SERENITY_ESTIMATED_AVERAGE_STEP_COUNT;

        [Obsolete] public static int THUCYDIDES_ESTIMATED_TESTS_PER_REQUIREMENT;

        /**
         * The estimated number of tests in a typical story.
         * Used to estimate functional coverage in the requirements reports.
         */
        public static int SERENITY_ESTIMATED_TESTS_PER_REQUIREMENT;

        [Obsolete] public static string THUCYDIDES_ISSUE_TRACKER_URL;

        /**
         *  Base URL for the issue tracking system to be referred to in the reports.
         *  If defined, any issues quoted in the form #1234 will be linked to the relevant
         *  issue in the issue tracking system. Works with JIRA, Trac etc.
         */
        public static string SERENITY_ISSUE_TRACKER_URL;

        [Obsolete] public static bool THUCYDIDES_NATIVE_EVENTS = true;

        /**
         * Activate native events in Firefox.
         * This is true by default, but can cause issues with some versions of linux.
         */
        public static bool SERENITY_NATIVE_EVENTS = true;

        /**
         * If the base JIRA URL is defined, Thucydides will build the issue tracker url using the standard JIRA form.
         */
        public static string JIRA_URL;

        /**
         *  If defined, the JIRA project id will be prepended to issue numbers.
         */
        public static string JIRA_PROJECT;

        /**
         *  If defined, the JIRA username required to connect to JIRA.
         */
        public static string JIRA_USERNAME;

        /**
         *  If defined, the JIRA password required to connect to JIRA.
         */
        public static string JIRA_PASSWORD;

        /**
         *  The JIRA workflow is defined in this file.
         */
        public static string SERENITY_JIRA_WORKFLOW;

        /**
         *  If set to true, JIRA Workflow is active.
         */
        public static bool SERENITY_JIRA_WORKFLOW_ACTIVE = false;

        [Obsolete] public static string THUCYDIDES_HISTORY;

        /**
         * Base directory in which history files are stored.
         */
        public static string SERENITY_HISTORY;

        [Obsolete] public static int THUCYDIDES_BROWSER_HEIGHT;

        /**
         *  Redimension the browser to enable larger screenshots.
         */
        public static int SERENITY_BROWSER_HEIGHT;

        [Obsolete] public static int THUCYDIDES_BROWSER_WIDTH;

        /**
         *  Redimension the browser to enable larger screenshots.
         */
        public static int SERENITY_BROWSER_WIDTH;

        [Obsolete] public static bool THUCYDIDES_BROWSER_MAXIMIZED = false;

        /**
         * Set to true to get WebDriver to maximise the Browser window before the tests are executed.
         */
        public static bool SERENITY_BROWSER_MAXIMIZED = false;

        [Obsolete] public static int THUCYDIDES_RESIZED_IMAGE_WIDTH;

        /**
         * If set, resize screenshots to this size to save space.
         */
        public static int? SERENITY_RESIZED_IMAGE_WIDTH;

        [Obsolete] public static string THUCYDIDES_PUBLIC_URL;

        /**
         * Public URL where the Thucydides reports will be displayed.
         * This is mainly for use by plugins.
         */
        public static string SERENITY_PUBLIC_URL;

        [Obsolete] public static bool THUCYDIDES_ACTIVATE_FIREBUGS;

        /**
         * Activate the Firebugs plugin for firefox.
         * Useful for debugging, but not very when running the tests on a build server.
         * It is not activated by default.
         */
        public static bool SERENITY_ACTIVATE_FIREBUGS = false;

        /**
         * Enable applets in Firefox.
         * Use the system property 'security.enable_java'.
         * Applets slow down webdriver, so are disabled by default.
         */
        public static string SECURITY_ENABLE_JAVA = "security.enable_java";

        [Obsolete] public static bool THUCYDIDES_ACTIVATE_HIGHLIGHTING;

        public static bool SERENITY_ACTIVATE_HIGHLIGHTING = false ;

        public enum BatchStrategyEnum
        {
            DivideEqually,
            DivideByTestCount
        }
        [Obsolete] public static BatchStrategyEnum THUCYDIDES_BATCH_STRATEGY = BatchStrategyEnum.DivideEqually;

        /**
         * Batch strategy to use for parallel batches.
         * Allowed values - DIVIDE_EQUALLY (default) and DIVIDE_BY_TEST_COUNT
         */
        public static BatchStrategyEnum SERENITY_BATCH_STRATEGY = BatchStrategyEnum.DivideEqually;

        [Obsolete] public static int THUCYDIDES_BATCH_COUNT;

        /**
         *  A deprecated property that is synonymous with thucydides.batch.size
         */
        public static int SERENITY_BATCH_COUNT;

        [Obsolete] public static int THUCYDIDES_BATCH_SIZE;

        /**
         *  If batch testing is being used, this is the size of the batches being executed.
         */
        public static int SERENITY_BATCH_SIZE;

        [Obsolete] public static int THUCYDIDES_BATCH_NUMBER;

        /**
         * If batch testing is being used, this is the number of the batch being run on this machine.
         */
        public static int SERENITY_BATCH_NUMBER;

        [Obsolete] public static string THUCYDIDES_PROXY_HTTP;

        /**
         * HTTP Proxy URL configuration for Firefox and PhantomJS
         */
        public static string SERENITY_PROXY_HTTP;

        [Obsolete]
        [Description("thucydides.proxy.http_port")]
        public static int? THUCYDIDES_PROXY_HTTP_PORT;

        /**
         * HTTP Proxy port configuration for Firefox and PhantomJS
         * Use 'thucydides.proxy.http_port'
         */
        [Description("serenity.proxy.http_port")] public static int? SERENITY_PROXY_HTTP_PORT;

        [Obsolete] public static string THUCYDIDES_PROXY_TYPE;

        /**
         * HTTP Proxy type configuration for Firefox and PhantomJS
         */
        public static string SERENITY_PROXY_TYPE;

        [Obsolete] public static string THUCYDIDES_PROXY_USER;

        /**
         * HTTP Proxy username configuration for Firefox and PhantomJS
         */
        public static string SERENITY_PROXY_USER;

        [Obsolete] public static string THUCYDIDES_PROXY_PASSWORD;

        /**
         * HTTP Proxy password configuration for Firefox and PhantomJS
         */
        public static string SERENITY_PROXY_PASSWORD;

        /**
         * How long webdriver waits for elements to appear by default, in milliseconds.
         */
        public static double WEBDRIVER_TIMEOUTS_IMPLICITLYWAIT;

        /**
         * How long webdriver waits by default when you use a fluent waiting method, in milliseconds.
         */
        public static double WEBDRIVER_WAIT_FOR_TIMEOUT;

        [Obsolete] public static string THUCYDIDES_EXT_PACKAGES;

        /**
         * Extension packages. This is a list of packages that will be scanned for custom TagProvider implementations.
         * To add a custom tag provider, just implement the TagProvider interface and specify the root package for this
         * provider in this parameter.
         */
        public static string SERENITY_EXT_PACKAGES;

        /**
         * Arguments to be passed to the Chrome driver, separated by commas.
         */
        public static string CHROME_SWITCHES;

        /**
         * Path to a Chrome-driver specific extensions file
         */
        public static string CHROME_EXTENSION;

        /**
         * Preferences to be passed to the Firefox driver, separated by semi-colons (commas often appear in the preference
         * values.
         */
        public static string FIREFOX_PREFERENCES;

        /**
         * Full path to the Firefox profile to be used with Firefox.
         * You can include Java system properties ${user.dir}, ${user.home} and the Windows environment variables %APPDIR%
         * and %USERPROFILE (assuming these are correctly set in the environment)
         */
        public static string WEBDRIVER_FIREFOX_PROFILE;

        [Obsolete] public static string THUCYDIDES_JQUERY_INTEGRATION;

        /**
         * Enable JQuery integration.
         * If set to true, JQuery will be injected into any page that does not already have it.
         * This option is deactivated by default, as it can slow down page loading.
         */
        public static bool SERENITY_JQUERY_INTEGRATION = false;

        [Description("saucelabs.browserName")]
        public static string SAUCELABS_BROWSERNAME;

        public static string SAUCELABS_TARGET_PLATFORM;

        public static string SAUCELABS_DRIVER_VERSION;

        public static string SAUCELABS_TEST_NAME;
        /**
         * SauceLabs URL if running the web tests on SauceLabs
         */
        public static string SAUCELABS_URL;

        /**
         * SauceLabs access key - if provided, Thucydides can generate links to the SauceLabs reports that don't require a login.
         */
        public static string SAUCELABS_ACCESS_KEY;

        /**
         * SauceLabs user id - if provided with the access key,
         * Thucydides can generate links to the SauceLabs reports that don't require a login.
         */
        public static string SAUCELABS_USER_ID;

        /**
         * Override the default implicit timeout value for the Saucelabs driver.
         */
        public static string SAUCELABS_IMPLICIT_TIMEOUT;

        /**
         * Saucelabs records screenshots as well as videos by default. Since Thucydides also records screenshots,
         * this feature is disabled by default. It can be reactivated using this system property.
         */
        public static bool SAUCELABS_RECORD_SCREENSHOTS = true ;

        /**
         * BrowserStack Hub URL if running the tests on BrowserStack Cloud
         */
        public static string BROWSERSTACK_URL;

        public static string BROWSERSTACK_OS;


        [Description("browserstack.os_version")]
        public static string BROWSERSTACK_OS_VERSION;

        /**
         * Browserstack uses this property for desktop browsers, like firefox, chrome and IE.
         */
        public static string BROWSERSTACK_BROWSER;

        /**
         * Browserstack uses this one for android and iphone.
         */
        [Description("browserstack.browserName")] public static string BROWSERSTACK_BROWSERNAME;

        public static string BROWSERSTACK_BROWSER_VERSION;

        /**
         * BrowserStack mobile device name on which tests should be run
         */
        public static string BROWSERSTACK_DEVICE;

        /**
         * Set the screen orientation of BrowserStack mobile device
         */
        public static string BROWSERSTACK_DEVICE_ORIENTATION;

        /**
         * Specify a name for a logical group of builds on BrowserStack
         */
        public static string BROWSERSTACK_PROJECT;

        /**
         * Specify a name for a logical group of tests on BrowserStack
         */
        public static string BROWSERSTACK_BUILD;

        /**
         * Specify an identifier for the test run on BrowserStack
         */
        public static string BROWSERSTACK_SESSION_NAME;

        /**
         * For Testing against internal/local servers on BrowserStack
         */
        public static string BROWSERSTACK_LOCAL;

        /**
         * Generates screenshots at various steps in tests on BrowserStack
         */
        public static string BROWSERSTACK_DEBUG;

        /**
         * Sets resolution of VM on BrowserStack
         */
        public static string BROWSERSTACK_RESOLUTION;

        public static string BROWSERSTACK_SELENIUM_VERSION;

        /**
         * Disable flash on Internet Explorer on BrowserStack
         */
        public static bool? BROWSERSTACK_IE_NO_FLASH;

        /**
         * Specify the Internet Explorer webdriver version on BrowserStack
         */
        public static string BROWSERSTACK_IE_DRIVER;

        /**
         *  Enable the popup blocker in Internet Explorer on BrowserStack
         */
        public static bool? BROWSERSTACK_IE_ENABLE_POPUPS;

        [Obsolete] public static int? THUCYDIDES_FILE_IO_RETRY_TIMEOUT;

        /**
         * Timeout (in seconds) for retrying file I/O.
         * Used in net.thucydides.core.resources.FileResources.copyResourceTo().
         * Sometimes, file I/O fails on Windows machine due to the way Windows handles memory-mapped
         * files (http://stackoverflow.com/questions/3602783/file-access-synchronized-on-java-object).
         * This property, if set, will retry copying the resource till timeout. A default value is used
         * if the property is not set.
         */
        public static int? SERENITY_FILE_IO_RETRY_TIMEOUT;

        public enum LoggingTypeEnum
        {
            Quiet,
            Normal,
            Verbose
        }
        [Obsolete]
        public static LoggingTypeEnum THUCYDIDES_LOGGING,

        /**
         * Three levels are supported: QUIET, NORMAL and VERBOSE
         */
        public static LoggingTypeEnum SERENITY_LOGGING = LoggingTypeEnum.Normal;

        [Obsolete] public static string THUCYDIDES_TEST_ROOT;

        /**
         * The root package for the tests in a given project.
         * If provided, Thucydides will log information about the total number of tests to be executed,
         * and keep a tally of the executed tests. It will also use this as the root package when determining the
         * capabilities associated with a test.
         * If you are using the File System Requirements provider, Thucydides will expect this directory structure to exist
         * at the top of the requirements tree. If you want to exclude packages in a requirements definition and start at a
         * lower level in the hierarchy, use the thucydides.requirement.exclusions property.
         * This is also used by the PackageAnnotationBasedTagProvider to know where to look for annotated requirements.
         */
        public static string SERENITY_TEST_ROOT;

        [Obsolete] public static string THUCYDIDES_REQUIREMENTS_DIR;

        /**
         * Use this property if you need to completely override the location of requirements for the File System Provider.
         */
        public static string SERENITY_REQUIREMENTS_DIR;

        [Obsolete] public static bool THUCYDIDES_USE_REQUIREMENTS_DIRECTORIES = true ;

        /**
         * By default, Thucydides will read requirements from the directory structure that contains the stories.
         * When other tag and requirements plugins are used, such as the JIRA plugin, this can cause conflicting
         * tags. Set this property to false to deactivate this feature (it is true by default).
         */
        public static bool SERENITY_USE_REQUIREMENTS_DIRECTORIES = true;

        [Obsolete] public static string THUCYDIDES_ANNOTATED_REQUIREMENTS_DIR;

        /**
         * Use this property if you need to completely override the location of requirements for the Annotated Provider.
         * This is recommended if you use File System and Annotated provider simultaneously.
         * The default value is stories.
         */
        public static string SERENITY_ANNOTATED_REQUIREMENTS_DIR;

        [Obsolete] public static string THUCYDIDES_LOWEST_REQUIREMENT_TYPE;

        /**
         * Determine what the lowest level requirement (test cases, feature files, story files, should be
         * called. 'Story' is used by default. 'feature' is a popular alternative.
         */
        public static string SERENITY_LOWEST_REQUIREMENT_TYPE;

        [Obsolete]
    THUCYDIDES_REQUIREMENT_TYPES,

        /**
         * The hierarchy of requirement types.
         * This is the list of requirement types to be used when reading requirements from the file system
         * and when organizing the reports. It is a comma-separated list of tags.The default value is: capability, feature
         */
        SERENITY_REQUIREMENT_TYPES,

        [Obsolete]
    THUCYDIDES_REQUIREMENT_EXCLUSIONS,

        /**
         * When deriving requirement types from a path, exclude any values from this comma-separated list.
         */
        SERENITY_REQUIREMENT_EXCLUSIONS,

        [Obsolete]
    THUCYDIDES_RELEASE_TYPES,

        /**
         * What tag names identify the release types (e.g. Release, Iteration, Sprint).
         * A comma-separated list. By default, "Release, Iteration"
         */
        SERENITY_RELEASE_TYPES,

        [Obsolete]
    THUCYDIDES_LOCATOR_FACTORY,

        /**
         * Normally, Serenity uses SmartElementLocatorFactory, an extension of the AjaxElementLocatorFactory
         * when instantiating page objects. This is to ensure that web elements are available and usable before they are used.
         * For alternative behaviour, you can set this value to DisplayedElementLocatorFactory, AjaxElementLocatorFactory or DefaultElementLocatorFactory.
         */
        SERENITY_LOCATOR_FACTORY,

        [Obsolete]
    THUCYDIDES_DATA_DIR,

        /**
         * Where Serenity stores local data.
         */
        SERENITY_DATA_DIR,

        /**
         * Allows you to override the default serenity.properties location for properties file.
         */
        PROPERTIES,

        [Obsolete]
    THUCYDIDES_TEST_REQUIREMENTS_BASEDIR,

        /**
         *  The base directory in which requirements are kept. It is assumed that this directory contains sub folders
         *  src/test/resources. If this property is set, the requirements are read from src/test/resources under this folder
         *  instead of the classpath or working directory. If you need to set an independent requirements directory that
         *  does not follow the src/test/resources convention, use thucydides.requirements.dir instead
         *
         *  This property is used to support situations where your working directory
         *  is different from the requirements base dir (for example when building a multi-module project from parent pom with
         *  requirements stored inside a sub-module : See Jira #Thucydides-100)
         */
        SERENITY_TEST_REQUIREMENTS_BASEDIR,


        /**
         * Set to true if you want the HTML source code to be recorded as well as the screenshots.
         * This is not currently used in the reports.
         */
        //    THUCYDIDES_STORE_HTML_SOURCE,

        [Obsolete]
    THUCYDIDES_KEEP_UNSCALED_SCREENSHOTS,

        /**
         * If set to true, a copy of the original screenshot will be kept when screenshots are scaled for the reports.
         * False by default to conserve disk space.
         */
        SERENITY_KEEP_UNSCALED_SCREENSHOTS,

        /**
         * If provided, only classes and/or methods with tags in this list will be executed. The parameter expects
         * a tag or comma-separated list of tags in the shortened form.
         * For example, -Dtags="iteration:I1" or -Dtags="color:red,flavor:strawberry"
         */
        TAGS,

        /**
         * If provided, each test in a test run will have these tags added.
         */
        INJECTED_TAGS,

        [Obsolete]
    THUCYDIDES_CSV_EXTRA_COLUMNS,

        /**
         * If set to true, historical flags will be displayed in test lists.
         * This must be set in conjunction with the serenity.historyDirectory property
         */
        SHOW_HISTORY_FLAGS,

        /**
         * Serenity will look in this directory for the previous build results, to use as a basis for the
         * historical flags shown in the test results. By default, the 'history' folder in the working directory will be used.
         */
        SERENITY_HISTORY_DIRECTORY("serenity.historyDirectory"),

        /**
         * Delete the history directory before a new set of results is recorded
         */
        DELETE_HISTORY_DIRECTORY,

        /**
         * Add extra columns to the CSV output, obtained from tag values.
         */
        SERENITY_CSV_EXTRA_COLUMNS,

        [Obsolete]
    THUCYDIDES_CONSOLE_HEADINGS,

        /**
         * Write the console headings using ascii-art ("ascii", default value) or in normal text ("normal")
         */
        SERENITY_CONSOLE_HEADINGS,

        [Obsolete]
    THUCYDIDES_CONSOLE_COLORS,
        SERENITY_CONSOLE_COLORS,

        /**
         * If set to true, Asciidoc formatting will be supported in the narrative texts.
         */
        NARRATIVE_FORMAT,

        /**
         * What format should test results be generated in.
         * By default, this is "json,xml".
         */
        OUTPUT_FORMATS,

        /**
         * If set to true (the default), allow markdown formatting in test outcome titles and descriptions.
         * This is a comma-separated lists of values from the following: story, narrative, step
         * By default, Markdown is enabled for story titles and narrative texts, but not for steps.
         */
        ENABLE_MARKDOWN,

        /**
         * Path to PhantomJS executable
         */
        PHANTOMJS_BINARY_PATH,

        /**
         * Path to the Gecko driver binary
         */
        WEBDRIVER_GECKO_DRIVER,

        /**
         * If set to true, don't format embedded tables in JBehave or Gherkin steps.
         * False by default.
         */
        IGNORE_EMBEDDED_TABLES,

        /**
         * If set, this will display the related tag statistics on the home page.
         * If you are using external requirements, you may not want to display these tags on the dashboard.
         */
        SHOW_RELATED_TAGS,

        /**
         * If set to true (the default value), a story tag will be extracted from the test case or feature file
         * containing the test.
         */
        USE_TEST_CASE_FOR_STORY_TAG,

        /**
         * Display the pie charts on the dashboard by default.
         * If this is set to false, the pie charts will be initially hidden on the dashboard.
         */
        SHOW_PIE_CHARTS,

        /**
         * If set, this will define the list of tag types to appear on the dashboard screens
         */
        DASHBOARD_TAG_LIST,

        /**
         * If set to false, render report names in a readable form as opposed to a hash format.
         * Note: this can cause problems on operating systems that limit path lengths such as Windows.
         */
        SERENITY_COMPRESS_FILENAMES,

        /**
         * If set, this will define the list of tag types to be excluded from the dashboard screens
         */
        DASHBOARD_EXCLUDED_TAG_LIST,

        /**
         * Format the JSON test outcomes nicely.
         * "true" or "false", turned off by default.
         */
        JSON_PRETTY_PRINTING,

        /**
         * What charset to use for JSON processing.
         * Defaults to UTF-8
         */
        JSON_CHARSET,

        /**
         * What charset to use for report generation.
         * Defaults to UTF-8
         */
        REPORT_CHARSET,

        /**
         * Stack traces are by default decluttered for readability.
         * For example, calls to instrumented code or internal test libraries is removed.
         * This behaviour can be deactivated by setting this property to false.
         */
        SIMPLIFIED_STACK_TRACES,

        [Obsolete]
    THUCYDIDES_DRY_RUN,

        /**
         * Run through the steps without actually executing them.
         */
        SERENITY_DRY_RUN,

        /**
         * What (human) language are the Cucumber feature files written in?
         * Defaults to "en"
         */
        FEATURE_FILE_LANGUAGE,

        /**
         * Display the context in the test title.
         * Set to false by default.
         * If the context is a browser type (chrome, ie, firefox, safari, opera), the browser icon will be displayed
         * If the context is an OS (linux, mac, windows, android, iphone), an icon will be displayed
         * Otherwise, the context name will be displayed at the start of the test title.
         */
        THUCYDIDES_DISPLAY_CONTEXT,

        /**
         * Include a context tag with a test if one is provided.
         * Set to 'true' by default
         */
        THUCYDIDES_ADD_CONTEXT_TAG,

        /**
         * What encoding to use for reading Cucumber feature files?
         * Defaults to system default encoding
         */
        FEATURE_FILE_ENCODING,

        /**
         * Fine-tune the number of threads Serenity uses for report generation.
         */
        REPORT_THREADS,
        REPORT_MAX_THREADS,
        REPORT_KEEP_ALIVE_TIME,

        /**
         * Set this to true if you want Serenity to report nested step structures for subsequent steps
         * after a step failure.
         */
        DEEP_STEP_EXECUTION_AFTER_FAILURES,


        /**
         * What test result (success,ignored, or pending) should be shown for manual annotated tests in the reports?
         */
        MANUAL_TEST_REPORT_RESULT,

        [Obsolete]
    THUCYDIDES_MAINTAIN_SESSION,

        /**
         * Keep the Thucydides session data between tests.
         * Normally, the session data is cleared between tests.
         */
        SERENITY_MAINTAIN_SESSION,

        /**
         * Path to PhantomJS SSL support
         */
        PHANTOMJS_SSL_PROTOCOL,

        /**
         * Comma-separated list of exception classes that should produce a compromised test in the reports.
         */
        SERENITY_COMPROMISED_ON,

        /**
         * Comma-separated list of exception classes that should produce an error in the reports.
         */
        SERENITY_ERROR_ON,

        /**
         * Comma-separated list of exception classes that should produce a failure in the reports.
         */
        SERENITY_FAIL_ON,

        /**
         * Comma-separated list of exception classes that should produce a pending test in the reports.
         */
        SERENITY_PENDING_ON,

        /**
         * If set to true, add a tag for test failures, based on the error type and message
         */
        SERENITY_TAG_FAILURES,

        /**
         * A comma-separated list of tag types for which human-readable report names will be generated.
         */
        SERENITY_LINKED_TAGS,

        /**
         * Should we assume that collections of webdriver elements are already on the page, or if we should wait for them to be available.
         * This property takes two values: Optimistic or Pessimistic. Optimistic means that the elements are assumed to be on the screen, and will be
         * loaded as-is immediately. This is the normal WebDriver behavior.
         * For applications with lots of ASynchronous activity, it is often better to wait until the elements are visible before using them. The Pessimistic
         * mode waits for at least one element to be visible before proceeding.
         * For legacy reasons, the default strategy is Pessimistic.
         */
        SERENITY_WEBDRIVER_COLLECTION_LOADING_STRATEGY("serenity.webdriver.collection_loading_strategy"),

        /**
         * Serenity will try to download drivers not present on the system.
         * If you don't want this behaviour, set this property to false
         */
        AUTOMATIC_DRIVER_DOWNLOAD,

        /**
         * If the Gecko Driver is on the system path, it will be used (with Marionnette) by default.
         * If you want to use the old-style Firefox driver, but have gecko on the system path,
         * then set this property to false.
         */
        USE_GECKO_DRIVER,

        /**
         * Use this property to pass options to Marionette using the 'moz:firefoxOptions' capability option.
         */
        GECKO_FIREFOX_OPTIONS,

        /**
         * Use this property to specify the maximum number of times to rerun the failing tests.
         */
        TEST_RETRY_COUNT,

        /**
         * Use this property to specify the maximum number of times to rerun the failing tests for cucumber tests.
         */
        TEST_RETRY_COUNT_CUCUMBER,

        /**
         * Record failures to a file specified by property rerun.failures.file or rerun.xml in current directory
         */
        RECORD_FAILURES,

        /**
         * Replay failures from a file specified by property rerun.failures.file or rerun.xml in current directory
         */
        REPLAY_FAILURES,

        /**
         * Location of the directory where the failure files are recorded.
         */
        RERUN_FAILURES_DIRECTORY,

        /**
         * Provide a text that distinguishes tests run in a particular environment or context from the same test
         * run in a different environment or context.
         */
        CONTEXT
    ;

    private String propertyName;
    public static final int DEFAULT_HEIGHT = 700;
    public static final int DEFAULT_WIDTH = 960;

    public static final String DEFAULT_HISTORY_DIRECTORY = "history";


    private final org.slf4j.Logger logger = LoggerFactory.getLogger(ThucydidesSystemProperty.class);

    ThucydidesSystemProperty(final String propertyName)
    {
        this.propertyName = propertyName;
    }

    ThucydidesSystemProperty()
    {
        this.propertyName = name().replaceAll("_", ".").toLowerCase();
    }

    public String getPropertyName()
    {
        return propertyName;
    }

    @Override
    public String toString()
    {
        return propertyName;
    }

    public String from(EnvironmentVariables environmentVariables)
    {
        return from(environmentVariables, null);
    }

    private Optional<String> legacyPropertyValueIfPresentIn(EnvironmentVariables environmentVariables)
    {
        String legacyValue = environmentVariables.getProperty(withLegacyPrefix(getPropertyName()));
        if (StringUtils.isNotEmpty(legacyValue))
        {
            logger.warn("Legacy property format detected for {}, please use the serenity.* format instead.", getPropertyName());
        }
        return Optional.fromNullable(legacyValue);
    }

    private String withLegacyPrefix(String propertyName)
    {
        return propertyName.replaceAll("serenity.", "thucydides.");
    }

    private String withSerenityPrefix(String propertyName)
    {
        return propertyName.replaceAll("thucydides.", "serenity.");
    }

    public String preferredName()
    {
        return withSerenityPrefix(getPropertyName());
    }

    public List<String> legacyNames()
    {
        List<String> names = new ArrayList<>(1);
        names.add(withLegacyPrefix(getPropertyName()));
        return names;
    }

    public String from(EnvironmentVariables environmentVariables, String defaultValue)
    {
        Optional<String> newPropertyValue
                = Optional.fromNullable(environmentVariables.getProperty(withSerenityPrefix(getPropertyName())));

        if (isDefined(newPropertyValue))
        {
            return newPropertyValue.get();
        }
        else
        {
            Optional<String> legacyValue = legacyPropertyValueIfPresentIn(environmentVariables);
            return (isDefined(legacyValue)) ? legacyValue.get() : defaultValue;
        }
    }

    private boolean isDefined(Optional<String> newPropertyValue)
    {
        return newPropertyValue.isPresent() && StringUtils.isNotEmpty(newPropertyValue.get());
    }

    public int integerFrom(EnvironmentVariables environmentVariables)
    {
        return integerFrom(environmentVariables, 0);
    }

    public int integerFrom(EnvironmentVariables environmentVariables, int defaultValue)
    {
        Optional<String> newPropertyValue
                = Optional.fromNullable(environmentVariables.getProperty(withSerenityPrefix(getPropertyName())));

        if (isDefined(newPropertyValue))
        {
            return Integer.parseInt(newPropertyValue.get());
        }
        else
        {
            Optional<String> legacyValue = legacyPropertyValueIfPresentIn(environmentVariables);
            return (isDefined(legacyValue)) ? Integer.parseInt(legacyValue.get()) : defaultValue;
        }
    }

    public Boolean booleanFrom(EnvironmentVariables environmentVariables)
    {
        return booleanFrom(environmentVariables, false);
    }

    public Boolean booleanFrom(EnvironmentVariables environmentVariables, Boolean defaultValue)
    {
        if (environmentVariables == null) { return defaultValue; }

        Optional<String> newPropertyValue
                = Optional.fromNullable(environmentVariables.getProperty(withSerenityPrefix(getPropertyName())));

        if (isDefined(newPropertyValue))
        {
            return Boolean.valueOf(newPropertyValue.get());
        }
        else
        {
            Optional<String> legacyValue = legacyPropertyValueIfPresentIn(environmentVariables);
            return (isDefined(legacyValue)) ? Boolean.valueOf(legacyValue.get()) : defaultValue;
        }
    }

    public boolean isDefinedIn(EnvironmentVariables environmentVariables)
    {
        return StringUtils.isNotEmpty(from(environmentVariables));
    }

}
}


