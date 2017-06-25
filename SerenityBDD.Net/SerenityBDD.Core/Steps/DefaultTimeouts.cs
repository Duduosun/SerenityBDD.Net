namespace SerenityBDD.Core.Steps
{
    public class DefaultTimeouts
    {
        public static int DEFAULT_IMPLICIT_WAIT_TIMEOUT { get; private set; } = 30000;
        public static int DEFAULT_WAIT_FOR_TIMEOUT { get; private set; } = 30000;
    }
}