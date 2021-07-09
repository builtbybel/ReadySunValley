using System;
using System.Diagnostics;
using System.Text;

namespace ReadySunValley.Assessment
{
    public class Bypass
    {
        public void Windows11(string resource)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;

            // Create temp filepath
            string tempPath = System.IO.Path.GetTempPath() + @"\ReadySunValley" + Guid.NewGuid() + ".reg";
            System.IO.StreamWriter sW = new System.IO.StreamWriter(tempPath, false, Encoding.Unicode);
            sW.Write(resource);
            sW.Close();

            // Reg import bypass.reg
            p.StartInfo.FileName = "REG";
            p.StartInfo.Arguments = "IMPORT \"" + tempPath + "\"";
            if (Environment.Is64BitOperatingSystem) p.StartInfo.Arguments += " /reg:64";

            p.Start();
            p.WaitForExit();
            System.IO.File.Delete(tempPath);
        }
    }
}