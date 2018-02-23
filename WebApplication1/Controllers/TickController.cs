using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using Encog.Neural.Data.Basic;
using Encog.Neural.Networks;
using Encog.Persist;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class TickController : ApiController
    {
        private FxContext _context;

        public TickController()
        {
            _context=new FxContext();
        }

        public string Post(string symbol, double close, long time)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var date = origin.AddSeconds(time);
            var symb = _context.Symbols.SingleOrDefault(s => s.Name == symbol);
            if (symb == null)
            {
                symb = new Symbol() { Name = symbol };
                _context.Symbols.Add(symb);
                _context.SaveChanges();
            }

            var tick = new Tick()
            {
                Id = Guid.NewGuid(),
                SymbolId = symb.Id,
                Close = close,
                Time = date
            };

            _context.Ticks.Add(tick);
            _context.SaveChanges();

            var quant = _context.Quants.Where(q => q.SymbolId == symb.Id).OrderByDescending(q => q.Ind).FirstOrDefault();
            if (quant == null)
            {
                _context.Quants.Add(new Quant()
                {
                    Id = Guid.NewGuid(),
                    SymbolId = symb.Id,
                    Close = close,
                    Recommendation = Recommendation.CloseAll,
                    Ind = 0,
                    TickId = tick.Id
                });
                return "ok";
            }

            var dif = Math.Abs(close - quant.Close);
            if (dif > symb.QuantSize)
            {
                var network = _context.Networks.SingleOrDefault(n => n.SymbolId == symb.Id);
                var sign = close < quant.Close ? -1 : 1;
                var steps = (int)(dif / symb.QuantSize);
                for (int i = 1; i <= steps; i++)
                {
                    Recommendation recommendation = Recommendation.CloseAll;
                    var newQuantVal = quant.Close + symb.QuantSize * sign * i;
                    var newQuant = new Quant()
                    {
                        TickId = tick.Id,
                        Close = newQuantVal,
                        Ind = quant.Ind + i,
                        Recommendation = recommendation
                    };

                    if (network != null)
                    {
                        network.QuantsProcessed++;

                        if (i == steps)
                        {
                            var inputQuants = _context.Quants.Where(q => q.SymbolId == symb.Id).OrderByDescending(q => q.Ind).Take(24).Reverse().ToList();
                            inputQuants.Add(newQuant);
                            if (inputQuants.Count == 25)
                            {
                                var predictor = (BasicNetwork)EncogDirectoryPersistence.LoadObject(new FileInfo(network.FileName));

                                BasicNeuralData input = GenerateInputNeuralData(inputQuants);

                                var predictData = predictor.Compute(input)[0];
                                newQuant.Recommendation = predictData < 0 ? Recommendation.Sell : Recommendation.Buy;
                            }

                            if (network.QuantsProcessed > 200)
                            {
                                _context.NetworkCalculationTasks.Add(new NetworkCalculationTask()
                                {
                                    ShouldStartAt = DateTime.Today.AddHours(18).AddMinutes(10),
                                    Status = TaskStatus.New,
                                    SymbolId = symb.Id
                                });
                            }
                        }
                    }

                    _context.Quants.Add(newQuant);
                    _context.SaveChanges();
                }
            }

            return "ok";
        }
        private BasicNeuralData GenerateInputNeuralData(List<Quant> inputQuants)
        {
            var result = new BasicNeuralData(inputQuants.Count);
            int resultIndex = 0;

            for (int i = 0; i < inputQuants.Count; i++)
            {
                result[resultIndex++] = i == 0
                    ? 0.0
                    : (inputQuants[i].Close - inputQuants[i - 1].Close) / inputQuants[i - 1].Close;
            }
            return result;
        }
    }
}
