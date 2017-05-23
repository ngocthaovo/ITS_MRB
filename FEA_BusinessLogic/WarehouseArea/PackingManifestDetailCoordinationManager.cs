using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.WarehouseArea
{
    public   class PackingManifestDetailCoordinationManager
    {
        public int InsertItem(PackingManifestDetailCoordination item, FEA_ITSEntities db)
        {

            db.PackingManifestDetailCoordinations.Add(item);
            return 1;


        }

        public int DeleteItem(string itemID, FEA_ITSEntities db)
        {

           db.PackingManifestDetailCoordinations.Remove(db.PackingManifestDetailCoordinations.Where(i => i.ID == itemID).SingleOrDefault());
           return 1;
        }

        public int DeleteItemByParent(string parentID, FEA_ITSEntities db)
        {
            int count = 0;

            List<PackingManifestDetailCoordination> lst = db.PackingManifestDetailCoordinations.Where(i => i.PackingManifestDetailID == parentID).ToList();
            foreach(var c in lst)
            {
                db.PackingManifestDetailCoordinations.Remove(c);
                count += 1;
            }


            return count;
        }
    }
}
