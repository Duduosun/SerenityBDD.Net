using System;

namespace SerenityBDD.Core.Steps
{
    public class PageUrls
    {
        private PageObject pageObject;

        public PageUrls(PageObject pageObject)
        {
            this.pageObject = pageObject;
        }

        public void overrideDefaultBaseUrl(string defaultBaseUrl)
        {
            throw new NotImplementedException();
        }

        public string getSystemBaseUrl()
        {
            throw new NotImplementedException();
        }

        public string getStartingUrl(string[] parameterValues)
        {
            throw new NotImplementedException();
        }

        public string getNamedUrl(string urlTemplateName, string[] parameterValues)
        {
            throw new NotImplementedException();
        }

        internal string getStartingUrl()
        {
            throw new NotImplementedException();
        }
    }
}