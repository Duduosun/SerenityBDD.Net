using System.Collections.Generic;
using System.IO;
using CsvHelper;

namespace SerenityBDD.Core.Time
{
    public class JavaPropertiesFileReader
    {
        public void Load(string filePath, Properties target)
        {
            // load the properties as name/value pairs from the given file
            using (var fs = File.OpenRead(filePath))
            using (var ts = new StreamReader(fs))
            {

                var csv = new CsvReader(ts);
                csv.Configuration.Delimiter = "=";
                csv.Configuration.IgnoreBlankLines = true;
                csv.Configuration.IgnoreHeaderWhiteSpace = true;

                
                while (csv.Read())
                {
                    var key = csv.CurrentRecord[0];
                    var value = csv.CurrentRecord[1];

                    target.Add(key, PropertyBase.withDefault(key, value));
                }
                
            }

            
        }
    }
    public class Properties
    {
        private Dictionary<string, PropertyBase> _properties;

        public Properties()
        {
            _properties = new Dictionary<string, PropertyBase>();
        }

        public Properties(Dictionary<string, PropertyBase> properties)
        {
            _properties = properties;
        }

        public void setProperty<T>(string name, T value)
        {
            _properties.Add(name, PropertyBase.withDefault(name, value ));
        }

        public void Load(string filePath )
        {
            _properties = new Dictionary<string, PropertyBase>();
          new JavaPropertiesFileReader().Load(filePath, this );
            
        }

        public void Add(string key, PropertyBase value)
        {
            _properties.Add(key, value);
        }
    }
}