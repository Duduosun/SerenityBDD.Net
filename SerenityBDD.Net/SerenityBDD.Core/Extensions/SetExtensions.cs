using System.Collections.Generic;

namespace SerenityBDD.Core.Extensions
{
    public static class SetExtensions
    {
        public static void AddAll<T>(this ISet<T> tgt, IEnumerable<T> src)
        {
            foreach (var s in src)
            {
                tgt.Add(s);
            }

        }
    }
}