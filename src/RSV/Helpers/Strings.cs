namespace ReadySunValley.Helpers
{
    internal class Strings
    {
        public class Titles
        {
            public static readonly string AppName = "ReadySunValley" + " (" + Assessment.OS.GetOS() + Assessment.OS.Is64Bit() + ")";
        }

        public class Body
        {
            public static readonly string AppInfo = "ReadySunValley" + "\nVersion " + Program.GetCurrentVersionTostring() +
                                                    "\n\nChecks if your device is ready for Windows 11/Sun Valley update.\r\n\n" +
                                                    "This project was forked initially from " + Helpers.Strings.Uri.CreditsRepo + "\r\n\n" +
                                                    "You can also reach out to me on\n" +
                                                    "\ttwitter.com/builtbybel\r\n\n" +
                                                    "(C) 2021, Builtbybel";

            public static readonly string Bypass = "If you are attempting to install Windows 11 and receive a message stating," +
                                                  "\"This PC can't run Windows 11\" it is likely that you do not have a TPM 2.0 requirement, Secure Boot or 4GB of RAM." +
                                                  "\n\nThe good news is that Microsoft includes a new \"LabConfig\" registry key that allows you to configure settings to bypass the TPM 2.0, the 4GB memory," +
                                                  "and Secure Boot requirements.\n\nPlease note, that by disabling the TPM 2.0 requirement, you are effectively reducing the security in Windows 11." +
                                                  "\n\nDo you want to bypass these restrictions?";

            public static readonly string BypassOK = "TPM and SecureBoot restriction has been bypassed.";
            public static readonly string BypassUndo = "System setttings are in place again.";
        }

        public class Hover
        {
            public static readonly string Recheck = "Run check again";
            public static readonly string AssetInfo = "Open @github.com/builtbybel/readysunvalley";

            public static readonly string CPUInfo = "Your CPU meets the soft requirements, it's just not listed on the offical list of supported processors.";
            public static readonly string CPUBad = "Your CPU doesn't meet the specification requirements, see individual info about frequency or cores below.";
            public static readonly string BootBad = "Your system needs to support a UEFI boot mode, right now your system is booting using Legacy. This doesn't necessarily mean that your system doesn't support it.Check your motherboard, system manual or bios for more information.";
            public static readonly string FreqBad = "Your CPU frequency doesn't meet the minimum requirements for Windows 11.";
            public static readonly string CoresBad = "You don't have enough processing cores to run Windows 11.";
            public static readonly string PartBad = "Your system needs to support GPT partition types, right now your system is booting using MBR. This doesn't necessarily mean that your system doesn't support it. Check your motherboard, system manual or bios for more information.";
            public static readonly string ScreenBad = "One or more of your monitors are too small to work on Windows 11.";
            public static readonly string RAMBad = "Your RAM does not meet the minimum requirements for Windows 11.";
            public static readonly string HDDBad = "Your drive does not have enough capacity to run Windows 11.";
            public static readonly string FreeSpaceInfo = "You don't have enough free space per the requirements, this doesn't mean you don't have enough total space. Just keep in mind Windows 11 requires at least 64GB of available space.";
            public static readonly string DirectXBad = "Your DirectX version is too low. This doesn't necessarily mean that your system doesn't support higher versions. Check DXDIAG for more information.";
            public static readonly string WDDMBad = "Your Windows Display Driver Model version does not meet the minimum requirements for Windows 11.";
            public static readonly string TPMInfo = "Your TPM version is too low. If you’re running an older version of TPM (1.2 typically), then you may be able to update it to TPM 2.0 with a firmware update.";
            public static readonly string TPMBad = "If no TPM is present, you’ll probably find it’s been disabled in the UEFI.";
            public static readonly string SecureBootBad = "Secure boot is disabled, or functionality is missing. This doesn't necessarily mean that your system doesn't support it. Check your motherboard, system manual, or bios for more information.";
            public static readonly string InetBad = "Windows 11 Home edition requires internet connectivity and a Microsoft account to complete device setup on first use. Switching a device out of Windows 11 Home in S mode also requires internet connectivity.";
        }

        public static class Uri
        {
            public const string GitRepo = "https://github.com/builtbybel/ReadySunValley/releases";
            public const string GitUpdateRepo = "https://github.com/builtbybel/readysunvalley/releases/tag/";
            public const string GitVersionCheck = "https://raw.githubusercontent.com/builtbybel/readysunvalley/master/appversion.txt";
            public const string GitVersionHint = "https://raw.githubusercontent.com/builtbybel/ReadySunValley/main/changes.txt";

            public const string CompareMS = "https://github.com/builtbybel/ReadySunValley/blob/main/assets/rsv-microsoft-requirements.png?raw=true";
            public const string CompareUtil = "https://github.com/rcmaehl/WhyNotWin11/releases/download/";
            public const string UtilVersionCheck = "https://raw.githubusercontent.com/builtbybel/ReadySunValley/main/utilversion.txt";

            public const string CreditsRepo = "https://github.com/mag-nif-i-cent/Affinity11";
            public const string VotePage = "https://www.builtbybel.com/blog/19-apps/41-check-with-the-readysunvalley-app-if-your-device-works-with-windows11-sun-valley-update";

            public const string ShareTwitter = "https://twitter.com/intent/tweet?text=Ready%20for%20%23Windows11/Sun%20Valley%20update?%20Here%20are%20my%20results%20%23ReadySunValley%20%23app";
        }
    }
}