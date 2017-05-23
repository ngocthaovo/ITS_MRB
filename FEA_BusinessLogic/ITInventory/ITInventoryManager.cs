using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class ITInventoryManager : Base.Connection
    {

        /// <summary>
        /// this function checking item in warehouse
        /// if the item exist then add item to it
        /// else - create new item
        /// </summary>
        /// <param name="ItemDetailID"></param>
        /// <param name="UnitID"></param>
        /// <param name="Quantity"></param>
        /// <param name="dbRef"></param>
        public void  AddItemToWarehouse(string ItemDetailID,string UnitID, int Quantity, FEA_BusinessLogic.FEA_ITSEntities dbRef)
        {
            var item = dbRef.ITInventories.Where(i => (i.ItemDetailID == ItemDetailID) && (i.UnitID == UnitID)).SingleOrDefault();
            if (item != null)
                item.Quantity += Quantity;
            else
            {
                item = new ITInventory()
                {
                    ID = Guid.NewGuid().ToString(),
                    ItemDetailID = ItemDetailID,
                    Quantity = Quantity,
                    UnitID = UnitID
                };
                dbRef.ITInventories.Add(item);
            }
        }

        public List<sp_GetDynamicInventory_Result> GetDynamicInventory(string DateFrom, string DateTo)
        {
            return db.sp_GetDynamicInventory(DateFrom, DateTo).ToList();   
        }

        public object GetDetailDynamicInventory(string DateFrom, string DateTo,string ItemDetailID)
        {
            return db.sp_GetDetailDynamicInventory(DateFrom, DateTo,ItemDetailID).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ItemDetailID"></param>
        /// <param name="UnitID"></param>
        /// <param name="Quantity"></param>
        /// <returns></returns>
        public bool CheckItemInStock(string ItemDetailID,string UnitID, int Quantity)
        {
            bool result = false;
            var item = db.ITInventories.Where(i => (i.ItemDetailID == ItemDetailID) && (i.UnitID == UnitID)).SingleOrDefault();
            if (item != null && item.Quantity >= Quantity)
                return true;

            return result;
        }


        public int CountInStock(string ItemDetailID, string UnitID)
        {
            var item = db.ITInventories.Where(i => (i.ItemDetailID == ItemDetailID) && (i.UnitID == UnitID)).SingleOrDefault();
            if (item != null )
                return item.Quantity.Value;

            return 0;
        }

        public List<sp_CheckCancelConfirmStockin_Result> CheckCancelConfirm(string OrderCode)
        {
            return db.sp_CheckCancelConfirmStockin(OrderCode).ToList();
        }

        public List<ITInventory> GetItems()
        {
            return db.ITInventories.ToList();
        }

        /// <summary>
        /// Calculate inventoty again
        /// </summary>
        /// <returns></returns>
        public bool InventoryCalculate()
        {
            try
            {
                db.sp_InventoryCalculate();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
