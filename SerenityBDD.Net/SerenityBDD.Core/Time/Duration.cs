using System;

namespace SerenityBDD.Core.Time
{
    public class Duration
    {
        public double In(TimeUnit units)
        {
            switch (units)
            {
                case TimeUnit.MILLISECONDS:
                    return this.TimeSpan.TotalMilliseconds;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(units), units, null);
            }
        }

        public static implicit operator TimeSpan(Duration src)
        {
            return src.TimeSpan;
        }

        public static implicit operator Duration(TimeSpan src)
        {
            return new Duration(src);
        }
        public Duration(TimeSpan timespan)
        {
            this.TimeSpan = timespan;
        }

        public Duration(long duration, TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.MILLISECONDS:
                    this.TimeSpan = TimeSpan.FromMilliseconds(duration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public TimeSpan TimeSpan { get; set; }

    }
}