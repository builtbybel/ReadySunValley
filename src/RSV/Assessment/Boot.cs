using Microsoft.Win32;
using System.Diagnostics;
using System.Linq;

namespace ReadySunValley.Assessment
{
    public static class Boot
    {
        public static string IsUEFI()
        {
            string env = "echo %firmware_type%";         // Compatible with >= Win8.1

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/C" + env;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output;
        }

        /* On systems with UEFI the registry key HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecureBoot\State\UEFISecureBootEnabled should be present
         In the Non-UEFI case this key is not present */

        public static bool IsSecureBoot()
        {
            try
            {
                RegistryKey UEFIKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State", true);

                if (UEFIKey != null)
                {
                    return (UEFIKey.GetValueNames().Contains("UEFISecureBootEnabled"));
                }
            }
            catch { }
            return false;
        }
    }
}