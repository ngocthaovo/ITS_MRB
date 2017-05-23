using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class UserPositionManager:Base.Connection
    {
        public List<UserPosition> GetItems(int? iEnabled=-1)
        {
            return db.UserPositions.Where(i => iEnabled < 0 ? true : i.Enabled == iEnabled).ToList();
        }
    }
}
