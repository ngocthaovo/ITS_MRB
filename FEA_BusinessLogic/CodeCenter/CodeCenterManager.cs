    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
   public class CodeCenterManager:Base.Connection
    {
       /// <summary>
       /// Get List of CostCenter from database
       /// </summary>
       /// <param name="iEnabled"></param>
       /// <returns></returns>
       public List<FEA_BusinessLogic.CostCenter> GetItems(int? iEnabled=-1)
       {
           return db.CostCenters.Where(i=>i.Enabled == 1).ToList();
       }

       public FEA_BusinessLogic.CostCenter GetItem(int ItemID)
       {
           return db.CostCenters.Where(i => i.CostCenterCode == ItemID).SingleOrDefault();
       }
    }
}
