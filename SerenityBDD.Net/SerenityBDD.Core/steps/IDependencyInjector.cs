namespace SerenityBDD.Core.steps
{
    public interface IDependencyInjector {
        void injectDependenciesInto(object newStepClass);
    }
}