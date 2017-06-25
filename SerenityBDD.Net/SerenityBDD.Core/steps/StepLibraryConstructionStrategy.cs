using System;

namespace SerenityBDD.Core.Steps
{
    public class StepLibraryConstructionStrategy
    {
        private readonly Type stepClass;

        public StepLibraryConstructionStrategy(Type stepClass)
        {
            this.stepClass = stepClass;
        }
       

        public ConstructionStrategy getStrategy()
        {
            throw new NotImplementedException();
        }
        public static StepLibraryConstructionStrategy forClass(Type stepClass)
        {
            return new StepLibraryConstructionStrategy(stepClass);
        }
    }
}