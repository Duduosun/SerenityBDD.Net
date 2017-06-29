using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.Steps
{
    public class PageObjectStepDelayer : PageObject
    {
        private readonly SystemClock _clock;
        private readonly PageObject _parent;

        public PageObjectStepDelayer(SystemClock clock, PageObject parent)
        {
            _clock = clock;
            _parent = parent;
            
        }

        public WaitForBuilder waitFor(int duration)
        {
            return new WaitForBuilder(duration, _parent, _clock);
        }
    }
}