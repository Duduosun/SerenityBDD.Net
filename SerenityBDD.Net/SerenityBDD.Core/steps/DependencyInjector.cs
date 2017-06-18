namespace SerenityBDD.Core.steps
{
    interface DependencyInjector 
    {
        void injectDependenciesInto(object newStepClass);
    }
}