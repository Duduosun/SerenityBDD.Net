using System.Collections.Generic;

namespace SerenityBDD.Core.Steps
{
    public interface IDependencyInjectorService {
        IEnumerable<IDependencyInjector> findDependencyInjectors();
    }
}