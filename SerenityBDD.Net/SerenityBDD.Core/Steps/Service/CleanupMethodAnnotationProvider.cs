using System;
using System.Collections.Generic;

namespace SerenityBDD.Core.Steps
{
    public interface CleanupMethodAnnotationProvider
    {
        IEnumerable<string> getCleanupMethodAnnotations();
    }
}