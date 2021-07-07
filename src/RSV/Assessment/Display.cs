using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ReadySunValley.Assessment
{
    public static class Display
    {
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public static string MonitorSize()
        {
            string size = string.Empty;

            Graphics graphics = Graphics.FromHwnd(IntPtr.Zero); 
            IntPtr desktop = graphics.GetHdc();
            int monitorHeight = GetDeviceCaps(desktop, 6);
            int monitorWidth = GetDeviceCaps(desktop, 4);
            size = $"{Math.Sqrt(Math.Pow(monitorHeight, 2) + Math.Pow(monitorWidth, 2)) / 25,4:#,##0.00}";

            return size;
        }
    }
}