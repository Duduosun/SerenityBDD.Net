using System;
using System.Drawing.Design;
using System.IO;
using System.Net.Http;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{
    public class Optional<T> : Optional
        where T : class
    {
    
        public static implicit operator T(Optional<T> myinstance)
        {
            return myinstance.get();
        }

        public T get()
        {
            return (T) _value;
        }

        public Optional(T value) : base(value)
        {

        }

    }

    public class Pages
    {
        public PageObject getPage(Type pageObjectClass)
        {
            throw new NotImplementedException();
        }

        public IWebDriver getDriver()
        {
            throw new NotImplementedException();
        }

        public void setDriver(IWebDriver driver)
        {
            throw new NotImplementedException();
        }
    }
}