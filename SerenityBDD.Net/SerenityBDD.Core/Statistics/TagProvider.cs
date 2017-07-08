using System;
using System.Collections.Generic;

namespace SerenityBDD.Core.Model
{
    public interface TagProvider
    {
        IEnumerable<TestTag> getTagsFor(TestOutcome testOutcome);
    }
}