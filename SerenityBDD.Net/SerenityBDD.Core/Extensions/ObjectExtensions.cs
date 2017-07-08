namespace SerenityBDD.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static T Or<T>(this T src, T alternate)
        {
            if (src == null) return alternate;
            return src;

        }

    }
}