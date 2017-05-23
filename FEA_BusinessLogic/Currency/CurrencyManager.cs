using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class CurrencyManager:Base.Connection
    {
        public List<Currency> GetItems(int iEnabled =-1)
        {
            return db.Currencies.Where(i => iEnabled >= 0 ? i.Status == iEnabled : true).ToList();
        }
    }
}
