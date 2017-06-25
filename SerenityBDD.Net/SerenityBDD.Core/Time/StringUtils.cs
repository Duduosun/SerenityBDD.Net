namespace SerenityBDD.Core.Time
{
    public static class StringUtils
    {
        public static bool isNotEmpty(string src)
        {
            return !string.IsNullOrEmpty(src);
        }
    }
}