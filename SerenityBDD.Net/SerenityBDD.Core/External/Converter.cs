using System;
using System.Collections.Generic;
using AutoMapper;

namespace SerenityBDD.Core.External
{
    public class CustomConverter<TSOURCE, TTARGET> : Converter<TSOURCE, TTARGET>
    {
        private readonly Func<TSOURCE, TTARGET> _converstionFunc;

        public CustomConverter(Func<TSOURCE, TTARGET > converstionFunc )
        {
            _converstionFunc = converstionFunc;
        }

        public override TTARGET Convert(TSOURCE src)
        {
            return _converstionFunc(src);
        }
    }
    public class AutoConverter<TSOURCE, TTARGET> : Converter<TSOURCE, TTARGET>
    {
        private IMapper _map;

        public override TTARGET Convert(TSOURCE src)
        {
            return _map.Map<TSOURCE, TTARGET>(src);
        }

        public AutoConverter()
        {
            var mc = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSOURCE, TTARGET>();
            });

            _map = mc.CreateMapper();
        }

        
    }
    public abstract class Converter<TSOURCE, TTARGET> 
    {
        
        
        public abstract TTARGET Convert(TSOURCE src);

        public IEnumerable<TTARGET> Convert(IEnumerable<TSOURCE> src)
        {
            foreach (var source in src)
            {
                yield return Convert(source);
            }
            
        }
    }
}