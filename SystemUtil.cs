#define TRACE
using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Globalization;

namespace CUHKSelfCheckLauncher
{
    public class SystemUtil
    {
        static DateTime launchDateTime;
        static DateTime rebootDateTime;

        public static void Init()
        {
            launchDateTime = DateTime.Now;
            rebootDateTime = Config.GetRebootDateTime();
        }

        public static void DailyReboot()
        {
            DateTime now = DateTime.Now;
            if (launchDateTime < rebootDateTime && now >= rebootDateTime)
                StartProcess(@"shutdown.exe", @" /r /f /t 0", true);
        }

        public static void StartProcess(string path, string arguments, bool hideWindow)
        {
            try
            {
                Process appProc = new System.Diagnostics.Process();
                appProc.StartInfo.FileName = path;
                if (!String.IsNullOrEmpty(arguments))
                    appProc.StartInfo.Arguments = arguments;
                if (hideWindow)
                    appProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                appProc.StartInfo.UseShellExecute = false;
                appProc.Start();
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }
        }

        public static void SaveScreenShots(string saveFilePath)
        {
            if (!saveFilePath.EndsWith(@"\"))
                saveFilePath += @"\";

            if(!Directory.Exists(saveFilePath))
                Directory.CreateDirectory(saveFilePath);

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            int screenID = 1;
            foreach (Screen screen in Screen.AllScreens)
            {
                double scaling = Config.GetScreenshotDpiScaling();
                Rectangle screenRect = screen.Bounds;
                screenRect.Width = (int)(screenRect.Width * scaling);
                screenRect.Height = (int)(screenRect.Height * scaling);

                Bitmap bmpScreenshot = new Bitmap(screenRect.Width, screenRect.Height, PixelFormat.Format32bppArgb);

                using (Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot))
                {
                    gfxScreenshot.CopyFromScreen(screenRect.X, screenRect.Y, 0, 0, screenRect.Size, CopyPixelOperation.SourceCopy);
                    gfxScreenshot.SmoothingMode = SmoothingMode.AntiAlias;
                    gfxScreenshot.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfxScreenshot.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    RectangleF launchTimeRect = new RectangleF(0, bmpScreenshot.Height - 10, 130, 10);
                    gfxScreenshot.FillRectangle(Brushes.AliceBlue, launchTimeRect);
                    gfxScreenshot.DrawString("Start: " + launchDateTime.ToString("yyyy-MM-dd HH:mm:ss"), new Font("Tahoma", 7), Brushes.Black, launchTimeRect);

                    RectangleF screenShotTimeRect = new RectangleF(bmpScreenshot.Width - 95, bmpScreenshot.Height - 10, 95, 10);
                    gfxScreenshot.FillRectangle(Brushes.AliceBlue, screenShotTimeRect);
                    gfxScreenshot.DrawString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), new Font("Tahoma", 7), Brushes.Black, screenShotTimeRect);
                }
                
                bmpScreenshot.Save(saveFilePath + screenID + ".png", ImageFormat.Png);
                screenID++;
            }
        }
    }
}
