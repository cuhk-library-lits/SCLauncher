using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;

namespace CUHKSelfCheckLauncher
{
	public static class Config
    {
        public const string APP_NAME = @"SCLauncher";
        public const string INI_FILE_PATH = @"config.ini";
        public const string RESOURCES_PATH = @"resources\";

        public const string INI_SECT_SELF_CHECK_CONFIG = @"3M Self Check";
        public const string INI_KEY_SELF_CHECK_BIN_PATH = @"BIN_PATH";
        public const string INI_KEY_SELF_CHECK_DISABLE_IE_PROCESS = @"DISABLE_IE_PROCESS";
        public const string INI_KEY_SELF_CHECK_AUTH_DISABLED_SMC = @"AUTH_DISABLED_SMC";
        public const string INI_KEY_SELF_CHECK_AUTH_CUHKLOGIN_SMC = @"AUTH_CUHKLOGIN_SMC";
        public const string INI_KEY_SELF_CHECK_AUTH_MODE = @"AUTH_MODE";
        
        public const string INI_SECT_STUNNEL = @"stunnel";
        public const string INI_KEY_STUNNEL_PATH = @"PATH";

        public const string INI_SECT_LAUNCH = @"launch";
        public const string INI_KEY_APP_PATH = @"APP_PATH";

        public const string INI_SECT_SYSTEM = @"system";
        public const string INI_KEY_DAILY_REBOOT_TIME_HHMM = @"DAILY_REBOOT_TIME_HHMM";
        public const string INI_KEY_SCREENSHOT_DPI_SCALING = @"SCREENSHOT_DPI_SCALING";

        public const string SELF_CHECK_AUTH_MODE_DISABLED = @"DISABLED";
        public const string SELF_CHECK_AUTH_MODE_CUHK_LOGIN = @"CUHKLOGIN";
        public const string PEM_FILE_NAME = @"client.pem";
        public const string STUNNEL_EXE_NAME = @"stunnel.exe";

        public const string SCREENSHOTS_FOLDER = @"screenshots";

        static string scLauncherPath = null;
        static string binPath = null;
        static string disableIEProcess = null;
        static string authDisabledSmc = null;
        static string authCUHKLoginSmc = null;
        static string authMode = null;
        static string stunnelPath = null;
        static string stunnelBinPath = null;
        static string stunnelConfigPath = null;
        static List<string> launchAppPaths = new List<string>();
        static DateTime rebootDateTime = DateTime.ParseExact("19000101000000", "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        static double screenshotDpiScaling = 1.0;

        static Config()
        {
            try
            {
                scLauncherPath = AppDomain.CurrentDomain.BaseDirectory;
                if (!scLauncherPath.EndsWith(@"\"))
                    scLauncherPath = scLauncherPath + @"\";
                
                TraceListener traceListener = new TextWriterTraceListener(scLauncherPath + APP_NAME + ".log");
                traceListener.TraceOutputOptions |= TraceOptions.DateTime;
                traceListener.TraceOutputOptions |= TraceOptions.Callstack;
                Trace.Listeners.Add(traceListener);
                Trace.AutoFlush = true;

                binPath = IniFileUtil.ReadValue(INI_SECT_SELF_CHECK_CONFIG, INI_KEY_SELF_CHECK_BIN_PATH, scLauncherPath + INI_FILE_PATH, @"");
                if (!binPath.EndsWith(@"\"))
                    binPath = binPath + @"\";
                
                disableIEProcess = IniFileUtil.ReadValue(INI_SECT_SELF_CHECK_CONFIG, INI_KEY_SELF_CHECK_DISABLE_IE_PROCESS, scLauncherPath + INI_FILE_PATH, @"");

                authDisabledSmc = IniFileUtil.ReadValue(INI_SECT_SELF_CHECK_CONFIG, INI_KEY_SELF_CHECK_AUTH_DISABLED_SMC, scLauncherPath + INI_FILE_PATH, @"");
                authCUHKLoginSmc = IniFileUtil.ReadValue(INI_SECT_SELF_CHECK_CONFIG, INI_KEY_SELF_CHECK_AUTH_CUHKLOGIN_SMC, scLauncherPath + INI_FILE_PATH, @"");

                authMode =  IniFileUtil.ReadValue(INI_SECT_SELF_CHECK_CONFIG, INI_KEY_SELF_CHECK_AUTH_MODE, scLauncherPath + INI_FILE_PATH, @"");
                if (String.IsNullOrEmpty(authMode))
                    authMode = SELF_CHECK_AUTH_MODE_DISABLED;
                
                stunnelPath = IniFileUtil.ReadValue(INI_SECT_STUNNEL, INI_KEY_STUNNEL_PATH, scLauncherPath + INI_FILE_PATH, @"");
                if (!stunnelPath.EndsWith(@"\"))
                    stunnelPath = stunnelPath + @"\";
                
                stunnelBinPath = stunnelPath + @"bin\";
                stunnelConfigPath = stunnelPath + @"config\";

                string[] launchKeyValues = IniFileUtil.ReadKeyValuePairs(INI_SECT_LAUNCH, scLauncherPath + INI_FILE_PATH);
                for (int i=0; launchKeyValues != null && i<launchKeyValues.Length; i++)
                {
                    if (String.IsNullOrEmpty(launchKeyValues[i]))
                        continue;
                    
                    int idx = launchKeyValues[i].IndexOf(@"=");
                    string key = launchKeyValues[i].Substring(0, idx);
                    string value = launchKeyValues[i].Substring(idx + 1);
                    if (INI_KEY_APP_PATH == key)
                        launchAppPaths.Add(value);
                }

                string dailyRebootTimeStr = IniFileUtil.ReadValue(INI_SECT_SYSTEM, INI_KEY_DAILY_REBOOT_TIME_HHMM, scLauncherPath + INI_FILE_PATH, @"");
                if (!String.IsNullOrEmpty(dailyRebootTimeStr))
                {
                    string rebootDateTimeStr = DateTime.Now.ToString("yyyyMMdd");
                    rebootDateTimeStr += dailyRebootTimeStr;
                    rebootDateTimeStr += "00";
                    rebootDateTime = DateTime.ParseExact(rebootDateTimeStr, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                }

                string screenshotDpiScalingStr = IniFileUtil.ReadValue(INI_SECT_SYSTEM, INI_KEY_SCREENSHOT_DPI_SCALING, scLauncherPath + INI_FILE_PATH, @"");
                screenshotDpiScaling = double.Parse(screenshotDpiScalingStr, CultureInfo.InvariantCulture.NumberFormat);
                    
            }
            catch (System.Exception)
            {
                // Do Nothing
            }
        }

        public static void SetupSelfCheckAuthMode()
        {
            if (SELF_CHECK_AUTH_MODE_DISABLED == authMode)
            {
                if (File.Exists(binPath + authCUHKLoginSmc))
                    File.Delete(binPath + authCUHKLoginSmc);

                if (!File.Exists(binPath + authDisabledSmc))
                    File.Copy(scLauncherPath + RESOURCES_PATH + authDisabledSmc, binPath + authDisabledSmc, true);

                return;
            }

            if (SELF_CHECK_AUTH_MODE_CUHK_LOGIN == authMode)
            {
                if (File.Exists(binPath + authDisabledSmc))
                    File.Delete(binPath + authDisabledSmc);

                if (!File.Exists(binPath + authCUHKLoginSmc))
                    File.Copy(scLauncherPath + RESOURCES_PATH + authCUHKLoginSmc, binPath + authCUHKLoginSmc, true);

                return;
            }
        }

        public static string GetDisableIEProcess()
        {
            return disableIEProcess;
        }

        public static string GetSTunnelExe()
        {
            return stunnelBinPath + STUNNEL_EXE_NAME;
        }

        public static void SetupSTunnel()
        {
            string ipAddress = IPUtils.GetAllLocalIPv4(NetworkInterfaceType.Ethernet).FirstOrDefault();
            string terminalPEMFile = scLauncherPath + RESOURCES_PATH + ipAddress + @".pem";
            if (!File.Exists(terminalPEMFile))
            {
                MessageBox.Show("File not found: " + terminalPEMFile);
                return;
            }

            if (File.Exists(stunnelConfigPath + PEM_FILE_NAME))
                File.Delete(stunnelConfigPath + PEM_FILE_NAME);

            File.Copy(terminalPEMFile, stunnelConfigPath + PEM_FILE_NAME, true);
        }

        public static List<string> GetLaunchApps()
        {
            return launchAppPaths;
        }

        public static string GetScreenshotPath()
        {
            return scLauncherPath + SCREENSHOTS_FOLDER;
        }

        public static DateTime GetRebootDateTime()
        {
            return rebootDateTime;
        }

        public static double GetScreenshotDpiScaling()
        {
            return screenshotDpiScaling;
        }
    }
}