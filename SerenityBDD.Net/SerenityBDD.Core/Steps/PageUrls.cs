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
    }
}