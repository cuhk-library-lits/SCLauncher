using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace CUHKSelfCheckLauncher
{
    public class ScreenShotHelper
    {
        public static void SaveScreenShots(string saveFilePath)
        {
            if (!saveFilePath.EndsWith(@"\"))
                saveFilePath += @"\";

            if(!Directory.Exists(saveFilePath))
                Directory.CreateDirectory(saveFilePath);

            int screenID = 1;
            foreach (Screen screen in Screen.AllScreens)
            {
                Bitmap bmpScreenshot = new Bitmap(screen.Bounds.Width, screen.Bounds.Height, PixelFormat.Format32bppArgb);
                Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                gfxScreenshot.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, 0, 0, screen.Bounds.Size, CopyPixelOperation.SourceCopy);
                gfxScreenshot.SmoothingMode = SmoothingMode.AntiAlias;
                gfxScreenshot.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gfxScreenshot.PixelOffsetMode = PixelOffsetMode.HighQuality;

                RectangleF rectf = new RectangleF(bmpScreenshot.Width - 90, bmpScreenshot.Height - 10, bmpScreenshot.Width, bmpScreenshot.Height);
                gfxScreenshot.FillRectangle(Brushes.AliceBlue, rectf);
                gfxScreenshot.DrawString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), new Font("Tahoma", 7), Brushes.Black, rectf);
                
                bmpScreenshot.Save(saveFilePath + screenID + ".png", ImageFormat.Png);
                screenID++;
            }
        }
    }
}
