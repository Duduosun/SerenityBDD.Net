using System;

namespace SerenityBDD.Core.External
{
    internal class Class
    {
        public static object forName(string className)
        {
            var t = Type.GetType(className);
            return Activator.CreateInstance(t);
        }
    }
}