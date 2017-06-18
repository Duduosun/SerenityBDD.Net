using System.Collections.Generic;

namespace SerenityBDD.Core.steps
{
    public interface IDependencyInjectorService {
        IEnumerable<IDependencyInjector> findDependencyInjectors();
    }
}