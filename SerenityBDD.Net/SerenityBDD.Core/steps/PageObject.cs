using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SerenityBDD.Core.time;
using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.Steps
{
    public class PageObject

    {
        #region Statics

        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(PageObject));

        private static readonly int WAIT_FOR_ELEMENT_PAUSE_LENGTH = 250;

        #endregion

        #region Private Vars

        private readonly EnvironmentVariables environmentVariables;

        private readonly Sleeper sleeper;
        private readonly Clock webdriverClock;

        private SystemClock clock;

        private IWebDriver driver;
        private JavascriptExecutorFacade javascriptExecutorFacade;

        private MatchingPageExpressions matchingPageExpressions;

        private Pages pages;

        private PageUrls pageUrls;

        private RenderedPageObjectView renderedView;
        private Duration waitForElementTimeout;

        private Duration waitForTimeout;

        #endregion

        #region Constructors

        protected PageObject()
        {
            webdriverClock = new SystemClock();
            //TODO: Replace injectors
            //this.clock = Injectors.getInjector().getInstance<SystemClock>();
            //this.environmentVariables = Injectors.getInjector().getProvider(EnvironmentVariables.class).get();
            //this.environmentVariables = Injectors.getInjector().getInstance<EnvironmentVariables>();

            sleeper = Sleeper.SYSTEM_SLEEPER;
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

        #endregion

        #region Properties

        public IWebDriver WebDriver { get; set; }

        #endregion

        #region Public Methods

        public void addJQuerySupport()
        {
            if (pageIsLoaded() && jqueryIntegrationIsActivated() && driverIsJQueryCompatible())
            {
                var jQueryEnabledPage = JQueryEnabledPage.withDriver(getDriver());
                jQueryEnabledPage.activateJQuery();
            }
        }

        public void blurActiveElement()
        {
            getJavascriptExecutorFacade().executeScript("document.activeElement.blur();");
        }

        /**
         * Override this method
         */

        public void callWhenPageOpensMethods()
        {
            foreach (var annotatedMethod in methodsAnnotatedWithWhenPageOpens())
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
                        e = ((InvocationTargetException) e).getTargetException();

                    throw new UnableToInvokeWhenPageOpensMethods(annotatedMethod, e);
                }
        }

        public void clickOn(IWebElement element)
        {
            element.Click();
        }

        /**
         * Does this page object work for this URL? When matching a URL, we check
         * with and without trailing slashes
         */

        public bool compatibleWithUrl(string currentUrl)
        {
            return thereAreNoPatternsDefined() || matchUrlAgainstEachPattern(currentUrl);
        }

        /**
         * Check that the specified text appears somewhere in the page.
         */

        public bool containsAllText(params string[] textValues)
        {
            foreach (var textValue in textValues)
                if (!getRenderedView().containsText(textValue))
                    return false;
            return true;
        }

        public bool containsElements(By bySelector)
        {
            return findAll(bySelector).Any();
        }

        public bool containsElements(string xpathOrCssSelector)
        {
            return findAll(xpathOrCssSelector).Any();
        }

        public bool containsText(string textValue
        )
        {
            return getRenderedView().containsText(textValue);
        }

        /**
         * Does the specified web element contain a given text value. Useful for dropdowns and so on.
         *
         * [Obsolete] use element(IWebElement).containsText(textValue)
         */

        [Obsolete]
        public bool containsTextInElement(IWebElement element, string textValue)
        {
            return element.containsText(textValue);
        }

        /**
         * Provides a fluent API for querying web elements.
         */

        public WebElementFacade element(IWebElement element)
        {
            return WebElementFacadeImpl.wrapWebElement(driver, element, getImplicitWaitTimeout(), getWaitForTimeout(),
                nameOf(element));
        }

        /**
         * Provides a fluent API for querying web elements.
         */

        public WebElementFacade element(By bySelector)
        {
            return WebElementFacadeImpl.wrapWebElement(driver, bySelector, getImplicitWaitTimeout(), getWaitForTimeout(),
                bySelector.ToString());
        }

        /**
* Provides a fluent API for querying web elements.
*/

        public WebElementFacade element(string xpathOrCssSelector)
        {
            return element(this.xpathOrCssSelector(xpathOrCssSelector));
        }

        /**
         * Clear a field and enter a value into it.
         * This is a more fluent alternative to using the typeInto method.
         */

        public FieldEntry enter(string value)
        {
            return new FieldEntry(value);
        }


        public object evaluateJavascript(string script)
        {
            addJQuerySupport();
            var js = new JavascriptExecutorFacade(driver);
            return js.executeScript(script);
        }


        public object evaluateJavascript(string script, params object[] args)
        {
            addJQuerySupport();
            var js = new JavascriptExecutorFacade(driver);
            return js.executeScript(script, args);
        }


        public WebElementFacade find(IEnumerable<By> selectors)
        {
            WebElementFacade e = null;
            foreach (var selector in selectors)
                if (e == null)
                    e = element(selector);
                else
                    e = e.find(selector);
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


        public List<WebElementFacade> findAll(string xpathOrCssSelector)
        {
            return findAll(this.xpathOrCssSelector(xpathOrCssSelector));
        }

        public WebElementFacade findBy(string xpathOrCssSelector)
        {
            return element(xpathOrCssSelector);
        }

        public PageObject foo()
        {
            return this;
        }


        public IAlert getAlert()
        {
            return driver.SwitchTo().Alert();
        }

        public IWebDriver getDriver()
        {
            return driver;
        }

        public Duration getImplicitWaitTimeout()
        {
            if (waitForElementTimeout == null)
            {
                var configuredWaitForTimeoutInMilliseconds =
                    ThucydidesSystemProperty.WEBDRIVER_TIMEOUTS_IMPLICITLYWAIT
                        .integerFrom(environmentVariables, DefaultTimeouts.DEFAULT_IMPLICIT_WAIT_TIMEOUT);

                waitForElementTimeout = new Duration(configuredWaitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
            }
            return waitForElementTimeout;
        }

        public string getSelectedLabelFrom(IWebElement dropdown)
        {
            return Dropdown.ForWebElement(dropdown).getSelectedLabel();
        }


        public ISet<string> getSelectedOptionLabelsFrom(IWebElement dropdown)
        {
            return Dropdown.ForWebElement(dropdown).getSelectedOptionLabels();
        }

        public ISet<string> getSelectedOptionValuesFrom(IWebElement dropdown)
        {
            return Dropdown.ForWebElement(dropdown).getSelectedOptionValues();
        }

        public string getSelectedValueFrom(IWebElement dropdown)
        {
            return Dropdown.ForWebElement(dropdown).getSelectedValue();
        }

        public string getTitle()
        {
            return driver.Title;
        }

        [Obsolete]
        public Duration getWaitForElementTimeout()
        {
            return getImplicitWaitTimeout();
        }

        public Duration getWaitForTimeout()
        {
            if (waitForTimeout == null)
            {
                var configuredWaitForTimeoutInMilliseconds =
                        ThucydidesSystemProperty.WEBDRIVER_WAIT_FOR_TIMEOUT
                            .integerFrom(environmentVariables, DefaultTimeouts.DEFAULT_WAIT_FOR_TIMEOUT)
                    ;
                waitForTimeout = new Duration(configuredWaitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
            }
            return waitForTimeout;
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

        public long implicitTimoutMilliseconds()
        {
            return (long) getImplicitWaitTimeout().In(TimeUnit.MILLISECONDS);
        }

        public RadioButtonGroup inRadioButtonGroup(string name)
        {
            return new RadioButtonGroup(getDriver().FindElements(By.Name(name)));
        }

        /**
         * Returns true if at least one matching element is found on the page and is visible.
         */

        public bool isElementVisible(By byCriteria)
        {
            return getRenderedView().elementIsDisplayed(byCriteria);
        }


        public WebElementFacade JQuery(IWebElement element)
        {
            return this.element(element);
        }

        public WebElementFacade JQuery(string xpathOrCssSelector)
        {
            return element(xpathOrCssSelector);
        }


        public bool matchesAnyUrl()
        {
            return thereAreNoPatternsDefined();
        }

        public T moveTo<T>(string xpathOrCssSelector)
            where T : WebElementFacade
        {
            if (!driverIsDisabled())
                withAction().MoveToElement(findBy(xpathOrCssSelector)).Perform();
            return (T) findBy(xpathOrCssSelector);
        }

        public WebElementFacade moveTo(By locator)
        {
            if (!driverIsDisabled())
                withAction().MoveToElement(find(locator)).Perform();
            return find(locator);
        }

        /**
         * Open the IWebDriver browser using a paramaterized URL. Parameters are
         * represented in the URL using {0}, {1}, etc.
         */

        public void open(params string[] parameterValues)
        {
            open(OpenMode.CHECK_URL_PATTERNS, parameterValues);
        }

        public void open(string urlTemplateName, string[] parameterValues)
        {
            open(OpenMode.CHECK_URL_PATTERNS, urlTemplateName, parameterValues);
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

        public void openAt(string startingUrl)
        {
            openPageAtUrl(updateUrlWithBaseUrlIfDefined(startingUrl));
            callWhenPageOpensMethods();
        }


        /**
         * Opens page without checking URL patterns. Same as open(string...)) otherwise.
         */

        public void openUnchecked(params string[] parameterValues)
        {
            open(OpenMode.IGNORE_URL_PATTERNS, parameterValues);
        }

        /**
         * Opens page without checking URL patterns. Same as {@link #open(string, string[])} otherwise.
         */

        public void openUnchecked(string urlTemplateName, params string[] parameterValues)
        {
            open(OpenMode.IGNORE_URL_PATTERNS, urlTemplateName, parameterValues);
        }

        /**
         * Opens page without checking URL patterns. Same as {@link #open()} otherwise.
         */

        public void openUnchecked()
        {
            open(OpenMode.IGNORE_URL_PATTERNS);
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

        public void selectFromDropdown(IWebElement dropdown, string visibleLabel)
        {
            Dropdown.ForWebElement(dropdown).Select(visibleLabel);
            notifyScreenChange();
        }


        public void selectMultipleItemsFromDropdown(IWebElement dropdown, params string[] selectedLabels)
        {
            Dropdown.ForWebElement(dropdown).selectMultipleItems(selectedLabels);
            notifyScreenChange();
        }

        public void setCheckbox(IWebElement field, bool value)
        {
            var checkbox = new Checkbox(field);
            checkbox.setChecked(value);
            notifyScreenChange();
        }

        public void setDefaultBaseUrl(string defaultBaseUrl)
        {
            pageUrls.overrideDefaultBaseUrl(defaultBaseUrl);
        }

        public void setDriver(IWebDriver driver)
        {
            setDriver(driver, getImplicitWaitTimeout());
        }

        public void setImplicitTimeout(int duration, TimeUnit unit)
        {
            waitForElementTimeout = new Duration(duration, unit);
            setDriverImplicitTimeout(waitForElementTimeout);
        }

        public void setPages(Pages pages)
        {
            this.pages = pages;
        }

        /**
         * Only for testing purposes.
         */

        public void setPageUrls(PageUrls pageUrls)
        {
            this.pageUrls = pageUrls;
        }

        public void setWaitForElementTimeout(long waitForTimeoutInMilliseconds)
        {
            waitForElementTimeout = new Duration(waitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
        }

        public void setWaitForTimeout(long waitForTimeoutInMilliseconds)
        {
            waitForTimeout = new Duration(waitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
            getRenderedView().setWaitForTimeout(waitForTimeout);
        }

        /**
         * Use the @At annotation (if present) to check that a page object is displaying the correct page.
         * Will throw an exception if the current URL does not match the expected one.
         */

        public void shouldBeDisplayed()
        {
            ensurePageIsOnAMatchingUrl();
        }

        /**
         * Fail the test if this element is not displayed (rendered) on the screen.
         */

        public void shouldBeVisible(IWebElement field)
        {
            element(field).shouldBeVisible();
        }

        public void shouldBeVisible(By byCriteria)
        {
            waitOnPage().until(ExpectedConditions.visibilityOfElementLocated(byCriteria));
        }

        /**
         * Check that all of the specified texts appears somewhere in the page.
         */

        public void shouldContainAllText(params string[] textValues)
        {
            if (!containsAllText(textValues))
            {
                string errorMessage = $"One of the text elements in {textValues} was not found in the page";
                throw new NoSuchElementException(errorMessage);
            }
        }

        /**
         * Check that the specified text appears somewhere in the page.
         */

        public void shouldContainText(string textValue)
        {
            if (!containsText(textValue))
            {
                string errorMessage = $"The text {textValue} was not found in the page";
                throw new NoSuchElementException(errorMessage);
            }
        }

        /*
         * Check that the element contains a given text.
         * [Obsolete] use element(IWebElement).shouldContainText(textValue)
         */

        [Obsolete]
        public void shouldContainTextInElement(IWebElement element, string textValue)
        {
            element.shouldContainText(textValue);
        }

        public void shouldNotBeVisible(IWebElement field)
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

        public void shouldNotBeVisible(By byCriteria)
        {
            var matchingElements = getDriver().FindElements(byCriteria);
            if (matchingElements.Any())
                waitOnPage().until(ExpectedConditions.invisibilityOfElementLocated(byCriteria));
        }

        /*
         * Check that the element does not contain a given text.
         * [Obsolete] use element(IWebElement).shouldNotContainText(textValue)
         */

        [Obsolete]
        public void shouldNotContainTextInElement(IWebElement element, string textValue)
        {
            element.shouldNotContainText(textValue);
        }

        public T switchToPage<T>(Type pageObjectClass)
            where T : PageObject
        {
            if (pages.getDriver() == null)
                pages.setDriver(driver);

            return (T) pages.getPage(pageObjectClass);
        }

        public IReadOnlyCollection<IWebElement> thenReturnElementList(By byListCriteria)
        {
            return driver.FindElements(byListCriteria);
        }

        /**
         * Clear a field and enter a value into it.
         */

        public void typeInto(IWebElement field, string value)
        {
            element(field).type(value);
        }

        public string updateUrlWithBaseUrlIfDefined(string startingUrl)
        {
            var baseUrl = pageUrls.getSystemBaseUrl();
            if (baseUrl != null && !StringUtils.isEmpty(baseUrl))
                return replaceHost(startingUrl, baseUrl);
            return startingUrl;
        }

        public FileToUpload upload(string filename)
        {
            return new FileToUpload(driver, filename).useRemoteDriver(isDefinedRemoteUrl());
        }


        public FileToUpload uploadData(string data)
        {
            var datafile = Files.createTempFile("upload", "data");
            Files.write(datafile, Encoding.UTF8.GetBytes(data));
            return new FileToUpload(driver, datafile.toAbsolutePath()).useRemoteDriver(isDefinedRemoteUrl());
        }

        public FileToUpload uploadData(byte[] data)
        {
            var datafile = Files.createTempFile("upload", "data");
            Files.write(datafile, data);
            return new FileToUpload(driver, datafile.toAbsolutePath()).useRemoteDriver(isDefinedRemoteUrl());
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

        public WaitForBuilder waitFor(int duration)
        {
            return new PageObjectStepDelayer(clock, this).waitFor(duration);
        }


        public WebElementFacade waitFor(WebElementFacade webElement)
        {
            return getRenderedView().waitFor(webElement);
        }

        public WebElementFacade waitFor(IWebElement element)
        {
            return waitFor(JQuery(element));
        }

        public PageObject waitForAbsenceOf(string xpathOrCssSelector)
        {
            return waitForRenderedElementsToDisappear(this.xpathOrCssSelector(xpathOrCssSelector));
        }

        /**
         * Waits for all of a number of text blocks to appear on the screen.
         */

        public PageObject waitForAllTextToAppear(params string[] expectedTexts)
        {
            getRenderedView().waitForAllTextToAppear(expectedTexts);
            return this;
        }

        public void waitForAngularRequestsToFinish()
        {
            if ((bool) getJavascriptExecutorFacade().executeScript(
                "return (typeof angular !== 'undefined')? true : false;"))
                getJavascriptExecutorFacade().executeAsyncScript(
                    "var callback = arguments[arguments.length - 1];"
                    +
                    "angular.element(document.body).injector().get('$browser').notifyWhenNoOutstandingRequests(callback);");
        }

        public PageObject waitForAnyRenderedElementOf(params By[] expectedElements)
        {
            getRenderedView().waitForAnyRenderedElementOf(expectedElements);
            return this;
        }

        /**
         * Waits for any of a number of text blocks to appear anywhere on the
         * screen.
         */

        public PageObject waitForAnyTextToAppear(params string[] expectedText)
        {
            getRenderedView().waitForAnyTextToAppear(expectedText);
            return this;
        }

        public PageObject waitForAnyTextToAppear(IWebElement element, params string[] expectedText)
        {
            getRenderedView().waitForAnyTextToAppear(element, expectedText);

            return this;
        }

        public ThucydidesFluentWait<IWebDriver> waitForCondition()
        {
            return new NormalFluentWait(driver, webdriverClock, sleeper)
                    .withTimeout(getWaitForTimeout(), TimeUnit.MILLISECONDS)
                    .pollingEvery(WAIT_FOR_ELEMENT_PAUSE_LENGTH, TimeUnit.MILLISECONDS)
                    .ignoring(typeof(NoSuchElementException), typeof(NoSuchFrameException))
                ;
        }

        public PageObject waitForPresenceOf(string xpathOrCssSelector)
        {
            return waitForRenderedElementsToBePresent(this.xpathOrCssSelector(xpathOrCssSelector));
        }

        public PageObject waitForRenderedElements(
            By byElementCriteria
        )
        {
            getRenderedView().waitFor(byElementCriteria);
            return this;
        }

        public PageObject waitForRenderedElementsToBePresent(
            By byElementCriteria
        )
        {
            getRenderedView().waitForPresenceOf(byElementCriteria);

            return this;
        }


        public PageObject waitForRenderedElementsToDisappear(
            By byElementCriteria
        )
        {
            getRenderedView().waitForElementsToDisappear(byElementCriteria);
            return this;
        }

        /**
         * Waits for a given text to appear anywhere on the page.
         */

        public PageObject waitForTextToAppear(string expectedText)
        {
            getRenderedView().waitForText(expectedText);
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

        /**
         * Waits for a given text to appear anywhere on the page.
         */

        public PageObject waitForTextToAppear(string expectedText, TimeSpan timeout)
        {
            getRenderedView().waitForTextToAppear(expectedText, timeout);
            return this;
        }

        /**
         * Waits for a given text to disappear from the element.
         */

        public PageObject waitForTextToDisappear(IWebElement element, string expectedText)
        {
            if (!driverIsDisabled())
                waitForCondition()?.until(elementDoesNotContain(element, expectedText));
            return this;
        }

        public PageObject waitForTextToDisappear(string expectedText)
        {
            return waitForTextToDisappear(expectedText, getWaitForTimeout());
        }

        /**
         * Waits for a given text to not be anywhere on the page.
         */

        public PageObject waitForTextToDisappear(string expectedText, TimeSpan timespan)
        {
            getRenderedView().waitForTextToDisappear(expectedText, timespan);
            return this;
        }

        public PageObject waitForTextToDisappear(string expectedText, long timeoutInMilliseconds)
        {
            getRenderedView().waitForTextToDisappear(expectedText, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
            return this;
        }

        public long waitForTimeoutInMilliseconds()
        {
            return (long) getWaitForTimeout().In(TimeUnit.MILLISECONDS);
        }

        public PageObject waitForTitleToAppear(string expectedTitle)
        {
            waitOnPage().until(ExpectedConditions.titleIs(expectedTitle));
            return this;
        }

        public PageObject waitForTitleToDisappear(
            string expectedTitle
        )
        {
            getRenderedView().waitForTitleToDisappear(expectedTitle);
            return this;
        }


        public ThucydidesFluentWait<IWebDriver> waitForWithRefresh()
        {
            return new FluentWaitWithRefresh(driver, webdriverClock, sleeper)
                    .withTimeout(getWaitForTimeout(),
                        TimeUnit.MILLISECONDS).pollingEvery(WAIT_FOR_ELEMENT_PAUSE_LENGTH, TimeUnit.MILLISECONDS)
                    .ignoring(typeof(NoSuchElementException), typeof(NoSuchFrameException))
                ;
        }

        public Actions withAction()
        {
            var proxiedDriver = ((WebDriverFacade) getDriver()).getProxiedDriver();
            return new Actions(proxiedDriver);
        }

        public PageObject withDriver(IWebDriver driver)
        {
            setDriver(driver);
            return this;
        }

        public static string[] withParameters(params string[] parameterValues)
        {
            return parameterValues;
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

        #endregion

        #region Private Methods

        private void checkUrlPatterns(
            OpenMode openMode
        )
        {
            if (openMode == OpenMode.CHECK_URL_PATTERNS)
                ensurePageIsOnAMatchingUrl();
        }

        private List<TTGT> convert<TSRC, TTGT>(ReadOnlyCollection<TSRC> matchingWebElements,
            Converter<TSRC, TTGT> converter)
        {
            return converter.Convert(matchingWebElements).ToList();
        }

        private bool driverIsDisabled()
        {
            return StepEventBus.getEventBus().webdriverCallsAreSuspended();
        }


        private ExpectedCondition elementDoesNotContain(IWebElement element, string expectedText)
        {
            return new ExpectedCondition(driver => element.Text.Contains(expectedText));
        }

        private void ensurePageIsOnAMatchingUrl()
        {
            if (!matchesAnyUrl())
            {
                var currentUrl = getDriver().Url;
                if (!compatibleWithUrl(currentUrl))
                    thisIsNotThePageYourLookingFor();
            }
        }

        private Duration getDefaultImplicitTimeout()
        {
            var configuredTimeout =
                ThucydidesSystemProperty.WEBDRIVER_TIMEOUTS_IMPLICITLYWAIT.integerFrom(environmentVariables);
            return new Duration(configuredTimeout, TimeUnit.MILLISECONDS);
        }

        private MatchingPageExpressions getMatchingPageExpressions()
        {
            if (matchingPageExpressions == null)
                matchingPageExpressions = new MatchingPageExpressions(this);
            return matchingPageExpressions;
        }

        private string hostComponentFrom(string protocol, string host, int port)
        {
            var u = new UriBuilder(protocol, host, port);
            return u.Uri.ToString();
        }

        private void initializePage()
        {
            addJQuerySupport();
            callWhenPageOpensMethods();
        }

        private bool isDefinedRemoteUrl()
        {
            var isRemoteUrl = ThucydidesSystemProperty.WEBDRIVER_REMOTE_URL.isDefinedIn(environmentVariables);
            var isSaucelabsUrl = ThucydidesSystemProperty.SAUCELABS_URL.isDefinedIn(environmentVariables);
            var isBrowserStack = ThucydidesSystemProperty.BROWSERSTACK_URL.isDefinedIn(environmentVariables);
            return isRemoteUrl || isSaucelabsUrl || isBrowserStack;
        }


        private bool javascriptIsSupportedIn(IWebDriver webDriver)
        {
            throw new NotImplementedException();
        }

        private bool jqueryIntegrationIsActivated()
        {
            return ThucydidesSystemProperty.THUCYDIDES_JQUERY_INTEGRATION.booleanFrom(environmentVariables, true);
        }

        private bool matchUrlAgainstEachPattern(string currentUrl)
        {
            return getMatchingPageExpressions().matchUrlAgainstEachPattern(currentUrl);
        }

        private IEnumerable<MethodInfo> methodsAnnotatedWithWhenPageOpens()
        {
            var methods = MethodFinder.inClass(GetType()).getAllMethods();
            var annotatedMethods = new List<MethodInfo>();
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<WhenPageOpens>();
                if (attr != null)
                    if (method.GetParameters().Length == 0)
                        annotatedMethods.Add(method);
                    else
                        throw new PageOpenMethodCannotHaveParametersException(method);
            }
            return annotatedMethods;
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

        private void notifyScreenChange()
        {
            StepEventBus.getEventBus().notifyScreenChange();
        }

        private void open(OpenMode openMode, string[] parameterValues)
        {
            var startingUrl = pageUrls.getStartingUrl(parameterValues);
            LOGGER.DebugFormat("Opening page at url {0}", startingUrl);
            openPageAtUrl(startingUrl);
            checkUrlPatterns(openMode);
            initializePage();
            LOGGER.Debug("Page opened");
        }

        private void open(OpenMode openMode, string urlTemplateName, string[] parameterValues)
        {
            var startingUrl = pageUrls.getNamedUrl(urlTemplateName, parameterValues);
            LOGGER.DebugFormat("Opening page at url {0}", startingUrl);
            openPageAtUrl(startingUrl);
            checkUrlPatterns(openMode);
            initializePage();
            LOGGER.Debug("Page opened");
        }

        private void open(
            OpenMode openMode
        )
        {
            var startingUrl = updateUrlWithBaseUrlIfDefined(pageUrls.getStartingUrl());
            openPageAtUrl(startingUrl);
            checkUrlPatterns(openMode);
            initializePage();
        }

        private void openPageAtUrl(string startingUrl)
        {
            getDriver().Navigate().GoToUrl(startingUrl);
            if (javascriptIsSupportedIn(getDriver()))
                addJQuerySupport();
        }

        private bool pageIsLoaded()
        {
            try
            {
                return driverIsInstantiated() && getDriver().Url != null;
            }
            catch (WebDriverException e)
            {
                return false;
            }
        }

        private string replaceHost(string starting, string baseUrlText)
        {
            var updatedUrl = starting;
            try
            {
                var startingUrl = new Uri(starting);
                var baseUrl = new Uri(baseUrlText);

                var startingHostComponent = hostComponentFrom(startingUrl.Scheme,
                    startingUrl.Host,
                    startingUrl.Port);
                var baseHostComponent = hostComponentFrom(baseUrl.Scheme,
                    baseUrl.Host,
                    baseUrl.Port);
                updatedUrl = starting.Replace(startingHostComponent, baseHostComponent);
            }
            catch (UriFormatException e)
            {
                LOGGER.ErrorFormat("Failed to analyse default page URL: Starting URL: {0}, Base URL: {1}", starting,
                    baseUrlText);
                LOGGER.Error("URL analysis failed with exception:", e);
            }

            return updatedUrl;
        }

        private void setDriverImplicitTimeout(Duration implicitTimeout)
        {
            if (driver.instanceof(typeof(ConfigurableTimeouts)))
                ((ConfigurableTimeouts) driver).setImplicitTimeout(implicitTimeout);
            else
                driver.Manage().Timeouts().ImplicitWait = implicitTimeout.TimeSpan;
        }

        private void setupPageUrls()
        {
            setPageUrls(new PageUrls(this));
        }

        private bool thereAreNoPatternsDefined()
        {
            return getMatchingPageExpressions().isEmpty();
        }

        private void thisIsNotThePageYourLookingFor()
        {
            var errorDetails = "This is not the page you're looking for: "
                               + "I was looking for a page compatible with " + GetType() + " but "
                               + "I was at the URL " + getDriver().Url;

            throw new WrongPageError(errorDetails);
        }

        private Converter<IWebElement, WebElementFacade> toWebElementFacades()
        {
            return new Converter<IWebElement, WebElementFacade>();
        }
        

        private long waitForTimeoutInSecondsWithAMinimumOfOneSecond()
        {
            return
                (long) (getWaitForTimeout().In(TimeUnit.SECONDS) < 1 ? 1 : getWaitForTimeout().In(TimeUnit.SECONDS));
        }

        private WebDriverWait waitOnPage()
        {
            return new WebDriverWait(driver, getWaitForTimeout());
            //        waitForTimeoutInSecondsWithAMinimumOfOneSecond());
        }

        #endregion

        #region Protected Methods

        protected bool driverIsInstantiated()
        {
            if (getDriver().instanceof(typeof(WebDriverFacade)))
                return ((WebDriverFacade) getDriver()).isEnabled() && ((WebDriverFacade) getDriver()).isInstantiated();
            return true;
        }


        protected bool driverIsJQueryCompatible()
        {
            try
            {
                if (getDriver().instanceof(typeof(WebDriverFacade)))
                    return SupportedWebDriver.ForClass(((WebDriverFacade) getDriver()).getDriverClass())
                        .supportsJavascriptInjection();
                return SupportedWebDriver.ForClass(getDriver()).supportsJavascriptInjection();
            }
            catch (ArgumentOutOfRangeException probablyAMockedDriver)
            {
                return false;
            }
        }

        protected ThucydidesFluentAdapter fluent()
        {
            return new ThucydidesFluentAdapter(getDriver());
        }

        protected SystemClock getClock()
        {
            return clock;
        }

        protected JavascriptExecutorFacade getJavascriptExecutorFacade()
        {
            if (javascriptExecutorFacade == null)
                javascriptExecutorFacade = new JavascriptExecutorFacade(driver);
            return javascriptExecutorFacade;
        }

        protected RenderedPageObjectView getRenderedView()
        {
            if (renderedView == null)
                renderedView = new RenderedPageObjectView(driver, this, getWaitForTimeout(), true);
            return renderedView;
        }

        protected void setDriver(IWebDriver driver, TimeSpan timeout)
        {
            this.driver = driver;
            new DefaultPageObjectInitialiser(driver, timeout).apply(this);
        }

        protected void waitABit(long timeInMilliseconds)
        {
            getClock().pauseFor(timeInMilliseconds);
        }

        #endregion

        #region Nested Types

        public class FieldEntry
        {
            #region Private Vars

            private readonly IWebDriver _driver;
            private readonly PageObject _parent;

            private readonly string value;

            #endregion

            #region Constructors

            public FieldEntry(PageObject parent, IWebDriver driver)
            {
                _parent = parent;
                _driver = driver;
            }

            public FieldEntry(string value)
            {
                this.value = value;
            }

            #endregion

            #region Public Methods

            public void into(IWebElement field)
            {
                _parent.element(field).type(value);
            }

            public void into(WebElementFacade field)
            {
                field.type(value);
            }

            public void intoField(By bySelector)
            {
                var field = _parent.getDriver().FindElement(bySelector);
                into(field);
            }

            #endregion
        }

        private enum OpenMode
        {
            CHECK_URL_PATTERNS,
            IGNORE_URL_PATTERNS
        }

        #endregion
    }
}