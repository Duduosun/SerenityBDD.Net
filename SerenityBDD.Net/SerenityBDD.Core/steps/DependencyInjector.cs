namespace SerenityBDD.Core.steps
{
    internal abstract class DependencyInjector : IDependencyInjector
    {
        public abstract void injectDependenciesInto(object newStepClass);
    }
}