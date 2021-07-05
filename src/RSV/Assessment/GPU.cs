using System;
using System.Management;
using System.Windows.Forms;

namespace ReadySunValley.Assessment
{
    public static class GPU
    {
        public static string Unit()
        {
            string result = string.Empty;
            try
            {
                ManagementObjectSearcher graphics = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");

                string gpu = string.Empty;
                foreach (ManagementObject mo in graphics.Get())
                {
                    foreach (PropertyData property in mo.Properties)
                    {
                        if (property.Name == "Description")
                        {
                            result = property.Value.ToString();
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            return result;
        }
    }
}