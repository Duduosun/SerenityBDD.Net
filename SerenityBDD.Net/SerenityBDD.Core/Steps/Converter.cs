using System.Collections.Generic;
using AutoMapper;

namespace SerenityBDD.Core.Steps
{
    public class Converter<TSOURCE, TTARGET>
    {
        private IMapper _map;


        public Converter()
        {
            var mc = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSOURCE, TTARGET>();
            });

            _map = mc.CreateMapper();
        }
        public TTARGET Convert(TSOURCE src)
        {
            return _map.Map<TSOURCE, TTARGET>(src);
        }

        public IEnumerable<TTARGET> Convert(IEnumerable<TSOURCE> src)
        {
            foreach (var source in src)
            {
                yield return _map.Map<TSOURCE, TTARGET>(source);
            }
            
        }
    }
}