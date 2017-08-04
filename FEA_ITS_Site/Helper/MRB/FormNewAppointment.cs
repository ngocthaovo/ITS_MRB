using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using FEA_BusinessLogic;
namespace FEA_ITS_Site.Helper.MRB
{
    public class FormNewAppointment:ScheduleBase
    {
        public FormNewAppointment() { }
        public int ID { get; set; }
        [Required(ErrorMessage = "The Subject must contain at least one character.")]
        public string Subject { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        public string Description { get; set; }
        public string UserList { get; set; }
        public string ItemList { get; set; }
        public string ItemId { get; set; }
    //    public string UserID { get; set; }

        public FormNewAppointment(MRBBookingDetail mrb)
            : base(mrb)
        {
            if (mrb != null)
            {
                Subject = mrb.Subject;
                StartTime = mrb.StartTime.Value;
                EndTime = mrb.EndTime.Value;
                Description = mrb.Description;
                UserList = mrb.UserList;
                ItemList = mrb.ItemList;
                ItemId = mrb.ItemID;
            //    UserID = mrb.UserID;
            }
        }
        public override void Assign(ScheduleBase source)
        {
            base.Assign(source);
            FormNewAppointment mrbs = source as FormNewAppointment;
            if (mrbs != null)
            {
                Subject = mrbs.Subject;
                StartTime = mrbs.StartTime;
                EndTime = mrbs.EndTime;
                Description = mrbs.Description;
                UserList = mrbs.UserList;
                ItemList = mrbs.ItemList;
                ItemId = mrbs.ItemId;
            //    UserID = mrbs.UserID;
            }
        }
    }
}