using System;
using OpenQA.Selenium;

namespace SerenityBDD.Core.Steps
{

    public class FileToUpload
    {
        private IWebDriver driver;
        private string filename;

        public FileToUpload(IWebDriver driver, string filename)
        {
            this.driver = driver;
            this.filename = filename;
        }

        public FileToUpload useRemoteDriver(bool isDefinedRemoteUrl)
        {
            // TODO: Find out what this is!
            throw new NotImplementedException();
        }
    }
}
    


