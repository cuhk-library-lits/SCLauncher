using System;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace CUHKSelfCheckLauncher
{
    public class LockKeyUtil
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        const int KEYEVENTF_EXTENDEDKEY = 0x1;
        const int KEYEVENTF_KEYUP = 0x2;
        const int VK_CAPITAL = 0x14;
        const int VK_NUMLOCK = 0x90;
        const int VK_SCROLL = 0x91;

        public static void CapslockOff()
        {
            if (Control.IsKeyLocked(Keys.CapsLock))
            {
                keybd_event(VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr)0);
                keybd_event(VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
            }
        }

        public static void NumlockOff()
        {
            if (Control.IsKeyLocked(Keys.NumLock))
            {
                keybd_event(VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr)0);
                keybd_event(VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
            }
        }

        public static void ScrolllockOff()
        {
            if (Control.IsKeyLocked(Keys.Scroll))
            {
                keybd_event(VK_SCROLL, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr)0);
                keybd_event(VK_SCROLL, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
            }
        }
    }
}
