using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class NodeManager:Base.Connection
    {
        public WFNode GetItem(string sNodeID)
        {
            return db.WFNodes.SingleOrDefault(i => i.NodeID == sNodeID);
        }

        public List<WFNode> GetItems(string sDocTypeID, int ? iEnabled=-1)
        {
            return db.WFNodes.Where(i => i.DocumentTypeID.Contains(sDocTypeID)
                                       && iEnabled < 0 ? true : i.Status == iEnabled).OrderBy(i=>i.Sequence).ToList();
        }

        public string InsertItem(WFNode o)
        {
            o.NodeID = Guid.NewGuid().ToString();
            o.Status = 1;
            db.WFNodes.Add(o);
            db.SaveChanges();
            return o.NodeID;
        }

        public bool DeleteItem(string sItemID)
        {
            WFNode item = db.WFNodes.Where(i => i.NodeID == sItemID).SingleOrDefault();
            if (item != null)
            {
                if (item.WFNodeDetails.Count > 0)
                    return false;

                db.WFNodes.Remove(item);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update Node's Infomation
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateItem(WFNode o, params System.Linq.Expressions.Expression<Func<WFNode, object>>[] properties)
        {
            var item = db.WFNodes.Where(i => i.NodeID == o.NodeID).SingleOrDefault();
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
