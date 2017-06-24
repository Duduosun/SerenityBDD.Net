using System;

namespace SerenityBDD.Core.Steps
{
    internal class InvocationTargetException : Exception
    {
        public Exception getTargetException()
        {
            return base.InnerException;
        }
    }
}