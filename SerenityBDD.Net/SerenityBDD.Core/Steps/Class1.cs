using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    class Class1
    {
    }

    
    public class FieldEntry
    {

        private string value;

        public FieldEntry(string value)
        {
            this.value = value;
        }

        public void into(IWebElement field)
        {
            element(field).type(value);
        }

        public void into(WebElementFacade field
        )
        {
            field.type(value);
        }

        public void intoField(By bySelector)
        {
            IWebElement field = getDriver().findElement(bySelector);
            into(field);
        }
    }
}
