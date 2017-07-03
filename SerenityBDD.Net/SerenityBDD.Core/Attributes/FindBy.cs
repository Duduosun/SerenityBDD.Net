using System;
using SerenityBDD.Core.Steps;

namespace SerenityBDD.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class FindBy : Attribute
    {

        public How How { get; set; } = How.ID;

        public string Using { get; set; } = string.Empty;

        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string NgModel { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;

        public string Css { get; set; } = string.Empty;

        public string TagName { get; set; } = string.Empty;

        public string LinkText { get; set; } = string.Empty;

        public string PartialLinkText { get; set; } = string.Empty;

        public string Xpath { get; set; } = string.Empty;

        public string Jquery { get; set; } = string.Empty;

        public string Sclocator { get; set; } = string.Empty;

        public string AndroidUIAutomator { get; set; } = string.Empty;

        public string IOSUIAutomation { get; set; } = string.Empty;

        public string AccessibilityId { get; set; } = string.Empty;

        public int? TimeoutInSeconds { get; set; } 

    }
}