using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.Steps
{
    public abstract class ThucydidesFluentWait<T> : Wait<T>
    {

        public static readonly Duration FIVE_HUNDRED_MILLIS = TimeSpan.FromMilliseconds(500);

        protected Duration timeout = FIVE_HUNDRED_MILLIS;
        protected Duration interval = FIVE_HUNDRED_MILLIS;

        private List<Type> ignoredExceptions = new List<Type>();

        private readonly Clock clock;
        private readonly T input;
        private readonly Sleeper sleeper;

        protected ThucydidesFluentWait(T input, Clock clock, Sleeper sleeper)
        {

            if (input == null) throw new ArgumentNullException(nameof(input));
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            if (sleeper == null) throw new ArgumentNullException(nameof(sleeper));
            this.input = input;
            this.clock = clock;
            this.sleeper = sleeper;
        }



        protected Clock getClock()
        {
            return clock;
        }

        protected T getInput()
        {
            return input;
        }

        protected Sleeper getSleeper()
        {
            return sleeper;
        }

        public bool until(Predicate<T> isTrue)
        {
            var end = getClock().laterBy(timeout.In(TimeUnit.MILLISECONDS));
            Exception lastException = null;
            var waitForConditionMessage = isTrue.ToString();
            while (true)
            {
                if (aPreviousStepHasFailed())
                {
                    return true;
                }
                try
                {
                    var value = isTrue(input);
                    bool b = false;
                    if (value != null && bool.TryParse(value.ToString(), out b))
                    {
                        if (b)
                        {
                            return value;
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Condition should be a bool function", nameof(isTrue));
                    }
                }
                catch (Exception e)
                {
                    lastException = propagateIfNotIngored(e);
                }

                if (!getClock().isNowBefore(end))
                {
                    String message = String.Format("Timed out after {0} milliseconds: ", timeout.In(TimeUnit.MILLISECONDS)) + waitForConditionMessage;
                    throw timeoutException(message, lastException);
                }

                try
                {
                    doWait();
                }
                catch (ThreadInterruptedException e)
                {
                    Thread.CurrentThread.Interrupt();
                    throw new WebDriverException("ThreadInterrupted during wait",e);
                }
            }
        }

        private bool aPreviousStepHasFailed()
        {
            return StepEventBus.getEventBus().aStepInTheCurrentTestHasFailed();
        }

        public abstract void doWait();

        private Exception propagateIfNotIngored(Exception e)
        {
            foreach (var ignoredException in ignoredExceptions)
            {
                if (ignoredException.IsAssignableFrom(e.GetType()))
                {
                    return e;
                }
            }
            throw e;
        }

        public ThucydidesFluentWait<T> ignoring(params Type[] types)
        {
            ignoredExceptions.AddRange(types);
            return this;
        }

        public ThucydidesFluentWait<T> withTimeout(long duration, TimeUnit unit)
        {
            this.timeout = new Duration(duration, unit);
            return this;
        }

        public ThucydidesFluentWait<T> withTimeout(Duration timeout)
        {
            this.timeout = timeout;
            return this;
        }

        public ThucydidesFluentWait<T> pollingEvery(long duration, TimeUnit unit)
        {
            this.interval = new Duration(duration, unit);
            return this;
        }

        protected Exception timeoutException(String message, Exception lastException)
        {
            throw new TimeoutException(message, lastException);
        }

        public TimeoutSchedule withTimeoutOf(int amount)
        {
            return new TimeoutSchedule(this, amount);
        }

        public PollingSchedule pollingEvery(int amount)
        {
            return new PollingSchedule(this, amount);
        }

        public void until(ExpectedCondition condition)
        {
            throw new NotImplementedException();
        }

        public void until(ExpectedCondition<bool> condition)
        {
            throw new NotImplementedException();
        }
    }
}