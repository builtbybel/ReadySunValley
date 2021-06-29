using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Moin11
{
    public partial class MainWindow : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        public const int ERROR_INVALID_FUNCTION = 1;

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

        [DllImport("kernel32.dll",
            EntryPoint = "GetFirmwareEnvironmentVariableA",
            SetLastError = true,
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int GetFirmwareType(string lpName, string lpGUID, IntPtr pBuffer, uint size);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
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

        private static T GetProperty<T>(IDxDiagContainer container, string propName)
        {
            container.GetProp(propName, out object variant);
            return (T)Convert.ChangeType(variant, typeof(T));
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

            var bit = Environment.Is64BitOperatingSystem ? "64bit" : "32bit";
            LblSystem.Text = "My OS version is " + RuntimeInformation.OSDescription + bit;
            LblAppVersion.Text = Program.GetCurrentVersionTostring();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bool bypassTPM = false;
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!hasAdministrativeRight)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                };
                try
                {
                    Process p = Process.Start(startInfo);
                    Environment.Exit(-1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            int performCompatibilityCount = 0;

            StatusWindow LoadingForm = new StatusWindow();
            LoadingForm.Show();

            LoadingForm.StatusText = "Checking system requirements... [1/9]";
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
            }
            LoadingForm.StatusText = "Checking CPU speed... [2/9]";
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
            LoadingForm.StatusText = "Getting core counts... [3/9]";
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
            LoadingForm.StatusText = "Checking CPU Compatibility... [4/9]";
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
            LoadingForm.StatusText = "Checking Partition Types... [5/9]";
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

            LoadingForm.StatusText = "Checking Secure Boot Status... [6/9]";
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
            long ram = 0;
            LoadingForm.StatusText = "Checking RAM Compatibility... [7/9]";
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
            LoadingForm.StatusText = "Checking disk size... [8/9]";
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

            LoadingForm.StatusText = "Getting DirectX && WDDM info... [9/9]";
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
            }

            if (bypassTPM)
            {
                tpminfo.Visible = false;
                lbl_tpm.Text = "Cannot get TPM info without admin privileges. Run as admin and try again.";
                LoadingForm.Hide();
            }
            else
            {
                LoadingForm.StatusText = "Getting TPM...";

                System.Diagnostics.Process.Start("tpm.msc");

                LoadingForm.Hide();
            }

            var sum = performCompatibilityCount;
            LblBadCompatibilty.Text = sum.ToString();

            if (sum <= 0)
            {
                LblBadCompatibilty.ForeColor = Color.Green;
            }
            else
            {
                LblBadCompatibilty.ForeColor = Color.Red;
            }
        }

        private void CaptureScreen()
        {
            Form f = ActiveForm;
            Bitmap bmp = new Bitmap(f.Width, f.Height);
            f.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = Application.StartupPath;
            dialog.Title = "Location";
            dialog.Filter = "PNG Images|*.png";
            dialog.FileName = "Compatibility-Screen-Win11";

            DialogResult result = dialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                bmp.Save(dialog.FileName);
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
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

        private void LnkOpenGitHub_Click(object sender, EventArgs e) => Process.Start("https://github.com/builtbybel/moin-11/releases");

        private void BtnCapture_Click(object sender, EventArgs e) => CaptureScreen();

        private void LnkTPMStatus_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start("tpm.msc");
    }
}