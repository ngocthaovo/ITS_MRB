using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.WarehouseArea
{
    public class PackingManifestReturnedItemManager : Base.Connection
    {

        public enum Type
        {
            Returned = 1,
            Stockined =2
        }

        public PackingManifestReturnedItem GetItem(string sID)
        {
            return db.PackingManifestReturnedItems.Where(i => i.ID == sID).SingleOrDefault();
        }

        /// <summary>
        /// Get item list
        /// </summary>
        /// <param name="iType"></param>
        /// <returns></returns>
        public List<PackingManifestReturnedItem> GetItems( int iType)
        {
            return db.PackingManifestReturnedItems.Where(i =>
                                                            ((iType > -1)?i.Type == iType:true)
                                                            ).OrderByDescending(i => i.Order).ToList();
        }


        public string GetItemNameByID(string ID)
        {
            var item = db.PackingManifestReturnedItems.Where(i => i.ID == ID).Select(i => i.ItemName).SingleOrDefault();
            return item == null ? "" : item.ToString();
        }
    }
}
