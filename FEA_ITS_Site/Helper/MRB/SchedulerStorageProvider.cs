using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FEA_ITS_Site.Helper.MRB
{
    public class SchedulerStorageProvider
    {
        static MVCxAppointmentStorage defaultAppointmentStorage;
        public static MVCxAppointmentStorage DefaultAppointmentStorage
        {
            get
            {
                if (defaultAppointmentStorage == null)
                    defaultAppointmentStorage = CreateDefaultAppointmentStorage();
                return defaultAppointmentStorage;
            }
        }

        static MVCxAppointmentStorage CreateDefaultAppointmentStorage()
        {
            MVCxAppointmentStorage appointmentStorage = new MVCxAppointmentStorage();
            appointmentStorage.Mappings.AppointmentId = "ID";
            appointmentStorage.Mappings.Start = "StartTime";
            appointmentStorage.Mappings.End = "EndTime";
            appointmentStorage.Mappings.Subject = "Subject";
            appointmentStorage.Mappings.Description = "Description";
         //   appointmentStorage.Mappings.Location = "Location";
            appointmentStorage.Mappings.AllDay = "AllDay";
        //    appointmentStorage.Mappings.Type = "EventType";
        //    appointmentStorage.Mappings.RecurrenceInfo = "RecurrenceInfo";
        //    appointmentStorage.Mappings.ReminderInfo = "ReminderInfo";
            appointmentStorage.Mappings.Label = "Label";
            appointmentStorage.Mappings.Status = "Status";
            appointmentStorage.Mappings.ResourceId = "RoomId";
            //appointmentStorage.CustomFieldMappings.Add("Price", "Price");
            appointmentStorage.CustomFieldMappings.Add("UserList", "UserList");
            appointmentStorage.CustomFieldMappings.Add("ItemList", "ItemList");
            appointmentStorage.CustomFieldMappings.Add("ItemId", "ItemId");
           // appointmentStorage.CustomFieldMappings.Add("UserID","UserID");
            return appointmentStorage;
        }

        static MVCxResourceStorage defaultResourceStorage;
        public static MVCxResourceStorage DefaultResourceStorage
        {
            get
            {
                if (defaultResourceStorage == null)
                    defaultResourceStorage = CreateDefaultResourceStorage();
                return defaultResourceStorage;
            }
        }
        static MVCxResourceStorage CreateDefaultResourceStorage()
        {
            MVCxResourceStorage resourceStorage = new MVCxResourceStorage();
            resourceStorage.Mappings.ResourceId = "RoomId";
            resourceStorage.Mappings.Caption = "RoomName";
            return resourceStorage;
        }
    }
}