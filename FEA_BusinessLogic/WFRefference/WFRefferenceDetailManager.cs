using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class WFRefferenceDetailManager : Base.Connection
    {
        public List<WFRefferenceDetail> GetItems(string WFRefferenceID)
        {
            var items = db.WFRefferenceDetails.Where(o => (WFRefferenceID == ""?true:o.WFRefferenceID == WFRefferenceID)).OrderBy(i => i.CreateDate).ToList();
            return items;

        }

        public bool InsertItem(WFRefferenceDetail o)
        {
            o.ID = Guid.NewGuid().ToString();
            o.CreateDate = DateTime.Now;

            if (o.Temp1 == null) o.Temp1 = "";
            if (o.Temp2 == null) o.Temp2 = "";
            if (o.Temp3 == null) o.Temp3 = "";

            db.WFRefferenceDetails.Add(o);
            db.SaveChanges();
            return true;
        }
    }
}
