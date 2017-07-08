namespace SerenityBDD.Core.Steps
{
    public  class ConstructionStrategy
    {
        public static ConstructionStrategy STEP_LIBRARY_WITH_WEBDRIVER {  get {  return new ConstructionStrategy(); } }
        public static ConstructionStrategy STEP_LIBRARY_WITH_PAGES { get { return new ConstructionStrategy(); } }
        public static ConstructionStrategy CONSTRUCTOR_WITH_PARAMETERS { get { return new ConstructionStrategy(); } }
        
        public bool equals(ConstructionStrategy target)
        {
            return target.Equals(this );
        }
    }
}