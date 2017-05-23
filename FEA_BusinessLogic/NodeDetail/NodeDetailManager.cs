using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class NodeDetailManager:Base.Connection
    {
        public List<WFNodeDetail> GetItems(string sNoteID)
        {
            return db.WFNodeDetails.Where(i => i.NodeID == sNoteID).ToList();
        }

        public string InsertItem(WFNodeDetail o)
        {
           // o = new WFNodeDetail();
            o.NodeDetailID = Guid.NewGuid().ToString();
            o.Status = 1;
            o.WFNode = null;
            o.User = null;
           
            db.WFNodeDetails.Add(o);
            db.SaveChanges();

            return o.NodeDetailID;
        }

        public bool DeleteItem(string sItemID)
        {
            WFNodeDetail item = db.WFNodeDetails.Where(i => i.NodeDetailID == sItemID).SingleOrDefault();
            if (item != null)
            {
                db.WFNodeDetails.Remove(item);
                db.SaveChanges();
                return true;
            }
            return false;
        }
        

        public bool IsDuplicate(string sNodeID, int iUserID, int iCenterCode)
        {
            int count = db.WFNodeDetails.Where(i => i.ApproverID == iUserID 
                                                    && i.NodeID == sNodeID 
                                                    && i.CostCenterCode == iCenterCode).Count();
            return count > 0 ? true : false;
        }
        /// <summary>
        /// Update NodeDetail's Infomation
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateItem(WFNodeDetail o, params System.Linq.Expressions.Expression<Func<WFNodeDetail, object>>[] properties)
        {
            var item = db.WFNodeDetails.Where(i => i.NodeDetailID == o.NodeDetailID).SingleOrDefault();
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
