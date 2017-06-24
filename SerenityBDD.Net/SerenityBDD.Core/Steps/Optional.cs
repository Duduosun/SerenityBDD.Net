namespace SerenityBDD.Core.Steps
{
    public class Optional
    {
        protected  object _value;

        public bool isPresent()
        {
            return _value != null;
        }

        public Optional(object value)
        {
            _value = value;
        }
        public static Optional<T> of<T>(T src)
            where T : class 
        {
            return new Optional<T>(src);
        }

        
        public static Optional absent()
        {
            return new Optional(null );
        }

        public static Optional<T2> fromNullable<T2>(T2 src)
            where T2 : class
        {
            if (src != null) return new Optional<T2>(src);

            return (Optional<T2>)Optional.absent();
        }

    }
}