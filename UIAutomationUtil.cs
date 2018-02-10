using System;
using System.Diagnostics;
using System.Windows.Automation;

namespace CUHKSelfCheckLauncher
{
    public class UIAutomationUtil
    {
        public static void CloseMainWindow (Process process)
        {
            if (process == null)
                throw new ArgumentNullException ("process");

            if (process.MainWindowHandle == IntPtr.Zero)
                return;

            AutomationElement element = AutomationElement.FromHandle (process.MainWindowHandle);
            WindowPattern windowPattern = null;
            try
            {
                windowPattern = element.GetCurrentPattern(WindowPattern.Pattern) as WindowPattern;
                if (windowPattern.WaitForInputIdle(10000))
                    windowPattern.Close();
            }
            catch (InvalidOperationException)
            {
                // object doesn't support the WindowPattern control pattern
            }
        }
        
        public static string GetChromeUrl (Process process)
        {
            if (process == null)
                throw new ArgumentNullException ("process");

            if (process.MainWindowHandle == IntPtr.Zero)
                return null;

            AutomationElement element = AutomationElement.FromHandle (process.MainWindowHandle);
            if (element == null)
                return null;

            AutomationElement edit = element.FindFirst (TreeScope.Children, new PropertyCondition (AutomationElement.ControlTypeProperty, ControlType.Edit));
            return ((ValuePattern) edit.GetCurrentPattern (ValuePattern.Pattern)).Current.Value as string;
        }

        public static string GetInternetExplorerUrl (Process process)
        {
            if (process == null)
                throw new ArgumentNullException ("process");

            if (process.MainWindowHandle == IntPtr.Zero)
                return null;

            AutomationElement element = AutomationElement.FromHandle (process.MainWindowHandle);
            if (element == null)
                return null;

            AutomationElement rebar = element.FindFirst (TreeScope.Children, new PropertyCondition (AutomationElement.ClassNameProperty, "ReBarWindow32"));
            if (rebar == null)
                return null;

            AutomationElement edit = rebar.FindFirst (TreeScope.Subtree, new PropertyCondition (AutomationElement.ControlTypeProperty, ControlType.Edit));

            return ((ValuePattern) edit.GetCurrentPattern (ValuePattern.Pattern)).Current.Value as string;
        }

        public static string GetFirefoxUrl (Process process)
        {
            if (process == null)
                throw new ArgumentNullException ("process");

            if (process.MainWindowHandle == IntPtr.Zero)
                return null;

            AutomationElement element = AutomationElement.FromHandle (process.MainWindowHandle);
            if (element == null)
                return null;

            AutomationElement doc = element.FindFirst (TreeScope.Subtree, new PropertyCondition (AutomationElement.ControlTypeProperty, ControlType.Document));
            if (doc == null)
                return null;

            return ((ValuePattern) doc.GetCurrentPattern (ValuePattern.Pattern)).Current.Value as string;
        }

    }
}