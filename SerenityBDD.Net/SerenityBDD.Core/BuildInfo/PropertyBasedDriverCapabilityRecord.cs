using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SerenityBDD.Core.Configuration;
using SerenityBDD.Core.Extensions;
using SerenityBDD.Core.Steps;
using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.BuildInfo
{
    public class PropertyBasedDriverCapabilityRecord: DriverCapabilityRecord
    {
        private readonly IConfiguration _configuration;
        private ILog LOGGER = LogManager.GetLogger(typeof(PropertyBasedDriverCapabilityRecord));

        private Dictionary<string, object> getCapabilityList()
        {
            return DesiredCapabilities.Firefox().ToDictionary();

        }
        public PropertyBasedDriverCapabilityRecord(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void registerCapabilities(string driver, ICapabilities capabilities)
        {
            
            Properties properties = new Properties();
            properties.setProperty("platform", capabilities.Platform.PlatformType.ToString());
            
            foreach (var capability in getCapabilityList())
            {
                var value = capabilities.GetCapability(capability.Key);
                if(value.instanceof(typeof(string)))
                    properties.setProperty(capability.Key, value);
            }
            
            LOGGER.Warn("Failed to store browser configuration for " + capabilities);
        
        }

    
        public IEnumerable<string> getDrivers()
        {
            return driverCapabilityRecords().Select(filePath => driverNameFrom(filePath));
    
        }

        private string driverNameFrom(string filePath)
        {
            return Path.GetFileName(filePath).Replace("browser-", "").Replace(".properties", "");
        }

        private IConfiguration Configuration
        {
            get {  return _configuration;}
        }
        private IEnumerable<string> driverCapabilityRecords()
        {
            var outputDirectory = Configuration.OutputDirectory;
            return Directory.EnumerateFiles(outputDirectory, "browser-*.properties");
        }


        public Dictionary<string, Properties> getDriverCapabilities()
        {
            Dictionary<string, Properties> driverCapabilities = new Dictionary<string, Properties>();

            foreach (var dcr in driverCapabilityRecords())
            {
                var driverName = driverNameFrom(dcr);
                var driverProperties = new Properties();

                try
                {
                    driverProperties.Load(dcr);
                    driverCapabilities.Add(driverName, driverProperties);
                }
                catch (IOException e)
                {
                    LOGGER.Error($"Failed to load properties for driver {dcr}", e);
                }
            }


            return driverCapabilities;
        }

    }
}