using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SerenityBDD.Core.Steps;

namespace SerenityBDD.Core.EventBus
{
    public class BroadCaster
    {
        private static ThreadLocal<StepEventBus> eventBusThreadLocal = new ThreadLocal<StepEventBus>();

        public static StepEventBus getEventBus()
        {
            if (eventBusThreadLocal.Value==null )
            {
                eventBusThreadLocal.Value =new StepEventBus();
            }
            return eventBusThreadLocal.Value;
        }

        public static void unregisterAllListeners()
        {
            eventBusThreadLocal.Dispose();
        }
    }
}
