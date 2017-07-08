using System;
using System.Collections.Generic;
using log4net;
using SerenityBDD.Core.Environment;
using SerenityBDD.Core.External;

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
}