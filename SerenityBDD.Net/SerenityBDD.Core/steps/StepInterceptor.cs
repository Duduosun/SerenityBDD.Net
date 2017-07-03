using System;
using System.Collections.Generic;
using log4net;
using SerenityBDD.Core.Webdriver;

namespace SerenityBDD.Core.Steps
{
    /// <summary>
    /// interceptor, but should probably use castle.windsor instead!
    /// </summary>
    internal class StepInterceptor : MethodInterceptor
    {

        private readonly Type _testStepClass;
        private readonly ILog _log = LogManager.GetLogger(typeof(StepInterceptor));
        private readonly EnvironmentVariables _environmentVariables;
        private readonly List<string> cleanupMethodsAnnotations = new List<string>();

        public StepInterceptor(Type testStepClass)
        {
            _testStepClass = testStepClass;
            _environmentVariables = ConfiguredEnvironment.getEnvironmentVariables();

            IEnumerable<CleanupMethodAnnotationProvider> cleanupMethodAnnotationProviders =
                ServiceLoader.load<CleanupMethodAnnotationProvider>();
            foreach (var cleanupMethodAnnotationProvider in cleanupMethodAnnotationProviders)
            {
                cleanupMethodsAnnotations.AddRange(cleanupMethodAnnotationProvider.getCleanupMethodAnnotations());
            }
        }

    }

    internal class CleanupMethodAnnotationProvider
    {
        public IEnumerable<string> getCleanupMethodAnnotations()
        {
            throw new NotImplementedException();
        }
    }

    internal static class ServiceLoader
    {
        public static IEnumerable<CleanupMethodAnnotationProvider> load<T>()
            where T : CleanupMethodAnnotationProvider
        {
            throw new NotImplementedException();
        }
    }

    internal static class ConfiguredEnvironment 
    {
        public static EnvironmentVariables getEnvironmentVariables()
        {
            throw new NotImplementedException();
        }

        public static Configuration getConfiguration()
        {
            throw new NotImplementedException();
        }
    }

    public class EnvironmentVariables
    {
        public string getProperty(string withSerenityPrefix)
        {
            throw new NotImplementedException();
        }
    }
}