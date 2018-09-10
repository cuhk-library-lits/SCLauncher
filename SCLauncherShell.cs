#define TRACE
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.ServiceProcess;
using System.IO;

namespace CUHKSelfCheckLauncher
{
    public class SCLauncherShell : Form
    {
        static Mutex mutex = new Mutex (false, "CUHKSelfCheckLauncher.SCLauncherShell");
        const string IE_PROCESS_NAME = @"iexplore";
        const int HEARTBEAT_INTERVAL = 1000;
        const int SCREEN_SHOT_INTERVAL = 30;
        const int REBOOT_CHECK_INTERVAL = 60;

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private Thread heartbeatThread = null;

        private bool stopHeartbeat = false;
        private int screenShotCounter = 0;
        private int rebootCheckCounter = 0;


        [STAThread]
        public static void Main()
        {
            if (!mutex.WaitOne(TimeSpan.FromSeconds(5), false))
                return;
            
            Application.Run(new SCLauncherShell());
        }

        public SCLauncherShell()
        {
            try
            {
                PrepareTrayIcon();
                SystemsLaunch();
                heartbeatThread = new Thread(new ThreadStart(Heartbeat));
                heartbeatThread.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                Exit();
            }
        }

        private void SystemsLaunch()
        {
            try
            {
                Config.SetupSelfCheckAuthMode();
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
            
            // Reload stunnel
            Config.SetupSTunnel();
            ServiceController[] services = ServiceController.GetServices();
            foreach(ServiceController service in services)
            {
                if (Config.INI_SECT_STUNNEL == service.ServiceName && service.Status ==  ServiceControllerStatus.Running)
                {
                    SystemUtil.StartProcess(Config.GetSTunnelExe(), @" -reload -quiet", true);
                    break;
                }
            }

            // Set launch time
            SystemUtil.Init();

            // Autolaunch
            List<string> launchAppPaths = Config.GetLaunchApps();
            foreach (string appPath in launchAppPaths)
                SystemUtil.StartProcess(appPath, null, false);
        }

        private void KillIEProcesses()
        {
            String disableIEProcess = Config.GetDisableIEProcess();
            if (String.IsNullOrEmpty(disableIEProcess))
                return;

            if (Process.GetProcessesByName(disableIEProcess).Length > 0)
            {
                foreach (var process in Process.GetProcessesByName(IE_PROCESS_NAME))
                    process.Kill();
            }
        }

        private void TurnOffAllLockKeys()
        {
            LockKeyUtil.CapslockOff();
            LockKeyUtil.NumlockOff();
            LockKeyUtil.ScrolllockOff();
        }

        private void SaveScreenShot()
        {
            if (screenShotCounter++ < SCREEN_SHOT_INTERVAL)
                return;
            try
            {
                SystemUtil.SaveScreenShots(Config.GetWebPath());
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
            screenShotCounter = 0;
        }

        private void CheckDailyReboot()
        {
            if (rebootCheckCounter++ < REBOOT_CHECK_INTERVAL)
                return;
            try
            {
                SystemUtil.DailyReboot();
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
            rebootCheckCounter = 0;
        }

        private void DoHeartbeat()
        {
            try
            {
                TurnOffAllLockKeys();
                KillIEProcesses();
                SaveScreenShot();
                CheckDailyReboot();
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
        }

        private void Heartbeat()
        {
            while(!stopHeartbeat)
            {
                DoHeartbeat();
                Thread.Sleep(HEARTBEAT_INTERVAL);
            }
        }

        private void Exit()
        {
            stopHeartbeat = true;
            if (heartbeatThread.IsAlive)
                heartbeatThread.Join();
            Application.Exit();
            mutex.ReleaseMutex();
        }

        protected void PrepareTrayIcon()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "SCLauncherShell";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
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
