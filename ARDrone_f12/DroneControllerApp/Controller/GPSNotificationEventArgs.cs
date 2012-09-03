using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DroneController
{
    public class GPSNotificationEventArgs : EventArgs
    {
        public string Message { get; set; }
        public GPSNotificationEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
