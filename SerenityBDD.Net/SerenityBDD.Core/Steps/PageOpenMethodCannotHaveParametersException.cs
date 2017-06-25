using System.Reflection;

namespace SerenityBDD.Core.Steps
{
    public class PageOpenMethodCannotHaveParametersException : UnableToInvokeWhenPageOpensMethods
    {

        public PageOpenMethodCannotHaveParametersException(MethodInfo method) : base("Methods marked with PageOpen cannot have parameters", method, null)
        {

        }


    }
}