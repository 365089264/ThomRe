using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceProcess;
using Quartz;
using Spring.Context;
using Spring.Context.Support;
using System.Diagnostics;

namespace CNE.Scheduler
{
    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();
        }

        public void button1_Click(object sender, EventArgs e)
        {
            var schedulerFactory = CreateSchedulerFactory();

            if (schedulerFactory == null) return;
            try
            {
                schedulerFactory.Start();
            }
            catch (Exception ex)
            {
                //this.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }

        private IScheduler CreateSchedulerFactory()
        {
            try
            {
                IApplicationContext context = ContextRegistry.GetContext();
                return (IScheduler)context.GetObject("CNEScheduler");
            }
            catch (Exception ex)
            {
                //this.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
            return null;
        }
    }
}
