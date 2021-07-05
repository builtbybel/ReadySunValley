using System.Diagnostics;

namespace ReadySunValley.Assessment
{
    public static class Boot
    {
        public static string IsUEFI()
        {
            // Compatible with >= Win8.1
            string env = "echo %firmware_type%";           

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
    }
}