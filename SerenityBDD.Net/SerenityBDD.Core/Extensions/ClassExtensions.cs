using System;

namespace SerenityBDD.Core.Extensions
{
    public static class ClassExtensions
    {
        public static bool instanceof(this object src, Type tgt)
        {
            return tgt.IsAssignableFrom(src.GetType());
        }
    }
}