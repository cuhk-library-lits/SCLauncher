#define TRACE
using System.Threading;
using System.Windows.Forms;

using System;
using System.Drawing;


using System.Diagnostics;
using System.Collections.Generic;
using System.ServiceProcess;
using System.IO;

namespace CUHKSelfCheckLauncher
{
    public class SCLauncher : Form
    {
        static Mutex mutex = new Mutex (false, "CUHKSelfCheckLauncher.SCLauncher");
        const string SHELL_PROCESS_NAME = @"SCLauncherShell";
        const int WATCHDOG_HEARTBEAT_INTERVAL = 30000;

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private Thread heartbeatThread = null;
        private bool stopHeartbeat = false;

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
            SystemUtil.StartProcess(Config.GetShellPath(), null, true);
            while(!ShellIsAlive())
                Thread.Sleep(1000);
        }

        private bool ShellIsAlive()
        {
            return Process.GetProcessesByName(SHELL_PROCESS_NAME).Length > 0;
        }

        private void RebootOnShellExit()
        {
            if (!ShellIsAlive())
            {
                Trace.TraceError("SCLauncherShell process terminated unexpectedly! Rebooting...");
                SystemUtil.StartProcess(@"shutdown.exe", @" /r /f /t 0", true);
            }
        }

        private void DoHeartbeat()
        {
            try
            {
                RebootOnShellExit();
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
                Thread.Sleep(WATCHDOG_HEARTBEAT_INTERVAL);
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
