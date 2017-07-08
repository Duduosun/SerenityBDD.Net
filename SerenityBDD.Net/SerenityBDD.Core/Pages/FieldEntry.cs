using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public class FieldEntry : PageObject
    {

        private string _value;

        public FieldEntry(string value)
        {
            this._value = value;
        }

        public void Into(IWebElement field)
        {
            Element(field).type(_value);
        }

        public void Into(WebElementFacade field
        )
        {
            field.type(_value);
        }

        public void IntoField(By bySelector)
        {
            IWebElement field = GetDriver().FindElement(bySelector);
            Into(field);
        }
    }
}
