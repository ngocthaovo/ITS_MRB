using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FEA_BusinessLogic;
namespace FEA_SABusinessLogic
{
    public class ExportItemCheckingManager : FEA_BusinessLogic.Base.Connection
    {
        public string InsertItem(ExportItemChecking item)
        {
            try
            {
                item.CreateDate = DateTime.Now;
                item.ID = Guid.NewGuid().ToString();

                db.ExportItemCheckings.Add(item);
                db.SaveChanges();
                return item.ID;
            }
            catch(Exception ex)
            {
                return "";
            }
        }

        public int InsertItems(List<ExportItemChecking> lst)
        {
            try
            {
                foreach (ExportItemChecking item in lst)
                {
                    item.CreateDate = DateTime.Now;
                    item.ID = Guid.NewGuid().ToString();

                    db.ExportItemCheckings.Add(item);
                }

                db.SaveChanges();
                return lst.Count();
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public List<ExportItemChecking> GetItems(string ExportItemDetailID)
        {
            return db.ExportItemCheckings.Where(i => i.ExportItemDetailID == ExportItemDetailID).OrderByDescending(i=>i.CreateDate).ToList();
        }
    }
}
