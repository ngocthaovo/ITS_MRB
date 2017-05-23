using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
   public class UnitManager:Base.Connection
    {
       public Unit GetItem(string ID)
       {
           return db.Units.Where(i => i.ID == ID).SingleOrDefault();
       }

       public List<Unit> GetItems(int iEnabled = -1)
       {
           return db.Units.Where(i => iEnabled >= 0 ? i.Status == iEnabled : true).ToList();
       }
    }
}
