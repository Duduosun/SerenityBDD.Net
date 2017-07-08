using System;
using System.Linq;

namespace SerenityBDD.Core.Model
{
    internal class Joiner
    {
        private readonly string _separator;
        private bool _ignoreNulls;

        private Joiner(string separator)
        {
            _separator = separator;

        }

        public static Joiner On(string separator)
        {
            return new Joiner(separator);
        }

        public string Join(params string[] args)
        {
            var argsTojoin = args.Where(x => !_ignoreNulls || !string.IsNullOrEmpty(x));
            return String.Join(_separator, argsTojoin);
        }

        public Joiner skipNulls()
        {
            _ignoreNulls = true;
            return this;
        }
    }
}