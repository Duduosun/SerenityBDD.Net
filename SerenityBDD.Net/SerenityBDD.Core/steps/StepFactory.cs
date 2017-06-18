using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace SerenityBDD.Core.steps
{
    public class StepFactory : IStepFactory
    {
        private readonly Pages _pages;
        private readonly IDictionary<Type, object> _index = new Dictionary<Type, object>();
        private readonly ILog Logger = LogManager.GetLogger(typeof(StepFactory));
        private readonly IDependencyInjectorService _dependencyInjectorService;

        public StepFactory(Pages pages)
        {
            _pages = pages;
        }

        public StepFactory():this(null)
        {
            
        }

        public object GetUniqueStepLibraryFor(object clazz, object[] constructorParameters)
        {
            throw new NotImplementedException();
        }

        public object GetStepLibraryFor(Type scenarioStepsClass)
        {
            if (isStepLibraryInstantiedFor(scenarioStepsClass))
            {
                return getManagedStepLibraryFor(scenarioStepsClass);
            }
            else
            {
                return getNewStepLibraryFor(scenarioStepsClass);
            }
        }

        private object getNewStepLibraryFor(Type scenarioStepsClass)
        {
            var stepInterceptor = new StepInterceptor(scenarioStepsClass);
            return instantiateNewStepLibraryFor(scenarioStepsClass, stepInterceptor);
        }

        private object instantiateNewStepLibraryFor(Type scenarioStepsClass, StepInterceptor interceptor, params object[] constructorParameters)
        {
            object steps = createProxyStepLibrary(scenarioStepsClass, interceptor, constructorParameters);
            indexStepLibrary(scenarioStepsClass, steps);
            
            instantiateAnyNestedStepLibrariesIn(steps, scenarioStepsClass);

            injectOtherDependenciesInto(steps);

            return steps;
        }
        

        private void injectOtherDependenciesInto(object newStepsClass)
        {
            var dependencyInjectors =new List<IDependencyInjector>( _dependencyInjectorService.findDependencyInjectors());
            dependencyInjectors.AddRange(getDefaultDependencyInjectors());

            foreach (var dependencyInjector in dependencyInjectors)
            {
                dependencyInjector.injectDependenciesInto(newStepsClass);
            }
        }

        private IEnumerable<IDependencyInjector> getDefaultDependencyInjectors()
        {
            if (_pages == null) return new IDependencyInjector[] {new EnvironmentDependencyInjector(),};

            return new IDependencyInjector[]
            {
                new PageObjectDependencyInjector(_pages),
                new EnvironmentDependencyInjector()
            };
        }


        private void instantiateAnyNestedStepLibrariesIn(object steps, Type scenarioStepsClass)
        {
            StepAnnotations.injectNestedScenarioStepsInto(steps, this, scenarioStepsClass);
        }

        private void indexStepLibrary(Type scenarioStepsClass, object steps)
        {
            _index.Add(scenarioStepsClass, steps);

        }


        object  createProxyStepLibrary(Type scenarioStepsClass,
            MethodInterceptor interceptor,
            params object[] parameters)
        {
            Enhancer e = new Enhancer();
            e.setSuperclass(scenarioStepsClass);
            e.setCallback(interceptor);

            var strategy = StepLibraryConstructionStrategy.forClass(scenarioStepsClass).getStrategy();
            if (ConstructionStrategy.STEP_LIBRARY_WITH_WEBDRIVER.equals(strategy))
            {
                return webEnabledStepLibrary(scenarioStepsClass, e);
            }
            else if (ConstructionStrategy.STEP_LIBRARY_WITH_PAGES.equals(strategy))
            {
                return stepLibraryWithPages(scenarioStepsClass, e);
            }
            else if (ConstructionStrategy.CONSTRUCTOR_WITH_PARAMETERS.equals(strategy) && parameters.Length > 0)
            {
                return immutableStepLibrary(scenarioStepsClass, e, parameters);
            }
            else
            {
                return e.create();
            }
        }

        private object immutableStepLibrary(Type scenarioStepsClass, Enhancer e, object[] parameters)
        {
            return e.create(argumentTypesFrom(scenarioStepsClass, parameters), parameters);
        }
        private Type[] argumentTypesFrom(Type scenarioStepsClass, Object[] parameters)
        {

            foreach (var candidateConstructor in inOrderOfIncreasingParameters(scenarioStepsClass.GetConstructors()))
            {
                var parameterTypes = candidateConstructor.GetParameters().Select(x => x.ParameterType).ToArray();

                if (parametersMatchFor(parameters, parameterTypes))
                {
                    return parameterTypes;
                }
            }
            throw new ArgumentOutOfRangeException("Could not find a matching constructor for class " + scenarioStepsClass + "with parameters " + parameters);
        }

        private IEnumerable<ConstructorInfo> inOrderOfIncreasingParameters(ConstructorInfo[] declaredConstructors)
        {
            return declaredConstructors.OrderByDescending(x => x.GetParameters().Length);

        }

        private bool parametersMatchFor(Object[] parameters, Type[] parameterTypes)
        {
            int parameterNumber = 0;
            if (parameters.Length != parameterTypes.Length)
            {
                return false;
            }
            else
            {
                foreach (var parameterType in parameterTypes)
                {

                    if (parameterNumber >= parameterTypes.Length)
                    {
                        return false;
                    }

                    if (parameter(parameters[parameterNumber]).cannotBeAssignedTo(parameterType))
                    {
                        return false;
                    }

                    if ((parameters[parameterNumber] != null)
                        && (!ClassUtils.isAssignable(parameters[parameterNumber].GetType(), parameterType)))
                    {
                        return false;
                    }
                    parameterNumber++;
                }
            }
            return true;
        }

        private ParameterAssignementChecker parameter(Object p)
        {
            return new ParameterAssignementChecker(p);
        }

        private class ParameterAssignementChecker
        {
            private static readonly bool PARAMETER_CAN_BE_ASSIGNED = false;
            private readonly Object parameter;

            public ParameterAssignementChecker(Object parameter)
            {
                this.parameter = parameter;
            }

            public bool cannotBeAssignedTo(Type parameterType)
            {
                if (parameter == null)
                {
                    return PARAMETER_CAN_BE_ASSIGNED;
                }

                return (!ClassUtils.isAssignable(parameter.GetType(), parameterType));
            }
        }

        private Type forTheClassOfParameter(Object p)
        {
            if (p == null)
            {
                return typeof(object);
            }

            return p.GetType();
        }

        private object stepLibraryWithPages(Type scenarioStepsClass, Enhancer e)
        {
            object newStepLibrary = e.create();
            return injectPagesInto(scenarioStepsClass, newStepLibrary);
        }

        private object injectPagesInto(Type scenarioStepsClass, object newStepLibrary)
        {
            return new PageInjector(_pages).injectPagesInto(scenarioStepsClass, newStepLibrary);

        }

        class PageInjector
        {
            private readonly Pages pages;
            private ILog _log = LogManager.GetLogger(typeof(PageInjector));
            public PageInjector(Pages pages)
            {
                this.pages = pages;
            }

            public object injectPagesInto(Type stepLibraryClass, object newStepLibrary)

            {
                var cs = newStepLibrary as ScenarioSteps;

                if (cs !=null )
                {
                    cs.setPages(pages);
                }
                else if (stepLibraryClass.hasAPagesField())
                {
                    var pagesField = stepLibraryClass.getPagesField();
                    
                    try
                    {
                        pagesField.SetValue(newStepLibrary, pages);
                    }
                    catch (Exception e)
                    {
                        _log.Error($"Could not instantiate pages field for step library {newStepLibrary}", e);
                    }
                }
                return newStepLibrary;
            }

        }

      
        private object webEnabledStepLibrary(Type scenarioStepsClass, Enhancer e)
        {
            if (scenarioStepsClass.hasAPagesConstructor())
            {
                return e.create(new[] {typeof(Pages)}, new[] {_pages});
            }
            else
            {
                var newStepLibrary = e.create();
                return injectPagesInto(scenarioStepsClass, newStepLibrary);
                
            }
        }

        private object getManagedStepLibraryFor(Type scenarioStepsClass)
        {
            return _index[scenarioStepsClass];
        }

        private bool isStepLibraryInstantiedFor(Type scenarioStepsClass)
        {
            return _index.ContainsKey(scenarioStepsClass);
        }
    }
}