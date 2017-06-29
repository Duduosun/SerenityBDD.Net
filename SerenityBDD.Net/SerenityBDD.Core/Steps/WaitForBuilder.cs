using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.Steps
{
    public class WaitForBuilder
    {
        private int duration;
        private SystemClock _clock;
        private PageObject _parent;

        public WaitForBuilder(int duration, PageObject _parent, SystemClock _clock)
        {
            this.duration = duration;
            this._parent = _parent;
            this._clock = _clock;
        }
    }
}