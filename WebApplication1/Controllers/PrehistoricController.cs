using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class Prehistoric : ApiController
    {
        private FxContext _context;

        public Prehistoric()
        {
            _context=new FxContext();
        }

        public int Post(string symbol, double close, long time)
        {
            var newQuantHasCome = SaveTick(symbol, close, time);
            if (!newQuantHasCome) return 3;
            var result = GetResult();
            return result;
        }

        bool SaveTick(string symbol, double close, long time)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var date = origin.AddSeconds(time);
            var symbolEntity = _context.Symbols.SingleOrDefault(s => s.Name == symbol);
            if (symbolEntity == null)
            {
                symbolEntity = new Symbol(){Name = symbol};
                _context.Symbols.Add(symbolEntity);
                _context.SaveChanges();
            }

            _context.Ticks.Add(new Tick()
            {
                Id = Guid.NewGuid(),
                SymbolId = symbolEntity.Id,
                Close = close,
                Time = date
            });


            return true;


        }

        int GetResult()
        {
            return 3;
        }
    }
}
