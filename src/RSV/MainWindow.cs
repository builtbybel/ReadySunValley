using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace ReadySunValley
{
    public partial class MainWindow : Form
    {
        private readonly string _infoApp = "ReadySunValley" + "\nVersion " + Program.GetCurrentVersionTostring() +
                                    "\n\nChecks if your device is ready for Windows 11/Sun Valley update.\r\n\n" +
                                    "This project was forked from https://github.com/mag-nif-i-cent/Affinity11\r\n\n" +
                                    "You can also reach out to me on\n" +
                                    "\ttwitter.com/builtbybel\r\n\n" +
                                    "(C) 2021, Builtbybel";

        // App update
        private readonly string _releaseURL = "https://raw.githubusercontent.com/builtbybel/readysunvalley/master/appversion.txt";

        public Version CurrentVersion = new Version(Application.ProductVersion);
        public Version LatestVersion;

        //Compare utilty update
        private readonly string _uriUtility = "https://github.com/rcmaehl/WhyNotWin11/releases/download/";

        private readonly string _uriUtilVersion = "https://raw.githubusercontent.com/builtbybel/ReadySunValley/main/utilversion.txt";

        public Version uriUtilLatestVersion;

        //UEFI or legacy mode
        public const int ERROR_INVALID_FUNCTION = 1;

        [DllImport("kernel32.dll",
            EntryPoint = "GetFirmwareEnvironmentVariableA",
            SetLastError = true,
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int GetFirmwareType(string lpName, string lpGUID, IntPtr pBuffer, uint size);

        // Internet conncetion
        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        // Detecting DirectX wrapping COM objects
        [Guid("7D0F462F-4064-4862-BC7F-933E5058C10F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDxDiagContainer
        {
            void EnumChildContainerNames(uint dwIndex, string pwszContainer, uint cchContainer);

            void EnumPropNames(uint dwIndex, string pwszPropName, uint cchPropName);

            void GetChildContainer(string pwszContainer, out IDxDiagContainer ppInstance);

            void GetNumberOfChildContainers(out uint pdwCount);

            void GetNumberOfProps(out uint pdwCount);

            void GetProp(string pwszPropName, out object pvarProp);
        }

        [ComImport]
        [Guid("A65B8071-3BFE-4213-9A5B-491DA4461CA7")]
        public class DxDiagProvider { }

        [Guid("9C6B4CB0-23F8-49CC-A3ED-45A55000A6D2")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDxDiagProvider
        {
            void Initialize(ref DXDIAG_INIT_PARAMS pParams);

            void GetRootContainer(out IDxDiagContainer ppInstance);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DXDIAG_INIT_PARAMS
        {
            public int dwSize;
            public uint dwDxDiagHeaderVersion;
            public bool bAllowWHQLChecks;
            public IntPtr pReserved;
        };

        private void checkAppUpdate()
        {
            try
            {
                WebRequest hreq = WebRequest.Create(_releaseURL);
                hreq.Timeout = 10000;
                hreq.Headers.Set("Cache-Control", "no-cache, no-store, must-revalidate");

                WebResponse hres = hreq.GetResponse();
                StreamReader sr = new StreamReader(hres.GetResponseStream());

                LatestVersion = new Version(sr.ReadToEnd().Trim());

                sr.Dispose();
                hres.Dispose();

                var equals = LatestVersion.CompareTo(CurrentVersion);

                if (equals == 0)
                {
                    return; // up-to-date
                }
                else if (equals < 0)
                {
                    return; // higher than available
                }
                else // new version
                {
                    if (MessageBox.Show("A new app version " + LatestVersion + " is available.\nDo you want to goto the Github update page?\n\nPress <No> to continue with compatibility check.", "App update available", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) // New release available!
                    {
                        Process.Start("https://github.com/builtbybel/readysunvalley/releases/tag/" + LatestVersion);
                    }
                }
            }
            catch { MessageBox.Show("App update check failed...", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private String FormatBytes(long byteCount)
        {
            string[] suf = { " B", " KB", " MB", " GB", " TB", " PB", " EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public static bool isUEFI()
        {
            GetFirmwareType("", "{00000000-0000-0000-0000-000000000000}", IntPtr.Zero, 0);

            if (Marshal.GetLastWin32Error() == ERROR_INVALID_FUNCTION)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool isINet()
        {
            return InternetGetConnectedState(out _, 0);
        }

        public string SecureBootStatus()
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

        public string ClockSpeed()
        {
            string clockSpeed = "";
            foreach (var item in new System.Management.ManagementObjectSearcher("select MaxClockSpeed from Win32_Processor").Get())
            {
                var clockSpeedx = (uint)item["MaxClockSpeed"];
                clockSpeed = clockSpeedx.ToString();
            }
            return clockSpeed;
        }

        private long GetTotalFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.AvailableFreeSpace;
                }
            }
            return -1;
        }

        private long GetTotalSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.TotalSize;
                }
            }
            return -1;
        }

        public MainWindow()
        {
            InitializeComponent();

            DoCompatibilityCheck();

            // GUI options
            LblMainMenu.Text = "\ue700";    // Hamburger Menu
        }

        private void DoCompatibilityCheck()
        {
            int performCompatibilityCount = 0;

            // Some OS Info
            var bit = Environment.Is64BitOperatingSystem ? "64bit" : "32bit";
            LblSystem.Text = "My OS version is " + RuntimeInformation.OSDescription + bit;

            // Compatibility routines
            StatusWindow LoadingForm = new StatusWindow();
            LoadingForm.Show();

            LoadingForm.StatusText = "Checking system requirements [1/10]";
            checkAppUpdate(); // Run here app also update check

            lbl_screen.Text = "";
            screengood.Visible = true;
            screenbad.Visible = false;
            int counter = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("\\root\\wmi", "SELECT * FROM WmiMonitorBasicDisplayParams").Get())
            {
                counter++;
                double width = (byte)item["MaxHorizontalImageSize"] / 2.54;
                double height = (byte)item["MaxVerticalImageSize"] / 2.54;
                double diagonal = Math.Sqrt(width * width + height * height);
                lbl_screen.Text = lbl_screen.Text + counter + ". " + diagonal.ToString("0.00") + " inch ";
                if (diagonal <= 9)
                {
                    screengood.Visible = false;
                    screenbad.Visible = true;

                    performCompatibilityCount += 1;
                }
            }

            if (isUEFI())
            {
                lbl_type.Text = "UEFI";
                bootgood.Visible = true;
                bootbad.Visible = false;
            }
            else
            {
                lbl_type.Text = "Legacy";
                bootgood.Visible = false;
                bootbad.Visible = true;

                performCompatibilityCount += 1;
            }

            LoadingForm.StatusText = "Checking CPU speed [2/10]";
            var clockspeed = ClockSpeed();
            lbl_clockspeed.Text = clockspeed + " MHz Frequency";
            int x = Int32.Parse(clockspeed);
            if (x > 1000)
            {
                freqgood.Visible = true;
                freqbad.Visible = false;
            }
            else
            {
                freqgood.Visible = false;
                freqbad.Visible = true;

                performCompatibilityCount += 1;
            }

            LoadingForm.StatusText = "Getting Core counts [3/10]";
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            lbl_coresnthreads.Text = coreCount + " Cores, " + Environment.ProcessorCount + " Threads";

            if (coreCount > 1)
            {
                coresgood.Visible = true;
                coresbad.Visible = false;
            }
            else
            {
                coresgood.Visible = false;
                coresbad.Visible = true;

                performCompatibilityCount += 1;
            }

            LoadingForm.StatusText = "Checking CPU Compatibility [4/10]";
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_Processor").Get())
            {
                lbl_cpu.Text = item["Name"].ToString();

                var amdbytes = Properties.Resources.amdsupport;
                string amdsupported = System.Text.Encoding.UTF8.GetString(amdbytes);

                var intelbytes = Properties.Resources.intelsupport;
                string intelsupported = System.Text.Encoding.UTF8.GetString(intelbytes);

                string supportedCPUs = amdsupported + "\n" + intelsupported;

                string myCPU = lbl_cpu.Text.ToUpper();

                bool FoundCPU = false;

                foreach (var cpu in supportedCPUs.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))

                    if (myCPU.Contains(cpu.ToUpper()))
                    {
                        FoundCPU = true;
                    }

                if (FoundCPU)
                {
                    cpugood.Visible = true;
                    cpubad.Visible = false;
                    cpuinfo.Visible = false;
                }
                else
                {
                    if (coreCount > 1 && x > 1000)
                    {
                        cpuinfo.Visible = true;
                        cpugood.Visible = false;
                        cpubad.Visible = false;
                    }
                    else
                    {
                        cpugood.Visible = false;
                        cpubad.Visible = true;
                        cpuinfo.Visible = false;

                        performCompatibilityCount += 1;
                    }
                }
            }

            LoadingForm.StatusText = "Checking Partition Types [5/10]";
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_DiskPartition").Get())
            {
                if (item["Type"].ToString().Contains("System"))
                {
                    if (item["Type"].ToString().Contains("GPT"))
                    {
                        lbl_part.Text = "GPT";
                        partgood.Visible = true;
                        partbad.Visible = false;
                    }
                    else
                    {
                        lbl_part.Text = "MBR";
                        partgood.Visible = false;
                        partbad.Visible = true;

                        performCompatibilityCount += 1;
                        break;
                    }
                }
            }

            LoadingForm.StatusText = "Checking Secure Boot Status [6/10]";
            lbl_secureboot.Text = SecureBootStatus();

            if (lbl_secureboot.Text.Contains("ON"))
            {
                securebootgood.Visible = true;
                securebootbad.Visible = false;
            }
            else
            {
                securebootgood.Visible = false;
                securebootbad.Visible = true;

                performCompatibilityCount += 1;
            }

            LoadingForm.StatusText = "Checking RAM Compatibility [7/10]";
            long ram = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_PhysicalMemory").Get())
            {
                string ramstr = item["Capacity"].ToString();
                ram = ram += long.Parse(ramstr);
            }
            lbl_ram.Text = FormatBytes(ram).ToString();

            if (lbl_ram.Text.Contains("GB"))
            {
                string amt = lbl_ram.Text.ToString();
                string[] splitted = amt.Split(' ');
                int ramtotal = int.Parse(splitted[0]);
                if (ramtotal >= 4)
                {
                    ramgood.Visible = true;
                    rambad.Visible = false;
                }
                else
                {
                    ramgood.Visible = false;
                    rambad.Visible = true;

                    performCompatibilityCount += 1;
                }
            }

            LoadingForm.StatusText = "Checking Disk size [8/10]";
            var systemdrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

            long systemfreespace = GetTotalFreeSpace(systemdrive);
            string systemfreespacestr = FormatBytes(systemfreespace).Split(' ')[0];
            Double systemfreespacedouble = Convert.ToDouble(systemfreespacestr);
            lbl_freespace.Text = FormatBytes(systemfreespace).ToString();

            if (systemfreespacedouble >= 64)
            {
                freespacegood.Visible = true;
                freespaceinfo.Visible = false;
            }
            else
            {
                freespacegood.Visible = false;
                freespaceinfo.Visible = true;

                performCompatibilityCount += 1;
            }

            long systemtotalspace = GetTotalSpace(systemdrive);
            string systemspacestr = FormatBytes(systemtotalspace).Split(' ')[0];
            Double systemspacedouble = Convert.ToDouble(systemspacestr);
            lbl_storage.Text = FormatBytes(systemtotalspace).ToString();

            if (systemspacedouble >= 64)
            {
                hddgood.Visible = true;
                hddbad.Visible = false;
            }
            else
            {
                hddgood.Visible = false;
                hddbad.Visible = true;

                performCompatibilityCount += 1;
            }

            LoadingForm.StatusText = "Getting DirectX && WDDM2 info [9/10]";
            try
            {
                Process.Start("dxdiag", "/x dxv.xml");
                while (!File.Exists("dxv.xml"))
                    Thread.Sleep(1000);

                XmlDocument doc = new XmlDocument();
                doc.Load("dxv.xml");
                XmlNode dxd = doc.SelectSingleNode("//DxDiag");
                XmlNode dxv = dxd.SelectSingleNode("//DirectXVersion");
                XmlNode wddmv = dxd.SelectSingleNode("//DriverModel");
                Double directXver = Convert.ToDouble(dxv.InnerText.Split(' ')[1]);
                Double wver = Convert.ToDouble(wddmv.InnerText.Split(' ')[1]);
                lbl_directx.Text = "DirectX " + directXver;
                lbl_wddm.Text = "Version: " + wver;

                if (directXver < 12)
                {
                    directgood.Visible = false;
                    directbad.Visible = true;

                    performCompatibilityCount += 1;
                }
                else
                {
                    directgood.Visible = true;
                    directbad.Visible = false;
                }

                if (wver >= 2)
                {
                    wddmbad.Visible = false;
                    wddmgood.Visible = true;
                    LoadingForm.Hide();
                }
            }
            catch { }

            LoadingForm.StatusText = "Getting Graphics card [9/10]";
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");

                string gpu = string.Empty;
                foreach (ManagementObject mo in searcher.Get())
                {
                    foreach (PropertyData property in mo.Properties)
                    {
                        if (property.Name == "Description")
                        {
                            gpu = property.Value.ToString();
                            lbl_wddm.Text += " (" + gpu + ")";
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }

            // Load tpm.msc
            Process.Start("tpm.msc");
            LoadingForm.Hide();

            LoadingForm.StatusText = "Checking Internet connection [10/10]";
            if (isINet())
            {
                lbl_inet.Text = "Available";
                inetgood.Visible = true;
                inetbad.Visible = false;
            }
            else
            {
                lbl_inet.Text = "No";
                inetgood.Visible = false;
                inetbad.Visible = true;

                performCompatibilityCount += 1;
            }

            // Sum summary
            var sum = performCompatibilityCount;
            LblBadCompatibilty.Text = sum.ToString();

            if (sum <= 0)
            {
                LblBadCompatibilty.ForeColor = Color.Green;
            }
            else
            {
                LblBadCompatibilty.ForeColor = Color.DeepPink;
            }
        }

        private void CaptureScreen()
        {
            Form f = ActiveForm;
            Bitmap bmp = new Bitmap(f.Width, f.Height);
            f.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

            SaveFileDialog dialog = new SaveFileDialog
            {
                InitialDirectory = Application.StartupPath,
                Title = "Location",
                Filter = "PNG Images|*.png",
                FileName = "Compatibility-Screen-Win11"
            };

            DialogResult result = dialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                bmp.Save(dialog.FileName);
            }
        }

        private void cpuinfo_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.cpuinfo, "Your CPU meets the soft requirements, it's just not listed on the offical list of supported processors.");
        }

        private void bootbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.bootbad, "Your system needs to support a UEFI boot mode, right now your system is booting using Legacy. This doesn't necessarily mean that your system doesn't support it. Check your motherboard, system manual or bios for more information.");
        }

        private void tpminfo_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.tpminfo, "The recommended TPM version is 2.0");
        }

        private void cpubad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.cpubad, "Your CPU doesn't meet the specification requirements, see individual info about frequency or cores below.");
        }

        private void freqbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.freqbad, "Your CPU frequency doesn't meet the minimum requirements for Windows 11.");
        }

        private void coresbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.coresbad, "You don't have enough processing cores to run Windows 11.");
        }

        private void partbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.partbad, "Your system needs to support GPT partition types, right now your system is booting using MBR. This doesn't necessarily mean that your system doesn't support it. Check your motherboard, system manual or bios for more information.");
        }

        private void rambad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.rambad, "Your RAM does not meet the minimum requirements for Windows 11.");
        }

        private void hddbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.hddbad, "Your drive does not have enough capacity to run Windows 11.");
        }

        private void directbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.directbad, "Your DirectX version is too low. This doesn't necessarily mean that your system doesn't support higher versions. Check DXDIAG for more information.");
        }

        private void screenbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.screenbad, "One or more of your monitors are too small to work on Windows 11.");
        }

        private void wddmbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.wddmbad, "Your Windows Display Driver Model version does not meet the minimum requirements for Windows 11.");
        }

        private void securebootbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.securebootbad, "Secure boot is disabled, or functionality is missing. This doesn't necessarily mean that your system doesn't support it. Check your motherboard, system manual, or bios for more information.");
        }

        private void freespaceinfo_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.freespaceinfo, "You don't have enough free space per the requirements, this doesn't mean you don't have enough total space. Just keep in mind Windows 11 requires at least 64GB of available space.");
        }

        private void inetbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.inetbad, "Windows 11 Home edition requires internet connectivity and a Microsoft account to complete device setup on first use. Switching a device out of Windows 11 Home in S mode also requires internet connectivity. ");
        }

        private void GetCompareUtil()
        {
            if (MessageBox.Show("Do you want to compare these results with the Utility \"WhyNotWin11\"?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                PBar.Visible = true;

                WebRequest hreq = WebRequest.Create(_uriUtilVersion);
                hreq.Timeout = 10000;
                hreq.Headers.Set("Cache-Control", "no-cache, no-store, must-revalidate");

                WebResponse hres = hreq.GetResponse();
                StreamReader sr = new StreamReader(hres.GetResponseStream());

                uriUtilLatestVersion = new Version(sr.ReadToEnd().Trim());

                sr.Dispose();
                hres.Dispose();

                var pkg = _uriUtility + uriUtilLatestVersion + "/" + "WhyNotWin11.exe";

                try
                {
                    WebClient wc = new WebClient();
                    wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);

                    wc.DownloadFileAsync(new Uri(pkg), @"WhyNotWin11.exe");
                }
                catch (Exception ex)
                { MessageBox.Show(ex.Message, this.Text); }
            }
        }

        public void DownloadProgressChanged(Object sender, DownloadProgressChangedEventArgs e)
        {
            PBar.Value = e.ProgressPercentage;
        }

        public void Completed(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "WhyNotWin11.exe",
                    UseShellExecute = true,
                };
                Process.Start(startInfo);

                PBar.Visible = false;

                MessageBox.Show("Ready! So now put the two apps next to each other and take a look at the results.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text);
                PBar.Visible = false;
            }
        }

        private void LnkOpenGitHub_Click(object sender, EventArgs e) => Process.Start("https://github.com/builtbybel/ReadySunValley/releases");

        private void LnkTPMStatus_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start("tpm.msc");

        private void LblMainMenu_Click(object sender, EventArgs e) => this.MainMenu.Show(Cursor.Position.X, Cursor.Position.Y);

        private void AppInfo_Click(object sender, EventArgs e) => MessageBox.Show(_infoApp, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void AppScreenshot_Click(object sender, EventArgs e) => CaptureScreen();

        private void AppCompare_Click(object sender, EventArgs e) => GetCompareUtil();

        private void AppCheck_Click(object sender, EventArgs e) => DoCompatibilityCheck();

        private void AppHelp_Click(object sender, EventArgs e) => Process.Start("https://www.builtbybel.com/blog/19-apps/41-check-with-the-readysunvalley-app-if-your-device-works-with-windows11-sun-valley-update");
    }
}