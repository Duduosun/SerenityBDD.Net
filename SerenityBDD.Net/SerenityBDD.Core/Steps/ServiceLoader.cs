using System;
using System.Collections.Generic;

namespace SerenityBDD.Core.Steps
{
    internal static class ServiceLoader
    {
        public static IEnumerable<CleanupMethodAnnotationProvider> load<T>()
            where T : CleanupMethodAnnotationProvider
        {
            throw new NotImplementedException();
        }
    }
}