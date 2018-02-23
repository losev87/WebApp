using System.Linq;
using System.Web.Http;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class QuantController : ApiController
    {
        public string Put(string symbol, double quantSize)
        {
            var context = new FxContext();

            var symb = context.Symbols.FirstOrDefault(s => s.Name == symbol);
            if (symb == null) return 0.ToString();

            symb.QuantSize = quantSize;

            context.SaveChanges();

            return "ok";
        }
    }
}
