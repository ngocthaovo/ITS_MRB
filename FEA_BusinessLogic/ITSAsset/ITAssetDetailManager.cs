using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.ITSAsset
{
    public class ITAssetDetailManager : Base.Connection
    {
        public int InsertItem(ITSAssetDetail o, out string sError)
        {
            try
            {
                o.UploadDate = DateTime.Now;


                o.AssetType = ITSAssetFTYManager.CheckAndInsertData(o.AssetType, (int)ITSAssetFTYManager.AssetType.Asset, db, out sError);
                o.Division = ITSAssetFTYManager.CheckAndInsertData(o.Division, (int)ITSAssetFTYManager.AssetType.Division, db, out sError);
                o.Department = ITSAssetFTYManager.CheckAndInsertData(o.Department, (int)ITSAssetFTYManager.AssetType.Department, db, out sError);
                o.Section = ITSAssetFTYManager.CheckAndInsertData(o.Section, (int)ITSAssetFTYManager.AssetType.Section, db, out sError);

                string recID = ITSAssetRecDataManager.CheckAndInsertData(o.RecCode, o.RecName, db, out sError);
                if (sError.Length == 0)
                    o.RecID = recID;

                db.ITSAssetDetails.Add(o);
                db.SaveChanges();
                sError = "";
                return 1;
            }
            catch (Exception ex)
            {
                sError = ex.Message;
                return 0;
            }
        }

        public List<ITSAssetDetail> GetItems(string Division, string Dept, string Section, string UserCode, string UserName, string AssetType, string Brand)
        {
            return db.ITSAssetDetails.Where(i =>
                    (i.Division.Contains(Division))
                    && (i.Department.Contains(Dept))
                    && (i.Section.Contains(Section))
                    && (UserCode.Length == 0 ? true : i.RecCode == UserCode)
                    && (i.RecName.Contains(UserName))
                    && (i.AssetType.Contains(AssetType))
                    && (i.Brand.Contains(Brand))
                    && (i.Status == 1)
                ).OrderByDescending(a=>a.UploadDate).ToList();
        }

        public int InsertItems(List<ITSAssetDetail> lst,Boolean isDeleteOlddata, out string sError)
        {
            sError = "";
            try
            {
                if (isDeleteOlddata == true)
                {
                    var lstOld = db.ITSAssetDetails.Where(i => i.CreatedByHand == 0).ToList();
                    lstOld.ForEach(i => db.ITSAssetDetails.Remove(i));
                }

                int count = 0;

                foreach (ITSAssetDetail item in lst)
                {

                    item.AssetType= ITSAssetFTYManager.CheckAndInsertData(item.AssetType, (int)ITSAssetFTYManager.AssetType.Asset, db, out sError);
                    item.Division = ITSAssetFTYManager.CheckAndInsertData(item.Division, (int)ITSAssetFTYManager.AssetType.Division, db, out sError);
                    item.Department = ITSAssetFTYManager.CheckAndInsertData(item.Department, (int)ITSAssetFTYManager.AssetType.Department, db, out sError);
                    item.Section = ITSAssetFTYManager.CheckAndInsertData(item.Section, (int)ITSAssetFTYManager.AssetType.Section, db, out sError);

                    string recID= ITSAssetRecDataManager.CheckAndInsertData(item.RecCode, item.RecName, db, out sError);
                    if (sError.Length == 0)
                        item.RecID = recID;

                    db.ITSAssetDetails.Add(item); count += 1;
                }

                db.SaveChanges();
                return count;
            }
            catch(Exception ex)
            {
                sError = ex.Message;
                return -1;
            }
        }

        public bool DeleteItem(string sItemID)
        {
            var item = db.ITSAssetDetails.Where(i => i.ID == sItemID).SingleOrDefault();
            if (item != null)
            {
               // db.ITSAssetDetails.Remove(item);
                item.Status = 0;
                db.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update ITSAssetDetails's Infomation
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateItem(ITSAssetDetail o, params System.Linq.Expressions.Expression<Func<ITSAssetDetail, object>>[] properties)
        {
            var item = db.ITSAssetDetails.Where(i => i.ID == o.ID).SingleOrDefault();
            if (item != null)
            {
                string sError = "";

                item.AssetType = ITSAssetFTYManager.CheckAndInsertData(item.AssetType, (int)ITSAssetFTYManager.AssetType.Asset, db, out sError);
                item.Division = ITSAssetFTYManager.CheckAndInsertData(item.Division, (int)ITSAssetFTYManager.AssetType.Division, db, out sError);
                item.Department = ITSAssetFTYManager.CheckAndInsertData(item.Department, (int)ITSAssetFTYManager.AssetType.Department, db, out sError);
                item.Section = ITSAssetFTYManager.CheckAndInsertData(item.Section, (int)ITSAssetFTYManager.AssetType.Section, db, out sError);

                string recID = ITSAssetRecDataManager.CheckAndInsertData(item.RecCode, item.RecName, db, out sError);
                if (sError.Length == 0)
                    item.RecID = recID;


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
