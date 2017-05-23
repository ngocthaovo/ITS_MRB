using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.ERP
{
    public class QueryPODivert:Base.Connection
    {
        public List<FEA_BusinessLogic.sp_QueryPODivert_Result> GetMain(string FepoCode)
        {
            return db.sp_QueryPODivert(FepoCode).ToList();

        }

        public List<FEA_BusinessLogic.sp_QueryPODivertDetail_Result> GetDetail(string FepoCode)
        {
            return db.sp_QueryPODivertDetail(FepoCode).ToList();
        }
    }
}
