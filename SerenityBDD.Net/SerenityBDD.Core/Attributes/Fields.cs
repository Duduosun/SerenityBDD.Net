using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SerenityBDD.Core.Steps
{
    public class Fields
    {
        private Type _type;
        private Fields(Type type)
        {
            _type = type;
            
        }

        public static Fields of(Type src)
        {
            return new Fields(src);
        }

        public IEnumerable<FieldInfo> allFields()
        {
            return _type.GetFields(BindingFlags.Instance);
        }

        public Optional<FieldInfo> withName(string name)
        {
            return new Optional<FieldInfo>( allFields().FirstOrDefault(x => x.Name == name));
        }
    }
}