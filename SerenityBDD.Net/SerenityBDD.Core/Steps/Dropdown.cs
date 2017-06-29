using System;
using System.Collections.Generic;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public class Dropdown
    {
        public static Dropdown ForWebElement(IWebElement dropdown)
        {
            throw new NotImplementedException();
        }

        public void Select(string visibleLabel)
        {
            throw new NotImplementedException();
        }

        public void selectMultipleItems(string[] selectedLabels)
        {
            throw new NotImplementedException();
        }

        public ISet<string> getSelectedOptionLabels()
        {
            throw new NotImplementedException();
        }

        public ISet<string> getSelectedOptionValues()
        {
            throw new NotImplementedException();
        }

        public string getSelectedValue()
        {
            throw new NotImplementedException();
        }

        public string getSelectedLabel()
        {
            throw new NotImplementedException();
        }
    }
}