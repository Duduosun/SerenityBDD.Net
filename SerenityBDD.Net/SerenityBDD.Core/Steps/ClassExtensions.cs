using System;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public static class ClassExtensions
    {
        public static bool instanceof(this object src, Type tgt)
        {
            return tgt.IsAssignableFrom(src.GetType());
        }
    }

    public static class WebElementExtensions
    {
        public static bool shouldNotContainText(this IWebElement src, string txt)
        {
            throw new NotImplementedException();
        }

        public static bool shouldContainText(this IWebElement src, string txt)
        {
            throw new NotImplementedException();
        }
        public static bool containsText(this IWebElement src, string txt)
        {
            throw new NotImplementedException();
        }
        
    }
}