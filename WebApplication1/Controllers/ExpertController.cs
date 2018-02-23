using System.Linq;
using System.Web.Http;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ExpertController : ApiController
    {
        public byte Get(string symbol)
        {
            var context = new FxContext();

            var symb = context.Symbols.FirstOrDefault(s => s.Name == symbol);
            if (symb == null) return 0;

            var quant = context.Quants.Where(q=>q.SymbolId == symb.Id).OrderByDescending(q => q.Ind).FirstOrDefault();
            if (quant == null) return 0;

            return (byte?)quant.Recommendation ?? 0;
        }
    }
}
