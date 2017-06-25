using System;

namespace SerenityBDD.Core.Steps
{
    internal class PageLooksDodgyException : Exception
    {
    
        public PageLooksDodgyException(string message, Exception e):base(message, e)
        {
    
        }
    }
}