using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Data.Objects;

namespace FEA_BusinessLogic.ERP
{
    public class Order:Base.Connection
    {
        public int ERPUpdateStatus(string OrderCode, string Comment, int Status)
        {
            ObjectParameter outParam = new ObjectParameter("Result", typeof(int));
            db.sp_ERPUpdateStatus(OrderCode, Comment, Status, outParam);
            return  Convert.ToInt16(outParam.Value);
        }
        public int UpdateOrderStatus(string sFEPOCodes, string materialType)
        {
            return db.sp_ERPUpdateStatusFPO(sFEPOCodes, materialType).SingleOrDefault().Value;
        }

        /// <summary>
        /// Update Order Price
        /// </summary>
        /// <param name="sOrders"></param>
        /// <param name="newPrice"></param>
        /// <returns></returns>
        public int UpdateOrderPrice(string sOrders, decimal newPrice)
        {
            return db.sp_ERRUpdateOrderPrice(sOrders, newPrice).SingleOrDefault().Value;
        }

        //Added by Tony (2017-01-03)
        public ERPDocument GetItem(string ItemID)
        {
            return db.ERPDocuments.Where(i => i.ID == ItemID || i.OrderCode == ItemID).SingleOrDefault();
            //return db.ERPDocuments.Where(i => i.ID == ItemID).SingleOrDefault();
        }
        public List<ERPDocumentDetail> GetItemDetail(string ItemID)
        {
            return db.ERPDocumentDetails.Where(i => i.ERPDocumentsID == ItemID).ToList();
        }
        //  Tai
        public enum OrderStatus
        {
            DRAFT = 1,
            SENDING = 2,
            CHECKED = 3,
            RETURNED = 4,
            FINSHED = 5,
            DELETED = 0,
            PUSHDATA = 7,
            New = -1
        }

        public enum OrderType
        {
            ACCESSORYMOVEOUTMULTI,
            FABRICMOVEOUT,
            FABRICMOVEOUTMULTI,
            ACCESSORYDEVELOPOUT,
            DEVELOPPRODUCT,
            ACCESSORYMOVEOUT,
            ACCESSORYOUT,
            FABRICOUT,
            FABRICDEVELOPOUT

        }
        public bool UpdateStatus(ERPDocument erp, params System.Linq.Expressions.Expression<Func<ERPDocument, object>>[] properties)
        {
            var item = db.ERPDocuments.Where(i => i.ID == erp.ID || i.OrderCode == erp.OrderCode).SingleOrDefault();
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
                    item.GetType().GetProperty(propertyName).SetValue(item, erp.GetType().GetProperty(propertyName).GetValue(erp));
                }
                db.SaveChanges();

            }
            else
            {
                return false;
            }
            return true;
        }
       //Added by Tony (2017-03-08)
       public  List<sp_GetOpenOrderDoc_Result> GetOpenOrder(string FEPOCode, string FromDate,string ToDate)
        {
            return db.sp_GetOpenOrderDoc(FEPOCode, FromDate, ToDate).ToList();
        }
    }

}
