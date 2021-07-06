using Microsoft.Win32;
using System;

namespace ReadySunValley.Assessment
{
    public static class OS
    {
        public static string GetOS()
        {
            return (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", "");
        }

        public static string Is64Bit()
        {
            string bitness = string.Empty;

            if (Environment.Is64BitOperatingSystem)
            {
                bitness = "64";
            }
            else
            {
                bitness = "32";
            }

            return bitness;
        }
    }
}