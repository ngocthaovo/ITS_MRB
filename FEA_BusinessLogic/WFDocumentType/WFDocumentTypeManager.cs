using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class WFDocumentTypeManager:Base.Connection
    {

        public List<WFDocumentType> GetItems( int? iEnabled=-1)
        {
            return db.WFDocumentTypes.Where(i => iEnabled >= 0 ? i.Status == iEnabled : true).OrderBy(i => i.DocumentTypeName).ToList();
        }
    }
}
