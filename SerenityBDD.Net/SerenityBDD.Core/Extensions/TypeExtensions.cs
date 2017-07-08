using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SerenityBDD.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool equalsIgnoreCase(this Type tgt, string name)
        {
            return tgt.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }
        public static FieldInfo getPagesField(this Type src)
        {
            return
                src.getPagesFields().FirstOrDefault();

        }

        public static IEnumerable<FieldInfo> getPagesFields(this Type src)
        {
            return
                src.GetFields(BindingFlags.Instance)
                    .Where(x => x.FieldType.IsAssignableFrom(typeof(Steps.Pages)));
        }
        public static bool hasAPagesField(this Type src)
        {
            return src.getPagesField() != null;

        }

        public static ConstructorInfo getPagesConstructor(this Type src)
        {
            return src.GetConstructor(new[] { typeof(Steps.Pages) });
        }
        public static bool hasAPagesConstructor(this Type src)
        {
            return src.getPagesConstructor() != null;

        }
    }
}