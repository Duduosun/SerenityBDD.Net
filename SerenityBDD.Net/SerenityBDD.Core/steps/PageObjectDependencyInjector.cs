using System;

namespace SerenityBDD.Core.steps
{
    internal class PageObjectDependencyInjector : DependencyInjector
    {
        private readonly Pages _pages;

        public PageObjectDependencyInjector(Pages pages)
        {
            _pages = pages;
            
        }

        public override void injectDependenciesInto(object newStepClass)
        {
            throw new NotImplementedException();
        }
    }
}