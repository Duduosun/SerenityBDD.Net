using System;
using System.Reflection;

namespace SerenityBDD.Core.Steps
{
    public class UnableToInvokeWhenPageOpensMethods : Exception
    {
        public MethodInfo Method { get; }

        public UnableToInvokeWhenPageOpensMethods(MethodInfo method, Exception exception) : this(
            "Could not execute @WhenPageOpens annotated method: " + exception.Message, method, exception)
        {

        }

        protected UnableToInvokeWhenPageOpensMethods(string message, MethodInfo methodInfo, Exception exception) : base(message, exception)
        {
            this.Method = methodInfo;
        }
    }
}