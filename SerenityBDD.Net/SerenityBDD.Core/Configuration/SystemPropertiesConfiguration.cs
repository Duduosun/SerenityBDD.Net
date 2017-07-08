using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SerenityBDD.Core.Steps;

namespace SerenityBDD.Core.Configuration
{
    class SystemPropertiesConfiguration : IConfiguration
    {
        public SystemPropertiesConfiguration(EnvironmentVariables environmentVariables)
        {
            throw new NotImplementedException();
        }

        public string getBaseUrl()
        {
            throw new NotImplementedException();
        }

        public string OutputDirectory { get; }
    }
}
