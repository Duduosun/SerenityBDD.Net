using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public class RadioButtonGroup
    {
        private ReadOnlyCollection<IWebElement> readOnlyCollection;

        public RadioButtonGroup(ReadOnlyCollection<IWebElement> readOnlyCollection)
        {
            this.readOnlyCollection = readOnlyCollection;
        }
    }
}