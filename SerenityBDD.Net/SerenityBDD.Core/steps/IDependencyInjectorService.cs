using System.Collections.Generic;

namespace SerenityBDD.Core.Steps
{
    public interface IDependencyInjectorService {
        IEnumerable<IDependencyInjector> findDependencyInjectors();
    }

    public interface IDependencyInjector
    {
        void injectDependenciesInto(object target);
    }
}