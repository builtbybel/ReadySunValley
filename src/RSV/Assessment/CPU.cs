namespace ReadySunValley.Assessment
{
    public static class CPU
    {
        public static string Architecture()
        {
            string result = string.Empty;

            switch (typeof(string).Assembly.GetName().ProcessorArchitecture)
            {
                case System.Reflection.ProcessorArchitecture.X86:

                    result = "32 Bit";
                    break;

                case System.Reflection.ProcessorArchitecture.Amd64:
                    result = "64 Bit";

                    break;

                case System.Reflection.ProcessorArchitecture.Arm:
                    result = "ARM";
                    break;
            }

            return result;
        }

        public static string ClockSpeed()
        {
            string clockSpeed = "";
            foreach (var item in new System.Management.ManagementObjectSearcher("select MaxClockSpeed from Win32_Processor").Get())
            {
                var clockSpeedx = (uint)item["MaxClockSpeed"];
                clockSpeed = clockSpeedx.ToString();
            }
            return clockSpeed;
        }
    }
}