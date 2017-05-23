using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Configuration;
namespace FEA_BusinessLogic.Base
{
   public class Connection
   {
       public FEA_ITSEntities db = new FEA_ITSEntities();
       //public static string m_defaultConnectionString = "metadata=res://*/FEASiteDataModel.csdl|res://*/FEASiteDataModel.ssdl|res://*/FEASiteDataModel.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=10.17.215.12;initial catalog=ITS;persist security info=True;user id=FEAWUser;password=!FEAWUser89;MultipleActiveResultSets=True;App=EntityFramework&quot;";
       public static string m_currentConnectionString 
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    //return "metadata=res://*/FEASiteDataModel.csdl|res://*/FEASiteDataModel.ssdl|res://*/FEASiteDataModel.msl;provider=System.Data.SqlClient;provider connection string='data source=10.17.215.12;initial catalog=ITS;persist security info=True;user id=dev_user;password=test;multipleactiveresultsets=True;application name=EntityFramework'";
                    return ConfigurationManager.ConnectionStrings["FEAServer-Far Eastern Apparel"].ConnectionString;
                }
                else
                {
                    if (HttpContext.Current.Session["CurrentConnectionString"] == null)
                    {
                        return "metadata=res://*/FEASiteDataModel.csdl|res://*/FEASiteDataModel.ssdl|res://*/FEASiteDataModel.msl;provider=System.Data.SqlClient;provider connection string='data source=10.17.213.167;initial catalog=ITS_Test;persist security info=True;user id=dev_user;password=test;multipleactiveresultsets=True;application name=EntityFramework'";
                    }
                    else
                    {
                        return HttpContext.Current.Session["CurrentConnectionString"].ToString();
                    }
                }
            }
        }



       System.Data.Common.DbTransaction dbContextTransaction;
       public Connection()
       {
           IObjectContextAdapter dbcontextadapter = (IObjectContextAdapter)db;
           dbcontextadapter.ObjectContext.CommandTimeout = 0;

           //db.Database.Connection.ConnectionString = ecsb.ConnectionString


            ChangeDatabase
                (db,
                initialCatalog: "",
                userId: "",
                password: "",
                dataSource: @"" // could be ip address 120.273.435.167 etc
                , configConnectionString: m_currentConnectionString
                ); 
       }


       public void BeginTransaction()
       {
           if(db.Database.Connection.State == System.Data.ConnectionState.Closed)
               db.Database.Connection.Open();
           dbContextTransaction = db.Database.Connection.BeginTransaction();
       }

       public void EndTransaction()
       {
           dbContextTransaction.Commit();
           db.Database.Connection.Close();
       }

       public void RollbackTransaction()
       {
           dbContextTransaction.Rollback();
           db.Database.Connection.Close();
       }


       public static  void SetConnectionString(string sConnectionString)
       {
           HttpContext.Current.Session["CurrentConnectionString"] = sConnectionString;
       }

       public  void ChangeDatabase(
         FEA_ITSEntities source,
        string initialCatalog = "",
        string dataSource = "",
        string userId = "",
        string password = "",
        bool integratedSecuity = false,
        string configConnectionString = "")
       /* this would be used if the
       *  connectionString name varied from 
       *  the base EF class name */
       {
           try
           {

               // add a reference to System.Configuration
               var entityCnxStringBuilder = new EntityConnectionStringBuilder
                          (configConnectionString);

               // init the sqlbuilder with the full EF connectionstring cargo
               var sqlCnxStringBuilder = new SqlConnectionStringBuilder
                   (entityCnxStringBuilder.ProviderConnectionString);

               // only populate parameters with values if added
               if (!string.IsNullOrEmpty(initialCatalog))
                   sqlCnxStringBuilder.InitialCatalog = initialCatalog;
               if (!string.IsNullOrEmpty(dataSource))
                   sqlCnxStringBuilder.DataSource = dataSource;
               if (!string.IsNullOrEmpty(userId))
                   sqlCnxStringBuilder.UserID = userId;
               if (!string.IsNullOrEmpty(password))
                   sqlCnxStringBuilder.Password = password;

               // set the integrated security status
               sqlCnxStringBuilder.IntegratedSecurity = integratedSecuity;

               // now flip the properties that were changed
               source.Database.Connection.ConnectionString
                   = sqlCnxStringBuilder.ConnectionString;
           }
           catch (Exception ex)
                {
               // set log item if required
           }
       }
    }




}
