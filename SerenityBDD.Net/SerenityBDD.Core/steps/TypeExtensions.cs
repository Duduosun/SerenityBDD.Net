using System;
using System.Linq;
using System.Reflection;

namespace SerenityBDD.Core.steps
{
    public static class TypeExtensions
    {
        public static FieldInfo getPagesField(this Type src)
        {
            return
                src.GetFields(BindingFlags.Instance)
                    .FirstOrDefault(x => x.FieldType.IsAssignableFrom(typeof(Pages)));
        }
        public static bool hasAPagesField(this Type src)
        {
            return src.getPagesField() != null;

        }

        public static ConstructorInfo getPagesConstructor(this Type src)
        {
            return src.GetConstructor(new[] {typeof(Pages)});
        }
        public static bool hasAPagesConstructor(this Type src)
        {
            return src.getPagesConstructor() != null;

        }
    }
}