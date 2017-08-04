using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FEA_BusinessLogic;

namespace FEA_ITS_Site.Helper.MRB
{
    public abstract class ScheduleBase
    {
        public ScheduleBase()
        {
        }

        public ScheduleBase(MRBBookingDetail mrb)
        {
            if (mrb != null)
            {
                ID = mrb.ID;
                Label = mrb.Label;
                AllDay = mrb.AllDay;
             //   Location = mrb.Location;
                RoomId = mrb.RoomId;
                Status = mrb.Status;
             //   RecurrenceInfo = mrb.RecurrenceInfo;
              //  ReminderInfo = mrb.ReminderInfo;
            }
        }

        public int ID { get; set; }
        public int? EventType { get; set; }
        public int? Label { get; set; }
        public bool? AllDay { get; set; }
        public string Location { get; set; }
        public int? RoomId { get; set; }
        public int? Status { get; set; }
        public string RecurrenceInfo { get; set; }
        public string ReminderInfo { get; set; }

        public virtual void Assign(ScheduleBase source)
        {
            if (source != null)
            {
                ID = source.ID;
               // EventType = source.EventType;
                Label = source.Label;
                AllDay = source.AllDay;
               // Location = source.Location;
                RoomId = source.RoomId;
                Status = source.Status;
             //   RecurrenceInfo = source.RecurrenceInfo;
               // ReminderInfo = source.ReminderInfo;
            }
        }
    }
}