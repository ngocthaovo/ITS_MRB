using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
namespace FEA_BusinessLogic
{
    public class ItemDetailManager:Base.Connection
    {
        public ItemDetail GetItem(string sItemID)
        {
            return db.ItemDetails.Where(i => i.ID == sItemID && i.Status == 1).SingleOrDefault();
        }

        /// <summary>
        /// Get Itemdetail by item
        /// </summary>
        /// <param name="sItemID"></param>
        /// <returns></returns>
        public List<ItemDetail> GetItems(string sItemID)
        {
            return db.ItemDetails.Where(i => i.ItemID == sItemID && i.Status ==1).OrderBy(i => i.ItemDetailName).OrderByDescending(i=>i.ItemDetailName).ToList();
        }
          /// <summary>
          /// Get Item Detail base on Position
          /// </summary>
          /// <param name="sItemID"></param>
          /// <param name="UserPositionID"></param>
          /// <returns></returns>
        public List<ItemDetail> GetItems(string sItemID,string UserPositionID)
        {
            if (UserPositionID == "017B2926-147C-451B-826F-0D11082A4311"    // Administrator
                || UserPositionID == "0008C5D0-6F02-47F9-A65F-13A7556293A1" // Vice Leader
                || UserPositionID == "0004196D-595F-46F0-B6D0-2DC44F54A8A3" // Leader
                || UserPositionID == "000F0723-5FE6-448D-A82A-B0DFB4A9C484" // Assistant
               // || UserPositionID == "0005EBA8-764E-4CD2-B64A-B3963CF639A8" // Supervisor // Modified by Steven 2015-03-10
                || UserPositionID == "01D00E0D-35F8-46BA-AE0B-F97CE4160CE2" // Assisstant Manager 
                )
            {
                return db.ItemDetails.Where(i => i.ItemID == sItemID && i.ID != "2378ea4d-21b1-4b7e-adab-8ebe5d07c538"/* Network - Unlimited */ && i.Status == 1).OrderBy(i => i.ItemDetailName).ToList();      
            }
            else
                return db.ItemDetails.Where(i => i.ItemID == sItemID).OrderBy(i => i.ItemDetailName).ToList();
            
        }

        public List<ItemDetail> GetReason()
        {
            return db.ItemDetails.Where(i => i.ItemID == "c4fbbdf1-20c7-48f1-923e-bd4da6acf377" /*ItemID: DRReason*/ && i.Status == 1).OrderBy(i => i.ItemDetailName).OrderByDescending(i => i.ItemDetailName).ToList();
        }
        /// <summary>
        /// Get All Item in DB
        /// </summary>
        /// <returns></returns>
        public List<ItemDetail> GetItems()
        {
            return db.ItemDetails.ToList();
        }


        public List<ItemDetail> GetItemsByItemType(string sItemType)
        {
            return db.ItemDetails.Where(i => i.Item.ItemType == sItemType && i.Status ==1).OrderBy(i => i.ItemDetailName).ToList();
        }

        /// <summary>
        /// Get List by ExportItem for Adjust Applicaiton
        /// </summary>
        /// <param name="ExportItemID"></param>
        /// <returns></returns>
        public List<ItemDetail> GetItemByExportItem(string ExportItemID)
        {
            return db.ExportItemDetails.Where(i => i.ExportItemID == ExportItemID).Select(a => a.ItemDetail).ToList();
        }


        /// <summary>
        /// Insert An Item to DB
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public Boolean InsertItem(ItemDetail o)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    o.ID = Guid.NewGuid().ToString();
                    o.Status = 1; // Default is Active
                    if (o.Temp1 == null) o.Temp1 = "";
                    if (o.Temp2 == null) o.Temp2 = "";

                    db.ItemDetails.Add(o);
                    db.SaveChanges();
                    transaction.Complete();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    return false;
                }
            }
        }

        /// <summary>
        /// Update Item Detail's Infomation
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateItem(ItemDetail o, params System.Linq.Expressions.Expression<Func<ItemDetail, object>>[] properties)
        {
            var item = db.ItemDetails.Where(i => i.ID == o.ID).SingleOrDefault();
            if (item != null)
            {
                foreach (var propertie in properties)
                {
                    var lambda = (LambdaExpression)propertie;
                    MemberExpression memberExpression;
                    if (lambda.Body is UnaryExpression)
                        memberExpression = (MemberExpression)((UnaryExpression)lambda.Body).Operand;
                    else
                        memberExpression = (MemberExpression)lambda.Body;

                    string propertyName = memberExpression.Member.Name;
                    item.GetType().GetProperty(propertyName).SetValue(item, o.GetType().GetProperty(propertyName).GetValue(o));
                }
                db.SaveChanges();
            }
            else
            {
                return false;
            }
            return true;
        }


    }
}
