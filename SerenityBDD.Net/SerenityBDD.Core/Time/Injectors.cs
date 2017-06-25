using System;

namespace SerenityBDD.Core.Time
{
    public class Injectors
    {

        private static Injector injector;

        public static Injector getInjector()
        {
            //if (injector == null)
            //{
            //    injector = Guice.createInjector(new ThucydidesModule());
            //}
            //return injector;
            throw new NotImplementedException("Some injection container should be here!");
        }

    }
}