using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;
using TaskStatus = WebApplication1.Models.TaskStatus;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new FxContext();
            var newTasks = context.NetworkCalculationTasks
                .Where(t => t.Status == TaskStatus.New && t.ShouldStartAt < DateTime.Now).ToArray();

            foreach (var networkCalculationTask in newTasks)
            {
                networkCalculationTask.Status = TaskStatus.InProgress;
            }

            context.SaveChanges();

            CreateTheBestNetwork(context, newTasks.Select(t=>t.SymbolId).ToArray());

            foreach (var networkCalculationTask in newTasks)
            {
                networkCalculationTask.Status = TaskStatus.Complited;
            }
            context.SaveChanges();
        }

        static void CreateTheBestNetwork(FxContext context, int[] symbolIds)
        {
            var netwotk = new Network()
            {

            };
        }
    }
}
