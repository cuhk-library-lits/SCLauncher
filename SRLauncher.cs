#define TRACE
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.ServiceProcess;
using System.Collections.Generic;

namespace CUHKSelfCheckLauncher
{
    public class SRLauncher : Form
    {
        static Mutex mutex = new Mutex (false, "CUHKSelfCheckLauncher.SRLauncher");

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        [STAThread]
        public static void Main()
        {
            if (!mutex.WaitOne(TimeSpan.FromSeconds(5), false))
                return;
            
            Application.Run(new SRLauncher());
        }

        public SRLauncher()
        {
            try
            {
                PrepareTrayIcon();
                SystemsLaunch();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            this.Close();
        }

        private void SystemsLaunch()
        {
            if (!StartBibliothecaConfigService())
                return;

            // Autolaunch
            List<string> launchAppPaths = Config.GetLaunchApps();
            foreach (string appPath in launchAppPaths)
                SystemUtil.StartProcess(appPath, null, false);
        }

        private bool StartBibliothecaConfigService()
        {
            String serviceName = Config.GetBibliothecaConfigServiceName();

            if (!String.IsNullOrEmpty(serviceName))
            {
                try
                {
                    // If not stopped initially, stop service
                    ServiceController sc = new ServiceController(serviceName);
                    sc.Refresh();
                    
                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(300));
                    }

                    // Start service
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(300));

                    return true;
                }
                catch (System.Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return false;
                }
            }
            return false;
        }

        private void Exit()
        {
            Application.Exit();
            mutex.ReleaseMutex();
        }

        protected void PrepareTrayIcon()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "SRLauncher";
            trayIcon.Icon = new Icon(SystemIcons.Information, 40, 40);
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                trayIcon.Dispose();
            }
            base.Dispose(isDisposing);
        }
    }
}
