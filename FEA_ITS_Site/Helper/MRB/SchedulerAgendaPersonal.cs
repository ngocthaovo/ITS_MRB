using DevExpress.XtraScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FEA_BusinessLogic.MRBScheduler;
using System.Collections;
using FEA_BusinessLogic;
namespace FEA_ITS_Site.Helper.MRB
{
    public class SchedulerAgendaPersonal
    {
        public bool ShowViewNavigator { get; set; }
        public bool ShowViewVisibleInterval { get; set; }

        public AppointmentStatusDisplayType AppointmentsStatus { get; set; }
        public bool ShowResource { get; set; }
        public bool ShowLabel { get; set; }
        public bool ShowRecurrence { get; set; }
        public int DayCount { get; set; }
        
        public SchedulerAgendaPersonal()
        {
            ShowViewNavigator = true;
            ShowViewVisibleInterval = true;

            AppointmentsStatus = AppointmentStatusDisplayType.Bounds;

            ShowResource = true;
            ShowLabel = true;
            ShowRecurrence = true;

            DayCount = 3;
        }
        public IEnumerable Appointments
        {
            get { return SchedulerManager.DataObject.Appointments; }
        }
        public IEnumerable Resources
        {
            get
            {
                return ((List<MRBBookingDetail>)SchedulerManager.DataObject.Resources).Where(x => x.ID > 3).ToList();
            }
        }
    }
}