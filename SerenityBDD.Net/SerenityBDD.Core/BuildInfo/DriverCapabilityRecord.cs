using System;
using System.Collections.Generic;
using System.Reflection;
using SerenityBDD.Core.Time;
using SerenityBDD.Core.Webdriver;

namespace SerenityBDD.Core.BuildInfo
{
    public interface DriverCapabilityRecord
    {
        IEnumerable<string> getDrivers();
        Dictionary<string, Properties > getDriverCapabilities();
    }
}