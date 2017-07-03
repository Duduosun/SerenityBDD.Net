using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerenityBDD.Core.DI
{
    public interface DependencyInjector
    {
        void injectDependenciesInto(object newStepClass);
    }
}
