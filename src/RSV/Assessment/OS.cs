using Microsoft.Win32;
using System;

namespace ReadySunValley.Assessment
{
    public class OS
    {
        public string ComputerName { get; set; }

        public string GetOS()
        {
            ComputerName = Environment.MachineName;

            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            int osbuild = Convert.ToInt32(key.GetValue("CurrentBuildNumber"));
            if (osbuild >= 22000)
            {
                return ("Windows 11 aka Sun Valley");
            }
            else return (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", "");
        }

        public string GetVersion()
        {
            return (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "DisplayVersion", "");
        }

        public string Is64Bit()
        {
            string bitness = string.Empty;

            if (Environment.Is64BitOperatingSystem)
            {
                bitness = "64bit";
            }
            else
            {
                bitness = "32bit";
            }

            return bitness;
        }
    }
}