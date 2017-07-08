using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SerenityBDD.Core.DI;
using SerenityBDD.Core.Injectors;
using SerenityBDD.Core.Steps;

namespace SerenityBDD.Core.Pages.Injectors
{
    internal class PageObjectDependencyInjector : DependencyInjector
    {
        private readonly Steps.Pages _pages;
        private EnvironmentDependencyInjector _environmentDependencyInjector;

        public PageObjectDependencyInjector(Steps.Pages pages)
        {
            _pages = pages;
            _environmentDependencyInjector = new EnvironmentDependencyInjector();

        }

        public void injectDependenciesInto(object target)
        {
            _environmentDependencyInjector.injectDependenciesInto(target);
            var fields = target.GetType().getPagesFields();
            updatePageObject(target, _pages);

            foreach (var field in nonAbstractFields(fields))
            {
                instantiatePageObjectIfNotAssigned(field, target);
            }
        }

        private IEnumerable<FieldInfo> nonAbstractFields(IEnumerable<FieldInfo> fields)
        {
            return fields.Where(x => !x.FieldType.IsAbstract);
        }
        private void instantiatePageObjectIfNotAssigned(FieldInfo pageObjectField, object target)
        {

            if (pageObjectField.GetValue(target) == null)
            {
                Type pageObjectClass = pageObjectField.FieldType;
                PageObject newPageObject = _pages.getPage(pageObjectClass);
                injectDependenciesInto(newPageObject);
                pageObjectField.SetValue(target, newPageObject);
            }
            else
            {
                updatePageObject(pageObjectField.GetValue(target), _pages);
            }

        }

        private void updatePageObject(object target, Steps.Pages pages)
        {
            var po = target as PageObject;
            if(po!=null) { 
                po.SetPages(pages);
                po.SetDriver(pages.getDriver());
            }
        }
    }
}