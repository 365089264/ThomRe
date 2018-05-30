using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;


namespace VAV.Scheduler
{
    [RunInstaller(true)]
    public partial class ServerInstaller : System.Configuration.Install.Installer
    {
        public ServerInstaller()
        {
            InitializeComponent();
            this.BeforeInstall += new InstallEventHandler(ServerInstallerInstall);
            this.BeforeUninstall += new InstallEventHandler(ServerInstallerInstall);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerInstallerInstall(object sender, InstallEventArgs e)
        {
            var serviceProcessInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            serviceInstaller.DisplayName = !String.IsNullOrEmpty(this.Context.Parameters["DisplayName"])
                                               ? this.Context.Parameters["DisplayName"]
                                               : "VAV Scheduler";
            serviceInstaller.Description = "ThomsonReuters VAV Scheduler for executing jobs on schedule.";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = !String.IsNullOrEmpty(this.Context.Parameters["ServiceName"])
                                               ? this.Context.Parameters["ServiceName"]
                                               : "VAV.Scheduler";
            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
