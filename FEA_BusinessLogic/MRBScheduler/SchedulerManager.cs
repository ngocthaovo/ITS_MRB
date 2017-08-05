using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Transactions;
using FEA_BusinessLogic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data;
using System.Data.Objects;
namespace FEA_BusinessLogic.MRBScheduler
{
    public class SchedulerManager : Base.Connection
    {
        public IList<T> GetEditableAppointments<T>() where T : ScheduleBase
        {
            string key = "MedicalSchedule_" + typeof(T).Name;
            IList<T> appointments = (IList<T>)HttpContext.Current.Session[key];

            if (appointments == null)
            {
                appointments =db.MRBBookingDetails.ToList().Select(schedule => (T)Activator.CreateInstance(typeof(T), schedule)).ToList();
                HttpContext.Current.Session[key] = appointments;
            }
            return appointments;
        }
        public IEnumerable getAppointments()
        {
            return db.MRBBookingDetails.ToList();
        }
        public IEnumerable getAppointmentByID(string UserCode)
        {
            return db.MRBBookingDetails.Where(x => x.UserID == UserCode).OrderBy(x => x.ID).ToList();
        }
        public IEnumerable getResources()
        {
            return db.MRBRooms.ToList();
        }
        public List<User> getAllUser()
        {
            return db.Users.ToList();
        }
        public void InsertAppointment(MRBBookingDetail apt)
        {
            if (apt == null)
                return;
            apt.ID = apt.GetHashCode();
            db.MRBBookingDetails.Add(apt);
            db.SaveChanges();
        }
        //update MRBBooing room
        public void UpdateAppointment(MRBBookingDetail apt)
        {
            if (apt == null)
                return;
            var item = db.MRBBookingDetails.SingleOrDefault(x => x.ID == apt.ID);
            item.Subject = apt.Subject;
            item.Description = apt.Description;
            item.StartTime = apt.StartTime;
            item.EndTime = apt.EndTime;
            item.UserList = apt.UserList;
            // item.UserID = "00139";
            //    item.RoomId = apt.RoomId;
            db.SaveChanges();
        }
        //delete MRBBooking room
        public void RemoveAppointment(MRBBookingDetail apt)
        {
            var item = db.MRBBookingDetails.Find(apt.ID);
            db.MRBBookingDetails.Remove(item);
            db.SaveChanges();
        }
        //Object thoi nhe
        public static SchedulerObject DataObject
        {
            get
            {
                return new SchedulerObject()
                {
                    Appointments = new SchedulerManager().getAppointments(),
                    Resources = new SchedulerManager().getResources()
                };
            }
        }
        public static SchedulerObject EditableDataObject
        {
            get
            {
                return new SchedulerObject()
                {
                    Appointments =new  SchedulerManager().GetEditableAppointments<EditableSchedule>(),
                    Resources = new SchedulerManager().getResources()
                };
            }
        }
        //public static SchedulerObject USerDataObject(string userID)
        //{
        //    return new SchedulerObject()
        //    {
        //        Appointments = new SchedulerManager().getAppointmentByID(userID),
        //        Resources = new SchedulerManager().getResources()
        //    };
        //}
    }
    public class SchedulerObject
    {
        public System.Collections.IEnumerable Appointments { get; set; }
        public System.Collections.IEnumerable Resources { get; set; }
    }
    //CUStom onm here
    public class EditableSchedule : ScheduleBase
    {
        public EditableSchedule()
        {
        }
        public EditableSchedule(MRBBookingDetail editMrb)
            : base(editMrb)
        {
            Subject = editMrb.Subject;
            Description = editMrb.Description;
            StartTime = editMrb.StartTime;
            EndTime = editMrb.EndTime;
        }

        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public override void Assign(ScheduleBase source)
        {
            base.Assign(source);
            EditableSchedule editableSchedule = source as EditableSchedule;
            if (editableSchedule != null)
            {
                Subject = editableSchedule.Subject;
                Description = editableSchedule.Description;
                StartTime = editableSchedule.StartTime;
                EndTime = editableSchedule.EndTime;
            }
        }
    }
    public class ValidationSchedule : ScheduleBase
    {
        public ValidationSchedule() { }
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

        public ValidationSchedule(MRBBookingDetail mrb)
            : base(mrb)
        {
            if (mrb != null)
            {
                Subject = mrb.Subject;
                //StartTime = mrb.StartTime.Value;
                //EndTime = mrb.EndTime.Value;
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
            ValidationSchedule mrbs = source as ValidationSchedule;
            if (mrbs != null)
            {
                Subject = mrbs.Subject;
                //StartTime = mrbs.StartTime;
                //EndTime = mrbs.EndTime;
                Description = mrbs.Description;
                UserList = mrbs.UserList;
                ItemList = mrbs.ItemList;
                ItemId = mrbs.ItemId;
                //    UserID = mrbs.UserID;
            }
        }
    }

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