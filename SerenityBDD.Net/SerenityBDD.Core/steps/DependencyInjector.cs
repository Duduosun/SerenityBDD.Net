namespace SerenityBDD.Core.Steps
{
    interface DependencyInjector 
    {
        void injectDependenciesInto(object newStepClass);
    }
}