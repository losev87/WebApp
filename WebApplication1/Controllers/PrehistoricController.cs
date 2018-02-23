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

        public string Put(string symbol)
        {
            var symb = _context.Symbols.FirstOrDefault(s => s.Name == symbol);
            if (symb == null) throw new Exception("Нет символа");

            _context.Quants.RemoveRange(_context.Quants.Where(q => q.SymbolId == symb.Id));
            _context.SaveChanges();

            var ticks = _context.Ticks.Where(t => t.SymbolId == symb.Id).OrderByDescending(t => t.Time).ToArray();
            if(!ticks.Any()) throw new Exception("Нет данных");

            List<Quant> newQuants = new List<Quant>();

            var firstTick = ticks.First();

            newQuants.Add(new Quant()
            {
                Id = Guid.NewGuid(),
                TickId = firstTick.Id,
                SymbolId = firstTick.SymbolId,
                Close = firstTick.Close
            });

            foreach (var item in ticks)
            {
                var dif = Math.Abs(item.Close - newQuants.Last().Close);
                if (dif > symb.QuantSize)
                {
                    var sign = item.Close < newQuants.Last().Close ? -1 : 1;
                    var steps = (int)(dif / symb.QuantSize);
                    for (int i = 0; i < steps; i++)
                    {
                        var newQuantVal = newQuants.Last().Close + symb.QuantSize * sign;
                        newQuants.Add(new Quant()
                        {
                            TickId = item.Id,
                            Close = newQuantVal
                        });
                    }
                }
            }

            newQuants.Reverse();

            var quants = newQuants.Select((q, ind) => new Quant()
            {
                Close = q.Close,
                Id = Guid.NewGuid(),
                Ind = ind,
                SymbolId = symb.Id,
                TickId = q.TickId
            }).ToArray();

            _context.Quants.AddRange(quants);
            _context.SaveChanges();

            _context.NetworkCalculationTasks.Add(new NetworkCalculationTask()
            {
                SymbolId = symb.Id,
                ShouldStartAt = DateTime.Now,
                Status = TaskStatus.New
            });
            _context.SaveChanges();

            return "ok";
        }

        public long Get(string symbol)
        {
            var symb = _context.Symbols.FirstOrDefault(s => s.Name == symbol);
            if (symb == null) return 0;
            var lastTick = _context.Ticks.Where(t => t.SymbolId == symb.Id).OrderByDescending(t => t.Time).FirstOrDefault();
            if (lastTick == null) return 0;
            var time1970 = new DateTime(1970,1,1,0,0,0);
            var lastTimeLong = (lastTick.Time - time1970).TotalSeconds;
            if (lastTimeLong < 0) lastTimeLong = 0;
            return (long)lastTimeLong;
        }

        public string Post(string symbol, double close, long time)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var date = origin.AddSeconds(time);
            var symbolEntity = _context.Symbols.SingleOrDefault(s => s.Name == symbol);
            if (symbolEntity == null)
            {
                symbolEntity = new Symbol() { Name = symbol };
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
            return "ok";
        }
    }
}
