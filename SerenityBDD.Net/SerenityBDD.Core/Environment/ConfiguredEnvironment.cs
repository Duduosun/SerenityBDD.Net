using System;
using System.Threading;
using SerenityBDD.Core.Configuration;
using SerenityBDD.Core.Steps;
using SerenityBDD.Core.Time;
using SerenityBDD.Core.Webdriver;

namespace SerenityBDD.Core.Environment
{
    internal static class ConfiguredEnvironment
    {
        private static readonly ThreadLocal<EnvironmentVariables> testEnvironmentVariables = new ThreadLocal<EnvironmentVariables>();
        private static readonly ThreadLocal<IConfiguration> testConfiguration = new ThreadLocal<IConfiguration>();

        public static void setTestEnvironmentVariables(EnvironmentVariables testEnvironment)
        {
            testEnvironmentVariables.Value = testEnvironment;
            testConfiguration.Value = new SystemPropertiesConfiguration(testEnvironment);
        }

        public static EnvironmentVariables getEnvironmentVariables()
        {
            if (testEnvironmentVariables.Value != null)
            {
                return testEnvironmentVariables.Value;
            }
            return Injectors.getInjector().getInstance<EnvironmentVariables>();
        }

        public static IConfiguration getConfiguration()
        {
            if (testConfiguration.Value != null)
            {
                return testConfiguration.Value;
            }
            return Injectors.getInjector().getInstance<IConfiguration>();
        }

        public static void reset()
        {
            testEnvironmentVariables.Dispose();
            testConfiguration.Dispose();
        }
    }
}