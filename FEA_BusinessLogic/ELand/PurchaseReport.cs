using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.ELand
{
    public class PurchaseReport :Base.Connection
    {
        public List<FEA_BusinessLogic.sp_QueryPurchaseReport_Result> GetMain(string DateTo,string DateFrom)
        {
            return db.sp_QueryPurchaseReport(DateFrom, DateTo).ToList();
        }
    }
}
