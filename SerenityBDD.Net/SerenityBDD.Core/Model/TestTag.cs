using System;

namespace SerenityBDD.Core.Model
{
    public class TestTag
    {
        public static TestTag withName(string manual)
        {
            throw new NotImplementedException();
        }

        public TestTag andType(string externalTests)
        {
            throw new NotImplementedException();
        }

        internal string getName()
        {
            throw new NotImplementedException();
        }

        public bool isAsOrMoreSpecificThan(TestTag tag)
        {
            throw new NotImplementedException();
        }
    }
}