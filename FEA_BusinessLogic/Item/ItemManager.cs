using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FEA_BusinessLogic
{
    public class ItemManager:Base.Connection
    {
        /// <summary>
        /// GetItem
        /// </summary>
        /// <param name="sItemID"></param>
        /// <returns></returns>
        public Item GetItem(string sItemID)
        {
            return db.Items.Where(i => i.ID == sItemID).SingleOrDefault();
        }

        /// <summary>
        /// GetItem List
        /// </summary>
        /// <param name="sItemType"></param>
        /// <param name="iEnabled"></param>
        /// <returns></returns>
        public List<Item> GetItems(string sItemType,string sOrderType,int? iEnabled = -1)
        {
            return db.Items.Where(i => i.ItemType == sItemType && (iEnabled >= 0 ? i.Status == iEnabled : true) && (sOrderType ==""?true:(sOrderType == i.Temp1))).OrderBy(i => i.ItemName).ToList();
        }

        public List<Item> GetAllItems(int? iEnabled = -1)
        {
            return db.Items.Where(i =>(iEnabled >= 0 ? i.Status == iEnabled : true)).OrderBy(i=>i.ItemType).ToList();
        }

          //Added by Tony (2017-04-26) Lấy thông tin Item từ phiếu yêu cầu
        public List<Item> GetRequestItem(string requestMainID)
        {
            return db.Items.Join(db.MNRequestMainDetails, a => a.ID, b => b.ItemID, (a, b) => new { a, b }).Where(i => i.b.RequestMainID == requestMainID).Select(i=>i.a).Distinct().ToList();
        }
        //Added by Tony (2017-04-26) Lấy thông tin ItemDetail từ phiếu yêu cầu
        public List<sp_GetItemDetailName_Result> GetRequestItemDetail(string requestMainID, string itemID)
        {
            return db.sp_GetItemDetailName(requestMainID, itemID).ToList();
        }

        /// <summary>
        /// Insert Item with Item to database
        /// </summary>
        /// <param name="o"></param>
        /// <returns>1: true; 0: false, -1: dubplicate Type</returns>
        public int InsertItem(Item o)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    o.ID = Guid.NewGuid().ToString();
                    if (o.Status == null) o.Status = 0;
                    if (o.Temp1 == null) o.Temp1 = "";
                    if (o.Temp2 == null) o.Temp2 = "";

                    db.Items.Add(o);
                    db.SaveChanges();
                    transaction.Complete();
                    return 1;
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    return 0;
                }
            }
        }

      

        /// <summary>
        /// Update Item's Infomation
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateItem(Item o, params System.Linq.Expressions.Expression<Func<Item, object>>[] properties)
        {
            var item = db.Items.Where(i => i.ID == o.ID).SingleOrDefault();
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
