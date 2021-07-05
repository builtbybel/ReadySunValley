using System;
using System.Text;

namespace ReadySunValley.Assessment
{
    public static class Bypass
    {
        public static void Windows11(string resource)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();

            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;

            // Create temp filepath
            string tempPath = System.IO.Path.GetTempPath() + @"\ReadySunValley" + Guid.NewGuid() + ".reg";
            System.IO.StreamWriter sW = new System.IO.StreamWriter(tempPath, false, Encoding.Unicode);
            sW.Write(resource);
            sW.Close();

            // Reg import bypass.reg
            proc.StartInfo.FileName = "REG";
            proc.StartInfo.Arguments = "IMPORT \"" + tempPath + "\"";
            if (Environment.Is64BitOperatingSystem) proc.StartInfo.Arguments += " /reg:64";

            proc.Start();
            proc.WaitForExit();
            System.IO.File.Delete(tempPath);
        }
    }
}