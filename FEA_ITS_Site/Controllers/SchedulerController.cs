using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FEA_ITS_Site.Helper.MRB;
using FEA_BusinessLogic.MRBScheduler;
using DevExpress.Web.Mvc;
using FEA_BusinessLogic;
namespace FEA_ITS_Site.Controllers
{
    public class SchedulerController : Controller
    {
        //
        // GET: /Scheduler/
        //Show scheduler Tooltip
        public ActionResult CustomToolTip()
        {
            return View("CustomToolTip", FEA_BusinessLogic.MRBScheduler.SchedulerManager.DataObject);
        }
        public ActionResult CustomToolTipPartial()
        {
            return PartialView("CustomToolTipPartial", FEA_BusinessLogic.MRBScheduler.SchedulerManager.DataObject);
        }
        public ActionResult CustomToolTipEditAppointment()
        {
            try
            {
                UpdateAppointment();
            }
            catch (Exception e)
            {
                ViewData["SchedulerErrorText"] = e.Message;
            }
            return PartialView("CustomToolTipPartial", FEA_BusinessLogic.MRBScheduler.SchedulerManager.DataObject);
        }
        //End personal
        public ActionResult PersonalMeeting()
        {
            ViewBag.UserCurrent = FEA_ITS_Site.Helper.UserLoginInfo.UserCode;
            ViewData["listUser"] = new SchedulerManager().getAllUser();
            return View("PersonalMeeting", SchedulerManager.DataObject);
        }
        public ActionResult SchedulerPartial()
        {
            ViewBag.UserCurrent = FEA_ITS_Site.Helper.UserLoginInfo.UserCode;
            ViewData["listUser"] = new SchedulerManager().getAllUser();
            return PartialView("SchedulerPartial", SchedulerManager.DataObject);
        }
        public ActionResult EditAppointment()
        {
            ViewData["listUser"] = new SchedulerManager().getAllUser();
            try
            {
                UpdateAppointment();
            }
            catch (Exception e)
            {
                ViewBag.SchedulerErrorText = e.Message;
            }
            return PartialView("SchedulerPartial", SchedulerManager.DataObject);
        }

        static void UpdateAppointment()
        {
           MRBBookingDetail [] insertedAppointments = SchedulerExtension.GetAppointmentsToInsert<MRBBookingDetail>("scheduler",new SchedulerManager().getAppointments(),
         new SchedulerManager().getResources(), SchedulerStorageProvider.DefaultAppointmentStorage, SchedulerStorageProvider.DefaultResourceStorage);
            foreach (var appt in insertedAppointments)
            {
                //appt.UserID = "012345";
                new SchedulerManager().InsertAppointment(appt);
            }

            MRBBookingDetail[] updatedAppointments = SchedulerExtension.GetAppointmentsToUpdate<MRBBookingDetail>("scheduler", new SchedulerManager().getAppointments(),
                new SchedulerManager().getResources(), SchedulerStorageProvider.DefaultAppointmentStorage, SchedulerStorageProvider.DefaultResourceStorage);
            foreach (var appt in updatedAppointments)
            {
                new SchedulerManager().UpdateAppointment(appt);
            }

            MRBBookingDetail[] removedAppointments = SchedulerExtension.GetAppointmentsToRemove<MRBBookingDetail>("scheduler", new SchedulerManager().getAppointments(),
                new SchedulerManager().getResources(), SchedulerStorageProvider.DefaultAppointmentStorage, SchedulerStorageProvider.DefaultResourceStorage);
            foreach (var appt in removedAppointments)
            {
                new SchedulerManager().RemoveAppointment(appt);
            }
        }
    }
}
