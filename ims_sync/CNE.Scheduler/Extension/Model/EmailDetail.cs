using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActiveUp.Net.Mail;

namespace CNE.Scheduler.Extension.Model
{
   public  class EmailDetail
    {
       public EmailDetail(int id, string receiveTime, string subject, string sender, string  belongMailBox)
       {
           this.ID = id;
           this.ReceiveTime = receiveTime;
           this.Subject = subject;
           this.Sender = sender;
           AttachNames = new List<string>();
           this.BelongMailBox = belongMailBox;
       }
       public string  BelongMailBox { get; set; }
       public int ID { get; set; }
       public string ReceiveTime { get; set; }
       public string Subject { get; set; }
       public string Sender { get; set; }
       public List<string> AttachNames { get; set; }
           
    }
}
