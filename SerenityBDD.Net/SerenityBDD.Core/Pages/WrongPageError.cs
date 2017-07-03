using System;

namespace SerenityBDD.Core.Steps
{
    public class WrongPageError : Exception
    {
        public WrongPageError(string message):base(message)
        {
            
        }
    }
}