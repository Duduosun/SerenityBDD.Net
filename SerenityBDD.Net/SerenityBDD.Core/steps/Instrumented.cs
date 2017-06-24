using System;

namespace SerenityBDD.Core.Steps
{
    public class Instrumented
    {
        private static IStepFactory _stepFactory ;

        public static InstrumentedBuilder<T> InstanceOf<T>()
        {
            return new InstrumentedBuilder<T>();
        }

        public  class InstrumentedBuilder<T>
        {
            private readonly Type _clazz;
            private readonly object[] _constructorParameters;

            public InstrumentedBuilder():this(new object[] { })
            {
                _constructorParameters = null;
                _clazz = typeof(T);

            }
            public InstrumentedBuilder(object[] constructorParameters)
            {
                _constructorParameters = constructorParameters;
            }

            public T newInstance()
            {
                return (T)_stepFactory.GetUniqueStepLibraryFor(_clazz, _constructorParameters);
            }

            public T withProperties(params object[] constructorParameters)
            {
                return (T) new InstrumentedBuilder<T>(constructorParameters).newInstance();
            }
        }
    }
}