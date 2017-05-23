using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class DocumentTypeManager:Base.Connection
    {

        public WFDocumentType GetItem(string sDocTypeID)
        {
            return db.WFDocumentTypes.Where(i => i.DocumentTypeID == sDocTypeID).SingleOrDefault();
        }

        /// <summary>
        /// Get list of items
        /// </summary>
        /// <param name="sName"></param>
        /// <param name="iEnabled"></param>
        /// <returns></returns>
       public List<WFDocumentType> GetItems(string sName,int? iEnabled = -1)
        {
            return db.WFDocumentTypes.Where(i => i.DocumentTypeName.Contains(sName)
                                             && (iEnabled < 0 ? true : i.Status == iEnabled)).ToList();
        }

        public string InsertItem(WFDocumentType o)
       {
           o.DocumentTypeID = Guid.NewGuid().ToString();
           db.WFDocumentTypes.Add(o);
           db.SaveChanges();
           return o.DocumentTypeID;
       }

        public bool DeleteItem(string sItemID)
        {
            WFDocumentType item = db.WFDocumentTypes.Where(i => i.DocumentTypeID == sItemID).SingleOrDefault();
            if(item != null)
            {
                if (item.WFNodes.Count > 0)
                    return false;

                db.WFDocumentTypes.Remove(item);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update DocumentType's Infomation
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateItem(WFDocumentType o, params System.Linq.Expressions.Expression<Func<WFDocumentType, object>>[] properties)
        {
            var item = db.WFDocumentTypes.Where(i => i.DocumentTypeID == o.DocumentTypeID).SingleOrDefault();
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
