using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using SerenityBDD.Core.Steps;
using SerenityBDD.Core.time;
using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.BuildInfo
{
    public class BuildInfoProvider
    {
        private readonly EnvironmentVariables environmentVariables;
        private readonly DriverCapabilityRecord driverCapabilityRecord;

        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(BuildInfoProvider));

        public BuildInfoProvider(EnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
            this.driverCapabilityRecord = Injectors.getInjector().getInstance<DriverCapabilityRecord>();
        }

        public BuildProperties getBuildProperties()
        {
            Dictionary<string, string> generalProperties = new Dictionary<string, string>();
            generalProperties.Add("Default Driver", ThucydidesSystemProperty.DRIVER.From(environmentVariables, "firefox"));
            generalProperties.Add("Operating System", JavaSystem.getProperty("os.name") + " version " + JavaSystem.getProperty("os.version"));
            addRemoteDriverPropertiesTo(generalProperties);
            addSaucelabsPropertiesTo(generalProperties);
            addCustomPropertiesTo(generalProperties);

            var drivers = driverCapabilityRecord.getDrivers();
            Dictionary<string, Properties> driverPropertiesMap = driverCapabilityRecord.getDriverCapabilities();

            return new BuildProperties(generalProperties, drivers, driverPropertiesMap);
        }

        private void addRemoteDriverPropertiesTo(Dictionary<string, string> buildProperties)
        {
            if (ThucydidesSystemProperty.WEBDRIVER_REMOTE_DRIVER.isDefinedIn(environmentVariables))
            {
                buildProperties.Add("Remote driver", ThucydidesSystemProperty.WEBDRIVER_REMOTE_DRIVER.From(environmentVariables));
                if (ThucydidesSystemProperty.WEBDRIVER_REMOTE_BROWSER_VERSION.From(environmentVariables) != null)
                {
                    buildProperties.Add("Remote browser version", ThucydidesSystemProperty.WEBDRIVER_REMOTE_BROWSER_VERSION.From(environmentVariables));
                }
                if (ThucydidesSystemProperty.WEBDRIVER_REMOTE_OS.From(environmentVariables) != null)
                {
                    buildProperties.Add("Remote OS", ThucydidesSystemProperty.WEBDRIVER_REMOTE_OS.From(environmentVariables));
                }
            }
        }

        private void addSaucelabsPropertiesTo(Dictionary<string, string> buildProperties)
        {
            if (ThucydidesSystemProperty.SAUCELABS_URL.isDefinedIn(environmentVariables))
            {
                buildProperties.Add("Saucelabs URL", maskAPIKey(ThucydidesSystemProperty.SAUCELABS_URL.From(environmentVariables)));
                if (ThucydidesSystemProperty.SAUCELABS_USER_ID.From(environmentVariables) != null)
                {
                    buildProperties.Add("Saucelabs user", ThucydidesSystemProperty.SAUCELABS_USER_ID.From(environmentVariables));
                }
                if (ThucydidesSystemProperty.SAUCELABS_TARGET_PLATFORM.From(environmentVariables) != null)
                {
                    buildProperties.Add("Saucelabs target platform", ThucydidesSystemProperty.SAUCELABS_TARGET_PLATFORM.From(environmentVariables));
                }
                if (ThucydidesSystemProperty.SAUCELABS_DRIVER_VERSION.From(environmentVariables) != null)
                {
                    buildProperties.Add("Saucelabs driver version", ThucydidesSystemProperty.SAUCELABS_DRIVER_VERSION.From(environmentVariables));
                }
                if (ThucydidesSystemProperty.WEBDRIVER_REMOTE_OS.From(environmentVariables) != null)
                {
                    buildProperties.Add("Remote OS", ThucydidesSystemProperty.WEBDRIVER_REMOTE_OS.From(environmentVariables));
                }
            }
        }

        private string maskAPIKey(string url)
        {
            int apiKeyStart = url.IndexOf(":");
            int apiKeyEnd = url.IndexOf("@");
            return url.Substring(0, apiKeyStart + 3) + "XXXXXXXXXXXXXXXX" + url.Substring(apiKeyEnd);
        }

        private void addCustomPropertiesTo(Dictionary<string, string> buildProperties)
        {

            var sysInfoKeys = sysInfoKeysIn(environmentVariables.getKeys());
            foreach (var key in sysInfoKeys)
            {
                string simplifiedKey = key.Replace("sysinfo.", "");
                string expression = environmentVariables.getProperty(key);

                string value = (isGroovyExpression(expression)) ? evaluateGroovyExpression(key, expression) : expression;

                buildProperties.Add(humanizedFormOf(simplifiedKey), value);
            }
        }

        private IEnumerable<string> sysInfoKeysIn(IEnumerable<string> keys)
        {
            return keys.Where(x => x.StartsWith("sysinfo."));

        }

        private bool isGroovyExpression(string expression)
        {
            return expression.StartsWith("${") && expression.EndsWith("}");
        }

        private string humanizedFormOf(string simplifiedKey)
        {
            return StringUtils.Capitalize(simplifiedKey.Replace(".", " "));
        }

        private string evaluateGroovyExpression(string key, string expression)
        {
            throw new NotImplementedException("Grooby not supported");
            /*
        Binding binding = new Binding();
        binding.setVariable("env", environmentVariables);
        GroovyShell shell = new GroovyShell(binding);
        Object result = null;
        try
        {
            string groovy = expression.substring(2, expression.length() - 1);
            if (StringUtils.isNotEmpty(groovy))
            {
                result = shell.evaluate(groovy);
            }
        }
        catch (GroovyRuntimeException e)
        {
            LOGGER.warn("Failed to evaluate build info expression '{0}' for key {1}", expression, key);
        }
        return (result != null) ? result.toString() : expression;
        */

        }
    }
}
