using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using JsonServices;
using JsonServices.Web;


namespace ITSProject
{
    /// <summary>
    /// Summary description for Handler1
    /// </summary>
    public class Handler1 : JsonHandler
    {
        public Handler1()
        {
            this.service.Name = "ITSProject";
            this.service.Description = "Day la JSON Services cho Android";
            InterfaceConfiguration IConfig = new InterfaceConfiguration("RestAPI", typeof(IServices), typeof(Services));
            this.service.Interfaces.Add(IConfig);     
        }
    }
}