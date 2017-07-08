using System.Collections.Generic;

namespace SerenityBDD.Core.DI
{
    public interface IDependencyInjectorService {
        IEnumerable<IDependencyInjector> findDependencyInjectors();
    }

    public interface IDependencyInjector
    {
        void injectDependenciesInto(object target);
    }
}