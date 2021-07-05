namespace ReadySunValley.Assessment
{
    public static class Inet
    {
        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool isINet()
        {
            return InternetGetConnectedState(out _, 0);
        }
    }
}