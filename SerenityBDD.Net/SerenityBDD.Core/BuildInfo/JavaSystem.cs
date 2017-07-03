using System;
using System.Collections.Generic;

namespace SerenityBDD.Core.BuildInfo
{
    public static class JavaSystem
    {
        private static Dictionary<string, string> _properties = null;

        static JavaSystem()
        {
            initKeyValues();
        }

        private static void initKeyValues()
        {
            _properties = new Dictionary<string, string>();
            _properties.Add("os.name", Environment.OSVersion.Platform.ToString());
            _properties.Add("os.version", Environment.OSVersion.VersionString);
        }

        public static string getProperty(string key)
        {
            return _properties[key];
        }
    }
}