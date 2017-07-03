using System;

namespace SerenityBDD.Core.Attributes
{
    public class ImplementedBy : Attribute
        
    {
        public Type WebElementFacadeType { get; set; }
    }
}