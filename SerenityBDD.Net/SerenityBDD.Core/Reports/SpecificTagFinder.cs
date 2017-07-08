using System;
using System.Collections.Generic;

namespace SerenityBDD.Core.Model
{
    internal class SpecificTagFinder
    {
            private readonly TestTag tag;

            public SpecificTagFinder(TestTag tag)
            {
                this.tag = tag;
            }

            public bool In(IEnumerable<TestTag> tags) {
                    foreach (var otherTag in tags) {
                        if ((otherTag != tag) && (otherTag.isAsOrMoreSpecificThan(tag))) {
                            return true;
                        }
                    }
                    return false;
                }
}

}