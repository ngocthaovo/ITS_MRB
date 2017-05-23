using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.ELand
{
    public class SignFlow : Base.Connection
    {
        public List<FEA_BusinessLogic.sp_QuerySignFlowEland_Result> GetMain(string OrderCode)
        {
            return db.sp_QuerySignFlowEland(OrderCode).ToList();
        }
    }
}
