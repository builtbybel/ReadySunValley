using Microsoft.Win32;

namespace ReadySunValley.Assessment
{
    public static class SecureBoot
    {
        public static string SecureBootStatus()
        {
            int rc = 0;
            string key = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecureBoot\State";
            string subkey = @"UEFISecureBootEnabled";
            try
            {
                object value = Registry.GetValue(key, subkey, rc);
                if (value != null)
                    rc = (int)value;
            }
            catch { }
            return $@"{(rc >= 1 ? "ON" : "OFF")}";
        }
    }
}