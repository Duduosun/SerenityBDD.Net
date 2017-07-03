using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using SerenityBDD.Core.Steps;

namespace SerenityBDD.Core.Time
{
    public class PropertyBase
    {

        private string propertyName;
        public static int DEFAULT_HEIGHT = 700;
        public static int DEFAULT_WIDTH = 960;

        public static string DEFAULT_HISTORY_DIRECTORY = "history";


        private ILog logger = LogManager.GetLogger(typeof(PropertyBase));

        public PropertyBase(string propertyName)
        {
            this.propertyName = propertyName.Replace("_", ".").ToLowerInvariant();
        }

        public static PropertyBase<T> create<T>(string propertyName)
        {
            return new PropertyBase<T>(propertyName);
        }
        public static PropertyBase<T> withDefault<T>(string propertyName, T defaultValue)
        {
            return new PropertyBase<T>(propertyName, defaultValue);
        }


        public string getPropertyName()
        {
            return propertyName;
        }

        public override string ToString()
        {
            return propertyName;
        }

        public string From(EnvironmentVariables environmentVariables)
        {
            return From(environmentVariables, null);
        }

        private Optional<string> legacyPropertyValueIfPresentIn(EnvironmentVariables environmentVariables)
        {
            string legacyValue = environmentVariables.getProperty(withLegacyPrefix(getPropertyName()));
            if (StringUtils.isNotEmpty(legacyValue))
            {
                logger.WarnFormat("Legacy property format detected for {0}, please use the serenity.* format instead.", getPropertyName());
            }
            return Optional.fromNullable(legacyValue);
        }

        private string withLegacyPrefix(string propertyName)
        {
            return propertyName.Replace("serenity.", "thucydides.");
        }

        private string withSerenityPrefix(string propertyName)
        {
            return propertyName.Replace("thucydides.", "serenity.");
        }

        public string preferredName()
        {
            return withSerenityPrefix(getPropertyName());
        }

        public List<string> legacyNames()
        {
            List<string> names = new[] { withLegacyPrefix(getPropertyName()) }.ToList();

            return names;
        }

        public string From(EnvironmentVariables environmentVariables, string defaultValue)
        {
            Optional<string> newPropertyValue
                    = Optional.fromNullable(environmentVariables.getProperty(withSerenityPrefix(getPropertyName())));

            if (isDefined(newPropertyValue))
            {
                return newPropertyValue.get();
            }
            else
            {
                Optional<string> legacyValue = legacyPropertyValueIfPresentIn(environmentVariables);
                return (isDefined(legacyValue)) ? legacyValue.get() : defaultValue;
            }
        }

        private bool isDefined(Optional<string> newPropertyValue)
        {
            return newPropertyValue.isPresent() && StringUtils.isNotEmpty(newPropertyValue.get());
        }

        public int integerFrom(EnvironmentVariables environmentVariables)
        {
            return integerFrom(environmentVariables, 0);
        }

        public int integerFrom(EnvironmentVariables environmentVariables, int defaultValue)
        {
            Optional<string> newPropertyValue
                    = Optional.fromNullable(environmentVariables.getProperty(withSerenityPrefix(getPropertyName())));

            if (isDefined(newPropertyValue))
            {
                return int.Parse(newPropertyValue.get());
            }
            else
            {
                Optional<string> legacyValue = legacyPropertyValueIfPresentIn(environmentVariables);
                return (isDefined(legacyValue)) ? int.Parse(legacyValue.get()) : defaultValue;
            }
        }

        public bool booleanFrom(EnvironmentVariables environmentVariables)
        {
            return booleanFrom(environmentVariables, false);
        }

        public bool booleanFrom(EnvironmentVariables environmentVariables, bool defaultValue)
        {
            if (environmentVariables == null) { return defaultValue; }

            Optional<string> newPropertyValue
                    = Optional.fromNullable(environmentVariables.getProperty(withSerenityPrefix(getPropertyName())));

            if (isDefined(newPropertyValue))
            {
                return bool.Parse(newPropertyValue.get());
            }
            else
            {
                Optional<string> legacyValue = legacyPropertyValueIfPresentIn(environmentVariables);
                return (isDefined(legacyValue)) ? bool.Parse(legacyValue.get()) : defaultValue;
            }
        }

        public bool isDefinedIn(EnvironmentVariables environmentVariables)
        {
            return StringUtils.isNotEmpty(From(environmentVariables));
        }

      
    }

    public class PropertyBase<T> : PropertyBase
    {
        public T Value { get; set; }

        public PropertyBase(string propertyName) : base(propertyName)
        {
        }

        public PropertyBase(string propertyName, T defaultValue) : base(propertyName)
        {
            this.Value = defaultValue;
        }
        public static implicit operator T(PropertyBase<T> src)
        {
            return (T)src.Value;
        }

        public static implicit operator PropertyBase<T>(T src)
        {
            return new PropertyBase<T>(nameof(src), src);
        }

    }
}