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
    public class SCLauncher : Form
    {
        static Mutex mutex = new Mutex (false, "CUHKSelfCheckLauncher.SCLauncher");
        const string IE_PROCESS_NAME = @"iexplore";
        const string PATRON_UI_PROCESS_NAME = @"PatronUI";

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private ProcessChain ieProcessChains = new ProcessChain();

        private Thread heartbeatThread = null;
        private bool stopHeartbeat = false;
        private int secInterval = 1000;

        [STAThread]
        public static void Main()
        {
            if (!mutex.WaitOne(TimeSpan.FromSeconds(5), false))
                return;
            
            Application.Run(new SCLauncher());
        }

        public SCLauncher()
        {
            try
            {
                prepareTrayIcon();
                systemsLaunch();
                heartbeatThread = new Thread(new ThreadStart(heartbeat));
                heartbeatThread.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                exit();
            }
        }

        private void systemsLaunch()
        {
            Config.SetupSelfCheckAuthMode();
            
            // Reload stunnel
            Config.SetupSTunnel();
            ServiceController[] services = ServiceController.GetServices();
            foreach(ServiceController service in services)
            {
                if (Config.INI_SECT_STUNNEL == service.ServiceName && service.Status ==  ServiceControllerStatus.Running)
                {
                    Process stunnelProc = new System.Diagnostics.Process();
                    stunnelProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    stunnelProc.StartInfo.FileName = Config.GetSTunnelExe();
                    stunnelProc.StartInfo.Arguments = @" -reload -quiet";
                    stunnelProc.StartInfo.UseShellExecute = false;
                    stunnelProc.Start();
                    break;
                }
            }

            // Autolaunch
            List<string> launchAppPaths = Config.GetLaunchApps();
            foreach (string appPath in launchAppPaths)
            {              
                try
                {
                    Process appProc = new System.Diagnostics.Process();
                    appProc.StartInfo.FileName = appPath;
                    appProc.StartInfo.UseShellExecute = false;
                    appProc.Start();
                }
                catch (System.Exception)
                {
                    // Do Nothing
                }
            }
        }

        private void killIEProcesses()
        {
            ieProcessChains.Clear();
            List<Process> invalidIEProcesses = new List<Process>();
            foreach (var process in Process.GetProcessesByName(IE_PROCESS_NAME))
            {
                ieProcessChains.addProcess(process.Id, ParentProcessUtilities.GetParentProcess(process.Id).Id);
                invalidIEProcesses.Add(process);
            }
            foreach (List<int> ieProcessChain in ieProcessChains.GetProcessChains())
            {
                if (PATRON_UI_PROCESS_NAME.Equals(Process.GetProcessById(ieProcessChain[0]).ProcessName))
                {
                    for (int i = 1; i < ieProcessChain.Count; i++)
                        invalidIEProcesses.Remove(Process.GetProcessById(ieProcessChain[i]));
                }
            }
            foreach (Process invalidIEProcess in invalidIEProcesses)
            {
                invalidIEProcess.Kill();
            }
        }

        private void turnOffAllLockKeys()
        {
            LockKeyUtil.CapslockOff();
            LockKeyUtil.NumlockOff();
            LockKeyUtil.ScrolllockOff();
        }

        private int screenShotInterval = 30;
        private void saveScreenShot()
        {
            if (screenShotInterval > 0)
            {
                screenShotInterval--;
                return;
            }
            ScreenShotUtil.SaveScreenShots(Config.GetScreenshotPath());
            screenShotInterval = 30;
        }

        private void doHeartbeat()
        {
            try
            {
                turnOffAllLockKeys();
                killIEProcesses();
                saveScreenShot();
            }
            catch (System.Exception)
            {
                // Do Nothing
            }
        }

        private void heartbeat()
        {
            while(!stopHeartbeat)
            {
                doHeartbeat();
                Thread.Sleep(secInterval);
            }
        }

        private void exit()
        {
            stopHeartbeat = true;
            if (heartbeatThread.IsAlive)
                heartbeatThread.Join();
            Application.Exit();
            mutex.ReleaseMutex();
        }

        protected void prepareTrayIcon()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "SCLauncher";
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
            exit();
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
