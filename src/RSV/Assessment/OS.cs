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

            return (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", "");
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