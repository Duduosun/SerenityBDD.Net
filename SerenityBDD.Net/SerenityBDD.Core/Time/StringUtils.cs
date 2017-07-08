using System;
using System.Globalization;
using System.Linq;

namespace SerenityBDD.Core.Time
{
    public static class StringUtils
    {
        public static bool isNotEmpty(string src)
        {
            return !string.IsNullOrEmpty(src);
        }

        public static bool isEmpty(string src)
        {
            return string.IsNullOrEmpty(src);
        }

        public static string Capitalize(string txt, CultureInfo cultureInfo=null)
        {
            if (cultureInfo == null) cultureInfo = new CultureInfo("en-GB", false);
            TextInfo textInfo = cultureInfo.TextInfo;
            return  textInfo.ToTitleCase(txt);
        
        }

        public static bool isNumeric(string s)
        {
            return s.All(ch => Char.IsNumber(ch) || ch == '.' || ch == '-');
        }
    }

   

}