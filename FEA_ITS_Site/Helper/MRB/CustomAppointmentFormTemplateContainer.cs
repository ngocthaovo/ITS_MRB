using DevExpress.Web.ASPxScheduler;
using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FEA_ITS_Site.Helper.MRB
{
    public class CustomAppointmentFormTemplateContainer: AppointmentFormTemplateContainer
    {
        public CustomAppointmentFormTemplateContainer(MVCxScheduler scheduler)
            : base(scheduler)
        {
        }

        public string UserList
        {
            get { return Convert.ToString(Appointment.CustomFields["UserList"]); }
        }
        public string ItemList
        {
            get { return Convert.ToString(Appointment.CustomFields["ItemList"]); }
        }
        public string ItemId
        {
            get { return Convert.ToString(Appointment.CustomFields["ItemId"]); }
        }
        //public string UserID
        //{
        //    get { return Convert.ToString(Appointment.CustomFields["UserID"]); }
        //}
    }
}