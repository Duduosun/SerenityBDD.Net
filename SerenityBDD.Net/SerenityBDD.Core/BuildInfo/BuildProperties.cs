using System.Collections.Generic;
using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.BuildInfo
{
    public class BuildProperties
    {
        private readonly Dictionary<string, string> generalProperties;
        private readonly IEnumerable<string> drivers;
        private readonly Dictionary<string, Properties> driverProperties;

        public BuildProperties(Dictionary<string, string> generalProperties, IEnumerable<string> drivers, Dictionary<string, Properties> driverProperties)
        {
            this.generalProperties = generalProperties;
            this.drivers = drivers;
            this.driverProperties = driverProperties;
        }

        public Dictionary<string, string> getGeneralProperties()
        {
            return new Dictionary<string, string>(this.generalProperties);
        }

        public IEnumerable<string> getDrivers()
        {
            return drivers;
        }

        public Dictionary<string, Properties> getDriverProperties()
        {
            return new Dictionary<string, Properties>(driverProperties);

        }
    }
}