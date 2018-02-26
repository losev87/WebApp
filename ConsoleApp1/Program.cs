using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog.Examples.CSVMarketExample;
using WebApplication1.Models;
using TaskStatus = WebApplication1.Models.TaskStatus;

namespace ConsoleApp1
{
    class Program
    {
        private static FxContext _context;

        static void Main(string[] args)
        {
            _context = new FxContext();
            var newTasks = _context.NetworkCalculationTasks
                .Where(t => t.Status == TaskStatus.New && t.ShouldStartAt < DateTime.Now).ToArray();

            foreach (var networkCalculationTask in newTasks)
            {
                networkCalculationTask.Status = TaskStatus.InProgress;
            }

            _context.SaveChanges();

            foreach (var task in newTasks)
            {
                CreateTheBestNetworks(task);
            }

            foreach (var networkCalculationTask in newTasks)
            {
                networkCalculationTask.Status = TaskStatus.Complited;
            }
            _context.SaveChanges();
        }

        static void CreateTheBestNetworks(NetworkCalculationTask task)
        {
            var symb = _context.Symbols.SingleOrDefault(s => s.Id == task.SymbolId);
            if(symb==null) throw new Exception("Нет символа");
            var quantsForTrainAndQuantsForTest = _context.Quants
                .Where(q => q.SymbolId == task.SymbolId)
                .OrderByDescending(q => q.Ind)
                .Take(2050)
                .Reverse()
                .ToArray();
            var quantsForTrain = quantsForTrainAndQuantsForTest.Take(2000)
                .Select((q, i) => new {Date = new DateTime(1970, 0, 0).AddDays(i + 1), Close = q.Close})
                .ToDictionary(q => q.Date, q => q.Close);
            var quantsForTest = quantsForTrainAndQuantsForTest.Skip(2000 - 25).Take(25 + 50)
                .Select((q, i) => new { Date = new DateTime(1970, 0, 0).AddDays(i + 1), Close = q.Close })
                .ToDictionary(q => q.Date, q => q.Close);
            //получить сеть из базы
            //получить кванты для обучения
            //получить кванты для тестирования
            MarketBuildTraining.Generate(forexFile);
            var err = MarketTrain.Train(dataDir);
            var best = MarketPrune.Incremental(dataDir);
            var gErr = MarketEvaluate.Evaluate(dataDir, forexFile);

            var netwotk = new Network()
            {

            };
        }
    }
}
