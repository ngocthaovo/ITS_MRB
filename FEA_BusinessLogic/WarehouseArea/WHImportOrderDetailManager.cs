using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FEA_BusinessLogic.WarehouseArea
{
    public class WHImportOrderDetailManager : Base.Connection
    {
        public enum ItemStatus
        {
            NotSetShelf = 0, // Chu thiet lap ke
            Shelfed = 1, //da thiet lap ke
            Exported = 2, // da xuat
            Returned = 3 // da tra lai

        }

        public List<WHImportOrderDetail> GetItems(string WHImportID)
        {
            return db.WHImportOrderDetails.Where(i =>
                                                     (i.Status != (int)ItemStatus.Returned)
                                                     && (i.WHImportOrderID == WHImportID)
                                                     ).ToList();
        }


        public int UpdateShelf(List<string> Ids, string ShelfID)
        {
            List<WHImportOrderDetail> lst = db.WHImportOrderDetails.Where(i => Ids.Contains(i.ID)).ToList();
            if (lst.Count > 0)
            {
                foreach (WHImportOrderDetail i in lst)
                {
                    i.ShelfID = ShelfID ;
                    i.Status = (int)FEA_BusinessLogic.WarehouseArea.WHImportOrderDetailManager.ItemStatus.Shelfed;
                }
                
                db.SaveChanges();


                WHImportOrder itemorder = lst[0].WHImportOrder;
                if (itemorder != null)
                {
                    int count = itemorder.WHImportOrderDetails.Where(i => i.ShelfID != null).Count();
                    if (count == itemorder.WHImportOrderDetails.Count())
                    { 
                        itemorder.Status = (int)FEA_BusinessLogic.WarehouseArea.WHImportOrderManager.OrderStatus.SHELFED;
                        db.SaveChanges();
                    }
                }

                return 1;
            }
            return 0;
        }


        public bool UpdateItemDetail(WHImportOrderDetail o, params System.Linq.Expressions.Expression<Func<WHImportOrderDetail, object>>[] properties)
        {
            try
            {
                WHImportOrderDetail item = db.WHImportOrderDetails.Where(i => i.ID == o.ID).SingleOrDefault();
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
                }
                db.SaveChanges();
                return true;

            }
            catch
            {
                return false;
            }

        }

    }
}
