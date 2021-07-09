namespace ReadySunValley.Assessment
{
    public class Inet
    {
        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public bool isINet()
        {
            return InternetGetConnectedState(out _, 0);
        }
    }
}