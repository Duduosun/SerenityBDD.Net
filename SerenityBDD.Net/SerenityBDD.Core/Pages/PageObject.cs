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

        private static readonly ILog Logger = LogManager.GetLogger(typeof(PageObject));

        private static readonly int WaitForElementPauseLength = 250;

        #endregion

        #region Private Vars

        private readonly EnvironmentVariables _environmentVariables;

        private readonly Sleeper _sleeper;
        private readonly Clock _webdriverClock;

        private SystemClock _clock;

        private IWebDriver _driver;
        private JavascriptExecutorFacade _javascriptExecutorFacade;

        private MatchingPageExpressions _matchingPageExpressions;

        private Pages _pages;

        private PageUrls _pageUrls;

        private RenderedPageObjectView _renderedView;
        private Duration _waitForElementTimeout;

        private Duration _waitForTimeout;

        #endregion

        #region Constructors

        protected PageObject()
        {
            _webdriverClock = new SystemClock();
            //TODO: Replace injectors
            //this.clock = Injectors.getInjector().getInstance<SystemClock>();
            //this.environmentVariables = Injectors.getInjector().getProvider(EnvironmentVariables.class).get();
            //this.environmentVariables = Injectors.getInjector().getInstance<EnvironmentVariables>();

            _sleeper = Sleeper.SYSTEM_SLEEPER;
            SetupPageUrls();
        }


        protected PageObject(IWebDriver driver, Action<PageObject> callback) : this()
        {
            SetDriver(driver);

            callback(this);
        }


        public PageObject(IWebDriver driver, EnvironmentVariables environmentVariables) : this()
        {
            this._environmentVariables = environmentVariables;
            SetDriver(driver);
        }

        #endregion

        #region Properties

        public IWebDriver WebDriver { get; set; }

        #endregion

        #region Public Methods

        public void AddJQuerySupport()
        {
            if (PageIsLoaded() && JqueryIntegrationIsActivated() && DriverIsJQueryCompatible())
            {
                var jQueryEnabledPage = JQueryEnabledPage.withDriver(GetDriver());
                jQueryEnabledPage.activateJQuery();
            }
        }

        public void BlurActiveElement()
        {
            GetJavascriptExecutorFacade().executeScript("document.activeElement.blur();");
        }

        /**
         * Override this method
         */

        public void CallWhenPageOpensMethods()
        {
            foreach (var annotatedMethod in MethodsAnnotatedWithWhenPageOpens())
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
                    Logger.Error("Could not execute @WhenPageOpens annotated method: ", e);
                    if (e is InvocationTargetException)
                        e = ((InvocationTargetException) e).getTargetException();

                    throw new UnableToInvokeWhenPageOpensMethods(annotatedMethod, e);
                }
        }

        public void ClickOn(IWebElement element)
        {
            element.Click();
        }

        /**
         * Does this page object work for this URL? When matching a URL, we check
         * with and without trailing slashes
         */

        public bool CompatibleWithUrl(string currentUrl)
        {
            return ThereAreNoPatternsDefined() || MatchUrlAgainstEachPattern(currentUrl);
        }

        /**
         * Check that the specified text appears somewhere in the page.
         */

        public bool ContainsAllText(params string[] textValues)
        {
            foreach (var textValue in textValues)
                if (!GetRenderedView().containsText(textValue))
                    return false;
            return true;
        }

        public bool ContainsElements(By bySelector)
        {
            return FindAll(bySelector).Any();
        }

        public bool ContainsElements(string xpathOrCssSelector)
        {
            return FindAll(xpathOrCssSelector).Any();
        }

        public bool ContainsText(string textValue
        )
        {
            return GetRenderedView().containsText(textValue);
        }

        /**
         * Does the specified web element contain a given text value. Useful for dropdowns and so on.
         *
         * [Obsolete] use element(IWebElement).containsText(textValue)
         */

        [Obsolete]
        public bool ContainsTextInElement(IWebElement element, string textValue)
        {
            return element.containsText(textValue);
        }

        /**
         * Provides a fluent API for querying web elements.
         */

        public WebElementFacade Element(IWebElement element)
        {
            return WebElementFacadeImpl.wrapWebElement(_driver, element, GetImplicitWaitTimeout(), GetWaitForTimeout(),
                NameOf(element));
        }

        /**
         * Provides a fluent API for querying web elements.
         */

        public WebElementFacade Element(By bySelector)
        {
            return WebElementFacadeImpl.wrapWebElement(_driver, bySelector, GetImplicitWaitTimeout(), GetWaitForTimeout(),
                bySelector.ToString());
        }

        /**
* Provides a fluent API for querying web elements.
*/

        public WebElementFacade Element(string xpathOrCssSelector)
        {
            return Element(this.XpathOrCssSelector(xpathOrCssSelector));
        }

        /**
         * Clear a field and enter a value into it.
         * This is a more fluent alternative to using the typeInto method.
         */

        public FieldEntry Enter(string value)
        {
            return new FieldEntry(value);
        }


        public object EvaluateJavascript(string script)
        {
            AddJQuerySupport();
            var js = new JavascriptExecutorFacade(_driver);
            return js.executeScript(script);
        }


        public object EvaluateJavascript(string script, params object[] args)
        {
            AddJQuerySupport();
            var js = new JavascriptExecutorFacade(_driver);
            return js.executeScript(script, args);
        }


        public WebElementFacade Find(IEnumerable<By> selectors)
        {
            WebElementFacade e = null;
            foreach (var selector in selectors)
                if (e == null)
                    e = Element(selector);
                else
                    e = e.find(selector);
            return e;
        }

        public WebElementFacade Find(params By[] selectors)
        {
            return Find(selectors.ToList());
        }

        public List<WebElementFacade> FindAll(By bySelector)
        {
            var matchingWebElements = _driver.FindElements(bySelector);
            return Convert(matchingWebElements, ToWebElementFacades());
        }


        public List<WebElementFacade> FindAll(string xpathOrCssSelector)
        {
            return FindAll(this.XpathOrCssSelector(xpathOrCssSelector));
        }

        public WebElementFacade FindBy(string xpathOrCssSelector)
        {
            return Element(xpathOrCssSelector);
        }

        public PageObject Foo()
        {
            return this;
        }


        public IAlert GetAlert()
        {
            return _driver.SwitchTo().Alert();
        }

        public IWebDriver GetDriver()
        {
            return _driver;
        }

        public Duration GetImplicitWaitTimeout()
        {
            if (_waitForElementTimeout == null)
            {
                var configuredWaitForTimeoutInMilliseconds =
                    ThucydidesSystemProperty.WEBDRIVER_TIMEOUTS_IMPLICITLYWAIT
                        .integerFrom(_environmentVariables, DefaultTimeouts.DEFAULT_IMPLICIT_WAIT_TIMEOUT);

                _waitForElementTimeout = new Duration(configuredWaitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
            }
            return _waitForElementTimeout;
        }

        public string GetSelectedLabelFrom(IWebElement dropdown)
        {
            return Dropdown.ForWebElement(dropdown).getSelectedLabel();
        }


        public ISet<string> GetSelectedOptionLabelsFrom(IWebElement dropdown)
        {
            return Dropdown.ForWebElement(dropdown).getSelectedOptionLabels();
        }

        public ISet<string> GetSelectedOptionValuesFrom(IWebElement dropdown)
        {
            return Dropdown.ForWebElement(dropdown).getSelectedOptionValues();
        }

        public string GetSelectedValueFrom(IWebElement dropdown)
        {
            return Dropdown.ForWebElement(dropdown).getSelectedValue();
        }

        public string GetTitle()
        {
            return _driver.Title;
        }

        [Obsolete]
        public Duration GetWaitForElementTimeout()
        {
            return GetImplicitWaitTimeout();
        }

        public Duration GetWaitForTimeout()
        {
            if (_waitForTimeout == null)
            {
                var configuredWaitForTimeoutInMilliseconds =
                        ThucydidesSystemProperty.WEBDRIVER_WAIT_FOR_TIMEOUT
                            .integerFrom(_environmentVariables, DefaultTimeouts.DEFAULT_WAIT_FOR_TIMEOUT)
                    ;
                _waitForTimeout = new Duration(configuredWaitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
            }
            return _waitForTimeout;
        }

        /**
         * Returns true if the specified element has the focus.
         *
         * [Obsolete] Use element(IWebElement).hasFocus() instead
         */

        public bool HasFocus(IWebElement element)
        {
            return element.Equals(_driver.SwitchTo().ActiveElement());
        }

        public long ImplicitTimoutMilliseconds()
        {
            return (long) GetImplicitWaitTimeout().In(TimeUnit.MILLISECONDS);
        }

        public RadioButtonGroup InRadioButtonGroup(string name)
        {
            return new RadioButtonGroup(GetDriver().FindElements(By.Name(name)));
        }

        /**
         * Returns true if at least one matching element is found on the page and is visible.
         */

        public bool IsElementVisible(By byCriteria)
        {
            return GetRenderedView().elementIsDisplayed(byCriteria);
        }


        public WebElementFacade JQuery(IWebElement element)
        {
            return this.Element(element);
        }

        public WebElementFacade JQuery(string xpathOrCssSelector)
        {
            return Element(xpathOrCssSelector);
        }


        public bool MatchesAnyUrl()
        {
            return ThereAreNoPatternsDefined();
        }

        public T MoveTo<T>(string xpathOrCssSelector)
            where T : WebElementFacade
        {
            if (!DriverIsDisabled())
                WithAction().MoveToElement(FindBy(xpathOrCssSelector)).Perform();
            return (T) FindBy(xpathOrCssSelector);
        }

        public WebElementFacade MoveTo(By locator)
        {
            if (!DriverIsDisabled())
                WithAction().MoveToElement(Find(locator)).Perform();
            return Find(locator);
        }

        /**
         * Open the IWebDriver browser using a paramaterized URL. Parameters are
         * represented in the URL using {0}, {1}, etc.
         */

        public void Open(params string[] parameterValues)
        {
            Open(OpenMode.CheckUrlPatterns, parameterValues);
        }

        public void Open(string urlTemplateName, string[] parameterValues)
        {
            Open(OpenMode.CheckUrlPatterns, urlTemplateName, parameterValues);
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

        public void Open()
        {
            Open(OpenMode.CheckUrlPatterns);
        }

        public void OpenAt(string startingUrl)
        {
            OpenPageAtUrl(UpdateUrlWithBaseUrlIfDefined(startingUrl));
            CallWhenPageOpensMethods();
        }


        /**
         * Opens page without checking URL patterns. Same as open(string...)) otherwise.
         */

        public void OpenUnchecked(params string[] parameterValues)
        {
            Open(OpenMode.IgnoreUrlPatterns, parameterValues);
        }

        /**
         * Opens page without checking URL patterns. Same as {@link #open(string, string[])} otherwise.
         */

        public void OpenUnchecked(string urlTemplateName, params string[] parameterValues)
        {
            Open(OpenMode.IgnoreUrlPatterns, urlTemplateName, parameterValues);
        }

        /**
         * Opens page without checking URL patterns. Same as {@link #open()} otherwise.
         */

        public void OpenUnchecked()
        {
            Open(OpenMode.IgnoreUrlPatterns);
        }


        public void ResetImplicitTimeout()
        {
            if (_driver.instanceof(typeof(ConfigurableTimeouts)))
            {
                _waitForElementTimeout = ((ConfigurableTimeouts) _driver).resetTimeouts();
            }
            else
            {
                _waitForElementTimeout = GetDefaultImplicitTimeout();
                _driver.Manage().Timeouts().ImplicitWait = _waitForElementTimeout;
            }
        }

        public void SelectFromDropdown(IWebElement dropdown, string visibleLabel)
        {
            Dropdown.ForWebElement(dropdown).Select(visibleLabel);
            NotifyScreenChange();
        }


        public void SelectMultipleItemsFromDropdown(IWebElement dropdown, params string[] selectedLabels)
        {
            Dropdown.ForWebElement(dropdown).selectMultipleItems(selectedLabels);
            NotifyScreenChange();
        }

        public void SetCheckbox(IWebElement field, bool value)
        {
            var checkbox = new Checkbox(field);
            checkbox.setChecked(value);
            NotifyScreenChange();
        }

        public void SetDefaultBaseUrl(string defaultBaseUrl)
        {
            _pageUrls.overrideDefaultBaseUrl(defaultBaseUrl);
        }

        public void SetDriver(IWebDriver driver)
        {
            SetDriver(driver, GetImplicitWaitTimeout());
        }

        public void SetImplicitTimeout(int duration, TimeUnit unit)
        {
            _waitForElementTimeout = new Duration(duration, unit);
            SetDriverImplicitTimeout(_waitForElementTimeout);
        }

        public void SetPages(Pages pages)
        {
            this._pages = pages;
        }

        /**
         * Only for testing purposes.
         */

        public void SetPageUrls(PageUrls pageUrls)
        {
            this._pageUrls = pageUrls;
        }

        public void SetWaitForElementTimeout(long waitForTimeoutInMilliseconds)
        {
            _waitForElementTimeout = new Duration(waitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
        }

        public void SetWaitForTimeout(long waitForTimeoutInMilliseconds)
        {
            _waitForTimeout = new Duration(waitForTimeoutInMilliseconds, TimeUnit.MILLISECONDS);
            GetRenderedView().setWaitForTimeout(_waitForTimeout);
        }

        /**
         * Use the @At annotation (if present) to check that a page object is displaying the correct page.
         * Will throw an exception if the current URL does not match the expected one.
         */

        public void ShouldBeDisplayed()
        {
            EnsurePageIsOnAMatchingUrl();
        }

        /**
         * Fail the test if this element is not displayed (rendered) on the screen.
         */

        public void ShouldBeVisible(IWebElement field)
        {
            Element(field).shouldBeVisible();
        }

        public void ShouldBeVisible(By byCriteria)
        {
            WaitOnPage().until(ExpectedConditions.visibilityOfElementLocated(byCriteria));
        }

        /**
         * Check that all of the specified texts appears somewhere in the page.
         */

        public void ShouldContainAllText(params string[] textValues)
        {
            if (!ContainsAllText(textValues))
            {
                string errorMessage = $"One of the text elements in {textValues} was not found in the page";
                throw new NoSuchElementException(errorMessage);
            }
        }

        /**
         * Check that the specified text appears somewhere in the page.
         */

        public void ShouldContainText(string textValue)
        {
            if (!ContainsText(textValue))
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
        public void ShouldContainTextInElement(IWebElement element, string textValue)
        {
            element.shouldContainText(textValue);
        }

        public void ShouldNotBeVisible(IWebElement field)
        {
            try
            {
                Element(field).shouldNotBeVisible();
            }
            catch (NoSuchElementException e)
            {
                // A non-existant element is not visible
            }
        }

        public void ShouldNotBeVisible(By byCriteria)
        {
            var matchingElements = GetDriver().FindElements(byCriteria);
            if (matchingElements.Any())
                WaitOnPage().until(ExpectedConditions.invisibilityOfElementLocated(byCriteria));
        }

        /*
         * Check that the element does not contain a given text.
         * [Obsolete] use element(IWebElement).shouldNotContainText(textValue)
         */

        [Obsolete]
        public void ShouldNotContainTextInElement(IWebElement element, string textValue)
        {
            element.shouldNotContainText(textValue);
        }

        public T SwitchToPage<T>(Type pageObjectClass)
            where T : PageObject
        {
            if (_pages.getDriver() == null)
                _pages.setDriver(_driver);

            return (T) _pages.getPage(pageObjectClass);
        }

        public IReadOnlyCollection<IWebElement> ThenReturnElementList(By byListCriteria)
        {
            return _driver.FindElements(byListCriteria);
        }

        /**
         * Clear a field and enter a value into it.
         */

        public void TypeInto(IWebElement field, string value)
        {
            Element(field).type(value);
        }

        public string UpdateUrlWithBaseUrlIfDefined(string startingUrl)
        {
            var baseUrl = _pageUrls.getSystemBaseUrl();
            if (baseUrl != null && !StringUtils.isEmpty(baseUrl))
                return ReplaceHost(startingUrl, baseUrl);
            return startingUrl;
        }

        public FileToUpload Upload(string filename)
        {
            return new FileToUpload(_driver, filename).useRemoteDriver(IsDefinedRemoteUrl());
        }


        public FileToUpload UploadData(string data)
        {
            var datafile = Files.createTempFile("upload", "data");
            Files.write(datafile, Encoding.UTF8.GetBytes(data));
            return new FileToUpload(_driver, datafile.toAbsolutePath()).useRemoteDriver(IsDefinedRemoteUrl());
        }

        public FileToUpload UploadData(byte[] data)
        {
            var datafile = Files.createTempFile("upload", "data");
            Files.write(datafile, data);
            return new FileToUpload(_driver, datafile.toAbsolutePath()).useRemoteDriver(IsDefinedRemoteUrl());
        }

        public PageObject WaitFor(string xpathOrCssSelector)
        {
            return WaitForRenderedElements(this.XpathOrCssSelector(xpathOrCssSelector));
        }

        public PageObject WaitFor(ExpectedCondition expectedCondition)
        {
            GetRenderedView().waitFor(expectedCondition);
            return this;
        }

        public WaitForBuilder WaitFor(int duration)
        {
            return new PageObjectStepDelayer(_clock, this).waitFor(duration);
        }


        public WebElementFacade WaitFor(WebElementFacade webElement)
        {
            return GetRenderedView().waitFor(webElement);
        }

        public WebElementFacade WaitFor(IWebElement element)
        {
            return WaitFor(JQuery(element));
        }

        public PageObject WaitForAbsenceOf(string xpathOrCssSelector)
        {
            return WaitForRenderedElementsToDisappear(this.XpathOrCssSelector(xpathOrCssSelector));
        }

        /**
         * Waits for all of a number of text blocks to appear on the screen.
         */

        public PageObject WaitForAllTextToAppear(params string[] expectedTexts)
        {
            GetRenderedView().waitForAllTextToAppear(expectedTexts);
            return this;
        }

        public void WaitForAngularRequestsToFinish()
        {
            if ((bool) GetJavascriptExecutorFacade().executeScript(
                "return (typeof angular !== 'undefined')? true : false;"))
                GetJavascriptExecutorFacade().executeAsyncScript(
                    "var callback = arguments[arguments.length - 1];"
                    +
                    "angular.element(document.body).injector().get('$browser').notifyWhenNoOutstandingRequests(callback);");
        }

        public PageObject WaitForAnyRenderedElementOf(params By[] expectedElements)
        {
            GetRenderedView().waitForAnyRenderedElementOf(expectedElements);
            return this;
        }

        /**
         * Waits for any of a number of text blocks to appear anywhere on the
         * screen.
         */

        public PageObject WaitForAnyTextToAppear(params string[] expectedText)
        {
            GetRenderedView().waitForAnyTextToAppear(expectedText);
            return this;
        }

        public PageObject WaitForAnyTextToAppear(IWebElement element, params string[] expectedText)
        {
            GetRenderedView().waitForAnyTextToAppear(element, expectedText);

            return this;
        }

        public ThucydidesFluentWait<IWebDriver> WaitForCondition()
        {
            return new NormalFluentWait(_driver, _webdriverClock, _sleeper)
                    .withTimeout(GetWaitForTimeout(), TimeUnit.MILLISECONDS)
                    .pollingEvery(WaitForElementPauseLength, TimeUnit.MILLISECONDS)
                    .ignoring(typeof(NoSuchElementException), typeof(NoSuchFrameException))
                ;
        }

        public PageObject WaitForPresenceOf(string xpathOrCssSelector)
        {
            return WaitForRenderedElementsToBePresent(this.XpathOrCssSelector(xpathOrCssSelector));
        }

        public PageObject WaitForRenderedElements(
            By byElementCriteria
        )
        {
            GetRenderedView().waitFor(byElementCriteria);
            return this;
        }

        public PageObject WaitForRenderedElementsToBePresent(
            By byElementCriteria
        )
        {
            GetRenderedView().waitForPresenceOf(byElementCriteria);

            return this;
        }


        public PageObject WaitForRenderedElementsToDisappear(
            By byElementCriteria
        )
        {
            GetRenderedView().waitForElementsToDisappear(byElementCriteria);
            return this;
        }

        /**
         * Waits for a given text to appear anywhere on the page.
         */

        public PageObject WaitForTextToAppear(string expectedText)
        {
            GetRenderedView().waitForText(expectedText);
            return this;
        }

        /**
         * Waits for a given text to appear inside the element.
         */

        public PageObject WaitForTextToAppear(IWebElement element, string expectedText)
        {
            GetRenderedView().waitForText(element, expectedText);
            return this;
        }

        /**
         * Waits for a given text to appear anywhere on the page.
         */

        public PageObject WaitForTextToAppear(string expectedText, TimeSpan timeout)
        {
            GetRenderedView().waitForTextToAppear(expectedText, timeout);
            return this;
        }

        /**
         * Waits for a given text to disappear from the element.
         */

        public PageObject WaitForTextToDisappear(IWebElement element, string expectedText)
        {
            if (!DriverIsDisabled())
                WaitForCondition()?.until(ElementDoesNotContain(element, expectedText));
            return this;
        }

        public PageObject WaitForTextToDisappear(string expectedText)
        {
            return WaitForTextToDisappear(expectedText, GetWaitForTimeout());
        }

        /**
         * Waits for a given text to not be anywhere on the page.
         */

        public PageObject WaitForTextToDisappear(string expectedText, TimeSpan timespan)
        {
            GetRenderedView().waitForTextToDisappear(expectedText, timespan);
            return this;
        }

        public PageObject WaitForTextToDisappear(string expectedText, long timeoutInMilliseconds)
        {
            GetRenderedView().waitForTextToDisappear(expectedText, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
            return this;
        }

        public long WaitForTimeoutInMilliseconds()
        {
            return (long) GetWaitForTimeout().In(TimeUnit.MILLISECONDS);
        }

        public PageObject WaitForTitleToAppear(string expectedTitle)
        {
            WaitOnPage().until(ExpectedConditions.titleIs(expectedTitle));
            return this;
        }

        public PageObject WaitForTitleToDisappear(
            string expectedTitle
        )
        {
            GetRenderedView().waitForTitleToDisappear(expectedTitle);
            return this;
        }


        public ThucydidesFluentWait<IWebDriver> WaitForWithRefresh()
        {
            return new FluentWaitWithRefresh(_driver, _webdriverClock, _sleeper)
                    .withTimeout(GetWaitForTimeout(),
                        TimeUnit.MILLISECONDS).pollingEvery(WaitForElementPauseLength, TimeUnit.MILLISECONDS)
                    .ignoring(typeof(NoSuchElementException), typeof(NoSuchFrameException))
                ;
        }

        public Actions WithAction()
        {
            var proxiedDriver = ((WebDriverFacade) GetDriver()).getProxiedDriver();
            return new Actions(proxiedDriver);
        }

        public PageObject WithDriver(IWebDriver driver)
        {
            SetDriver(driver);
            return this;
        }

        public static string[] WithParameters(params string[] parameterValues)
        {
            return parameterValues;
        }

        public RenderedPageObjectView WithTimeoutOf(int timeout, TimeUnit units)
        {
            return WithTimeoutOf(new Duration(timeout, units));
        }

        public RenderedPageObjectView WithTimeoutOf(Duration timeout)
        {
            return new RenderedPageObjectView(_driver, this, timeout, false);
        }

        public By XpathOrCssSelector(string src)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        private void CheckUrlPatterns(
            OpenMode openMode
        )
        {
            if (openMode == OpenMode.CheckUrlPatterns)
                EnsurePageIsOnAMatchingUrl();
        }

        private List<TTGT> Convert<TSRC, TTGT>(ReadOnlyCollection<TSRC> matchingWebElements,
            Converter<TSRC, TTGT> converter)
        {
            return converter.Convert(matchingWebElements).ToList();
        }

        private bool DriverIsDisabled()
        {
            return StepEventBus.getEventBus().webdriverCallsAreSuspended();
        }


        private ExpectedCondition ElementDoesNotContain(IWebElement element, string expectedText)
        {
            return new ExpectedCondition(driver => element.Text.Contains(expectedText));
        }

        private void EnsurePageIsOnAMatchingUrl()
        {
            if (!MatchesAnyUrl())
            {
                var currentUrl = GetDriver().Url;
                if (!CompatibleWithUrl(currentUrl))
                    ThisIsNotThePageYourLookingFor();
            }
        }

        private Duration GetDefaultImplicitTimeout()
        {
            var configuredTimeout =
                ThucydidesSystemProperty.WEBDRIVER_TIMEOUTS_IMPLICITLYWAIT.integerFrom(_environmentVariables);
            return new Duration(configuredTimeout, TimeUnit.MILLISECONDS);
        }

        private MatchingPageExpressions GetMatchingPageExpressions()
        {
            if (_matchingPageExpressions == null)
                _matchingPageExpressions = new MatchingPageExpressions(this);
            return _matchingPageExpressions;
        }

        private string HostComponentFrom(string protocol, string host, int port)
        {
            var u = new UriBuilder(protocol, host, port);
            return u.Uri.ToString();
        }

        private void InitializePage()
        {
            AddJQuerySupport();
            CallWhenPageOpensMethods();
        }

        private bool IsDefinedRemoteUrl()
        {
            var isRemoteUrl = ThucydidesSystemProperty.WEBDRIVER_REMOTE_URL.isDefinedIn(_environmentVariables);
            var isSaucelabsUrl = ThucydidesSystemProperty.SAUCELABS_URL.isDefinedIn(_environmentVariables);
            var isBrowserStack = ThucydidesSystemProperty.BROWSERSTACK_URL.isDefinedIn(_environmentVariables);
            return isRemoteUrl || isSaucelabsUrl || isBrowserStack;
        }


        private bool JavascriptIsSupportedIn(IWebDriver webDriver)
        {
            throw new NotImplementedException();
        }

        private bool JqueryIntegrationIsActivated()
        {
            return ThucydidesSystemProperty.THUCYDIDES_JQUERY_INTEGRATION.booleanFrom(_environmentVariables, true);
        }

        private bool MatchUrlAgainstEachPattern(string currentUrl)
        {
            return GetMatchingPageExpressions().matchUrlAgainstEachPattern(currentUrl);
        }

        private IEnumerable<MethodInfo> MethodsAnnotatedWithWhenPageOpens()
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

        private string NameOf(IWebElement element)
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

        private void NotifyScreenChange()
        {
            StepEventBus.getEventBus().notifyScreenChange();
        }

        private void Open(OpenMode openMode, string[] parameterValues)
        {
            var startingUrl = _pageUrls.getStartingUrl(parameterValues);
            Logger.DebugFormat("Opening page at url {0}", startingUrl);
            OpenPageAtUrl(startingUrl);
            CheckUrlPatterns(openMode);
            InitializePage();
            Logger.Debug("Page opened");
        }

        private void Open(OpenMode openMode, string urlTemplateName, string[] parameterValues)
        {
            var startingUrl = _pageUrls.getNamedUrl(urlTemplateName, parameterValues);
            Logger.DebugFormat("Opening page at url {0}", startingUrl);
            OpenPageAtUrl(startingUrl);
            CheckUrlPatterns(openMode);
            InitializePage();
            Logger.Debug("Page opened");
        }

        private void Open(
            OpenMode openMode
        )
        {
            var startingUrl = UpdateUrlWithBaseUrlIfDefined(_pageUrls.getStartingUrl());
            OpenPageAtUrl(startingUrl);
            CheckUrlPatterns(openMode);
            InitializePage();
        }

        private void OpenPageAtUrl(string startingUrl)
        {
            GetDriver().Navigate().GoToUrl(startingUrl);
            if (JavascriptIsSupportedIn(GetDriver()))
                AddJQuerySupport();
        }

        private bool PageIsLoaded()
        {
            try
            {
                return DriverIsInstantiated() && GetDriver().Url != null;
            }
            catch (WebDriverException e)
            {
                return false;
            }
        }

        private string ReplaceHost(string starting, string baseUrlText)
        {
            var updatedUrl = starting;
            try
            {
                var startingUrl = new Uri(starting);
                var baseUrl = new Uri(baseUrlText);

                var startingHostComponent = HostComponentFrom(startingUrl.Scheme,
                    startingUrl.Host,
                    startingUrl.Port);
                var baseHostComponent = HostComponentFrom(baseUrl.Scheme,
                    baseUrl.Host,
                    baseUrl.Port);
                updatedUrl = starting.Replace(startingHostComponent, baseHostComponent);
            }
            catch (UriFormatException e)
            {
                Logger.ErrorFormat("Failed to analyse default page URL: Starting URL: {0}, Base URL: {1}", starting,
                    baseUrlText);
                Logger.Error("URL analysis failed with exception:", e);
            }

            return updatedUrl;
        }

        private void SetDriverImplicitTimeout(Duration implicitTimeout)
        {
            if (_driver.instanceof(typeof(ConfigurableTimeouts)))
                ((ConfigurableTimeouts) _driver).setImplicitTimeout(implicitTimeout);
            else
                _driver.Manage().Timeouts().ImplicitWait = implicitTimeout.TimeSpan;
        }

        private void SetupPageUrls()
        {
            SetPageUrls(new PageUrls(this));
        }

        private bool ThereAreNoPatternsDefined()
        {
            return GetMatchingPageExpressions().isEmpty();
        }

        private void ThisIsNotThePageYourLookingFor()
        {
            var errorDetails = "This is not the page you're looking for: "
                               + "I was looking for a page compatible with " + GetType() + " but "
                               + "I was at the URL " + GetDriver().Url;

            throw new WrongPageError(errorDetails);
        }

        private Converter<IWebElement, WebElementFacade> ToWebElementFacades()
        {
            return new Converter<IWebElement, WebElementFacade>();
        }
        

        private long WaitForTimeoutInSecondsWithAMinimumOfOneSecond()
        {
            return
                (long) (GetWaitForTimeout().In(TimeUnit.SECONDS) < 1 ? 1 : GetWaitForTimeout().In(TimeUnit.SECONDS));
        }

        private WebDriverWait WaitOnPage()
        {
            return new WebDriverWait(_driver, GetWaitForTimeout());
            //        waitForTimeoutInSecondsWithAMinimumOfOneSecond());
        }

        #endregion

        #region Protected Methods

        protected bool DriverIsInstantiated()
        {
            if (GetDriver().instanceof(typeof(WebDriverFacade)))
                return ((WebDriverFacade) GetDriver()).isEnabled() && ((WebDriverFacade) GetDriver()).isInstantiated();
            return true;
        }


        protected bool DriverIsJQueryCompatible()
        {
            try
            {
                if (GetDriver().instanceof(typeof(WebDriverFacade)))
                    return SupportedWebDriver.ForClass(((WebDriverFacade) GetDriver()).getDriverClass())
                        .supportsJavascriptInjection();
                return SupportedWebDriver.ForClass(GetDriver()).supportsJavascriptInjection();
            }
            catch (ArgumentOutOfRangeException probablyAMockedDriver)
            {
                return false;
            }
        }

        protected ThucydidesFluentAdapter Fluent()
        {
            return new ThucydidesFluentAdapter(GetDriver());
        }

        protected SystemClock GetClock()
        {
            return _clock;
        }

        protected JavascriptExecutorFacade GetJavascriptExecutorFacade()
        {
            if (_javascriptExecutorFacade == null)
                _javascriptExecutorFacade = new JavascriptExecutorFacade(_driver);
            return _javascriptExecutorFacade;
        }

        protected RenderedPageObjectView GetRenderedView()
        {
            if (_renderedView == null)
                _renderedView = new RenderedPageObjectView(_driver, this, GetWaitForTimeout(), true);
            return _renderedView;
        }

        protected void SetDriver(IWebDriver driver, TimeSpan timeout)
        {
            this._driver = driver;
            new DefaultPageObjectInitialiser(driver, timeout).apply(this);
        }

        protected void WaitABit(long timeInMilliseconds)
        {
            GetClock().pauseFor(timeInMilliseconds);
        }

        #endregion

        #region Nested Types

        public class FieldEntry
        {
            #region Private Vars

            private readonly IWebDriver _driver;
            private readonly PageObject _parent;

            private readonly string _value;

            #endregion

            #region Constructors

            public FieldEntry(PageObject parent, IWebDriver driver)
            {
                _parent = parent;
                _driver = driver;
            }

            public FieldEntry(string value)
            {
                this._value = value;
            }

            #endregion

            #region Public Methods

            public void Into(IWebElement field)
            {
                _parent.Element(field).type(_value);
            }

            public void Into(WebElementFacade field)
            {
                field.type(_value);
            }

            public void IntoField(By bySelector)
            {
                var field = _parent.GetDriver().FindElement(bySelector);
                Into(field);
            }

            #endregion
        }

        private enum OpenMode
        {
            CheckUrlPatterns,
            IgnoreUrlPatterns
        }

        #endregion
    }
}