using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
/* using System.Threading;
 using System.Globalization; */

namespace ReadySunValley
{
    public partial class MainWindow : Form
    {
        private Assessment.Boot bootInfo = new Assessment.Boot();
        private Assessment.Bypass bypassInfo = new Assessment.Bypass();
        private Assessment.CPU cpuInfo = new Assessment.CPU();
        private Assessment.Display displayInfo = new Assessment.Display();
        private Assessment.GPU gpuInfo = new Assessment.GPU();
        private Assessment.Inet inetInfo = new Assessment.Inet();
        private Assessment.OS osInfo = new Assessment.OS();
        private Assessment.Storage storageInfo = new Assessment.Storage();

        public MainWindow()
        {
            // Uncomment lower line and add lang code to run localization test
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja");

            InitializeComponent();

            // GUI options
            this.Text += "(" + osInfo.GetOS() + "\x20"
                             + osInfo.GetVersion() + "\x20"
                             + osInfo.Is64Bit() + ")";                  // Title& OS info
            lblMainMenu.Text = "\ue700";                                // Hamburger menu
            btnRecheck.Text = "\ue72c";                                 // Refresh

            // Some tooltip options
            ToolTip tt = new ToolTip();
            tt.AutoPopDelay = 15000;
            tt.IsBalloon = true;
        }

        public void Globalization()
        {
            btnCompareUtil.Text = Locales.Locale.btnCompareUtil;
            btnPnlShareScreen.Text = Locales.Locale.btnPnlShareScreen;
            btnShareScreen.Text = Locales.Locale.btnShareScreen;
            checkCompareMS.Text = Locales.Locale.checkCompareMS;
            lblBitness.Text = Locales.Locale.lblBitness;
            lblBitnessCheck.Text = Locales.Locale.lblBitnessCheck;
            lblBootType.Text = Locales.Locale.lblBootType;
            lblBootTypeCheck.Text = Locales.Locale.lblBootTypeCheck;
            lblCores.Text = Locales.Locale.lblCores;
            lblCoresCheck.Text = Locales.Locale.lblCoresCheck;
            lblCPU.Text = Locales.Locale.lblCPU;
            lblDirectX.Text = Locales.Locale.lblDirectX;
            lblDirectXCheck.Text = Locales.Locale.lblDirectXCheck;
            lblDiskType.Text = Locales.Locale.lblDiskType;
            lnkDiskTypeCheck.Text = Locales.Locale.lnkDiskTypeCheck;
            lblDisplay.Text = Locales.Locale.lblDisplay;
            lblDisplayCheck.Text = Locales.Locale.lblDisplayCheck;
            lblFreeSpace.Text = Locales.Locale.lblFreeSpace;
            lblFreeSpaceCheck.Text = Locales.Locale.lblFreeSpaceCheck;
            lblHeader.Text = Locales.Locale.lblHeader;
            lblInet.Text = Locales.Locale.lblInet;
            lblInetCheck.Text = Locales.Locale.lblInetCheck;
            lblMhz.Text = Locales.Locale.lblMhz;
            lblMhzCheck.Text = Locales.Locale.lblMhzCheck;
            lblRAM.Text = Locales.Locale.lblRAM;
            lblRAMCheck.Text = Locales.Locale.lblRAMCheck;
            lblSecureBoot.Text = Locales.Locale.lblSecureBoot;
            lblSecureBootCheck.Text = Locales.Locale.lblSecureBootCheck;
            lblStatus.Text = Locales.Locale.lblStatus;
            lblStorage.Text = Locales.Locale.lblStorage;
            lblStorageCheck.Text = Locales.Locale.lblStorageCheck;
            lblSubHeader.Text = Locales.Locale.lblSubHeader;
            lblTPM.Text = Locales.Locale.lblTPM;
            lblTPMCheck.Text = Locales.Locale.assessmentTPMFail;
            lblWDDM.Text = Locales.Locale.lblWDDM;
            lblWDDMCheck.Text = Locales.Locale.lblWDDMCheck;
            lnkCompatibilityFix.Text = Locales.Locale.lnkCompatibilityFix;
            menuVoteContent.Text = Locales.Locale.menuVoteContent;
            menuVote.Text = Locales.Locale.menuVote;
            menuBypassUndo.Text = Locales.Locale.menuBypassUndo;
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            // GUI localization
            Globalization();

            // Run Assessments
            DoCompatibilityCheck();
        }

        private void MainWindow_SizeChanged(object sender, EventArgs e)
        {
            int formWidth = this.Width;

            if (formWidth < 880 && pbCompare.Visible == false)
            {
                btnPnlShareScreen.Visible = true;
                btnShareScreen.Visible = false;
            }
            else
            {
                btnShareScreen.Visible = true;
                btnPnlShareScreen.Visible = false;
            }
        }

        private void DoCompatibilityCheck()
        {
            int performCompatibilityCount = 0;

            // Run all the assessments
            this.Enabled = false;

            // First checks
            lblStatus.Text = Locales.Locale.assessmentSystemRequirements;
            Helpers.Utils.AppUpdate();

            // CPU arch
            lblStatus.Text = Locales.Locale.assessmentCPUArchitecture;
            lblBitnessCheck.Text = cpuInfo.Architecture();
            if (lblBitnessCheck.Text == "64 Bit")
            {
                archgood.Visible = true;
                archbad.Visible = false;
            }
            else
            {
                archbad.Visible = true;
                archgood.Visible = false;

                performCompatibilityCount += 1;
            }

            // Display
            lblStatus.Text = Locales.Locale.assessmentDisplay;
            lblDisplayCheck.Text = displayInfo.MonitorSize();

            if (double.Parse(lblDisplayCheck.Text) <= 9)
            {
                lblDisplayCheck.Text += " " + Locales.Locale.assessmentDisplayFormat;
                screengood.Visible = false;
                screenbad.Visible = true;

                performCompatibilityCount += 1;
            }
            else
            {
                lblDisplayCheck.Text += " " + Locales.Locale.assessmentDisplayFormat;
                screengood.Visible = true;
                screenbad.Visible = false;
            }

            // Boot Method
            lblBootTypeCheck.Text = bootInfo.IsUEFI();
            if (lblBootTypeCheck.Text.Contains("UEFI"))
            {
                bootgood.Visible = true;
                bootbad.Visible = false;
            }
            else
            {
                bootgood.Visible = false;
                bootbad.Visible = true;

                performCompatibilityCount += 1;
            }

            // CPU Clock Speed
            lblStatus.Text = Locales.Locale.assessmentCPUSpeed;
            var clockspeed = cpuInfo.ClockSpeed();
            lblMhzCheck.Text = clockspeed + " " + Locales.Locale.assessmentCPUSpeedFormat;
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

            // CPU Core
            lblStatus.Text = Locales.Locale.assessmentCPUCores;
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            lblCoresCheck.Text = coreCount + " " + Locales.Locale.lblCores + ", " + Environment.ProcessorCount + " Threads";

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

            // CPU Compatibility
            lblStatus.Text = Locales.Locale.assessmentCPUCompatibility;
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_Processor").Get())
            {
                lblCPU.Text = item["Name"].ToString();

                var amdbytes = Properties.Resources.amdsupport;
                string amdsupported = System.Text.Encoding.UTF8.GetString(amdbytes);

                var intelbytes = Properties.Resources.intelsupport;
                string intelsupported = System.Text.Encoding.UTF8.GetString(intelbytes);

                var qualcommbytes = Properties.Resources.qualcommsupport;
                string qualcommsupported = System.Text.Encoding.UTF8.GetString(qualcommbytes);

                string supportedCPUs = amdsupported + "\n" + intelsupported + "\n" + qualcommsupported;
                string myCPU = lblCPU.Text.ToUpper();

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

            // Partition Type
            lblStatus.Text = Locales.Locale.assessmentPartitionType;
            bool FoundGPT = false;

            ManagementObjectSearcher partitions = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskPartition");
            foreach (ManagementObject queryObj in partitions.Get())
            {
                if (queryObj["Type"].ToString().Contains("GPT"))
                {
                    lnkDiskTypeCheck.Text = "GPT " + Locales.Locale.lnkDiskTypeInfo;
                    partgood.Visible = true;
                    partbad.Visible = false;
                    FoundGPT = true;
                }
                else
                {
                    lnkDiskTypeCheck.Text = "MBR " + Locales.Locale.lnkDiskTypeInfo;
                    partgood.Visible = false;
                    partbad.Visible = true;
                }
            }
            if (!FoundGPT) performCompatibilityCount += 1;

            // Secure Boot
            lblStatus.Text = Locales.Locale.assessmentSecureBoot;

            if (bootInfo.IsSecureBoot())
            {
                lblSecureBootCheck.Text = Locales.Locale.assessmentSecureBootOK;

                securebootgood.Visible = true;
                securebootbad.Visible = false;
            }
            else
            {
                securebootgood.Visible = false;
                securebootbad.Visible = true;
                lblSecureBootCheck.Text = Locales.Locale.assessmentSecureBootFail;

                performCompatibilityCount += 1;
            }

            // RAM
            lblStatus.Text = Locales.Locale.assessmentRAM;
            long mem = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_PhysicalMemory").Get())
            {
                string ramstr = item["Capacity"].ToString();
                mem += long.Parse(ramstr);
            }
            lblRAMCheck.Text = Helpers.Utils.FormatBytes(mem).ToString();

            if (lblRAMCheck.Text.Contains("GB"))
            {
                string amt = lblRAMCheck.Text.ToString();
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

            // Storage info
            lblStatus.Text = Locales.Locale.assessmentStorage;
            var systemdrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

            long systemfreespace = storageInfo.GetTotalFreeSpace(systemdrive);
            string systemfreespacestr = Helpers.Utils.FormatBytes(systemfreespace).Split(' ')[0];
            Double systemfreespacedouble = Convert.ToDouble(systemfreespacestr);
            lblFreeSpaceCheck.Text = Helpers.Utils.FormatBytes(systemfreespace).ToString();

            if (systemfreespacedouble >= 6) // Free space
            {
                freespacegood.Visible = true;
                freespacebad.Visible = false;
            }
            else
            {
                freespacegood.Visible = false;
                freespacebad.Visible = true;
            }

            long systemtotalspace = storageInfo.GetTotalSpace(systemdrive);
            string systemspacestr = Helpers.Utils.FormatBytes(systemtotalspace).Split(' ')[0];
            Double systemspacedouble = Convert.ToDouble(systemspacestr);
            lblStorageCheck.Text = Helpers.Utils.FormatBytes(systemtotalspace).ToString();

            if (lblStorageCheck.Text.Contains("GB") && (systemspacedouble >= 64)) // Total space, min. 64 GB!
            {
                hddgood.Visible = true;
                hddbad.Visible = false;
            }
            else if (lblStorageCheck.Text.Contains("TB") && (systemspacedouble >= 1))
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

            // DirectX & WDDM
            lblStatus.Text = Locales.Locale.assessmentDirectXWDDM;
            var psi = new ProcessStartInfo();

            if (IntPtr.Size == 4 && Environment.Is64BitOperatingSystem)
            {
                psi.FileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "sysnative\\dxdiag");
            }
            else psi.FileName = System.IO.Path.Combine(Environment.SystemDirectory, "dxdiag");  // Native version

            try
            {
                string directxver;
                string wddmver;
                string check;
                string dxpath = @"dxv.txt";

                psi.Arguments = "/dontskip /t " + dxpath; // don't bypass any diagnostics due to previous crashes in DxDiag.
                using (var prc = Process.Start(psi))
                {
                    if (!File.Exists(dxpath))
                    {
                        prc.WaitForExit();
                        if (prc.ExitCode != 0)
                        {
                            MessageBox.Show("dxdiag failed with exit code " + prc.ExitCode.ToString());
                        }
                    }

                    do
                        System.Threading.Thread.Sleep(100);
                    while (!File.Exists(dxpath));
                    using (var sr = new StreamReader(dxpath))
                    {
                        while (sr.Peek() != -1)
                        {
                            check = sr.ReadLine();
                            if (check.Contains("DirectX Version:"))
                            {
                                directxver = check;
                                lblDirectXCheck.Text = Regex.Replace(directxver, "[^0-9.]", "");
                            }

                            if (check.Contains("Driver Model:"))
                            {
                                wddmver = check;
                                lblWDDMCheck.Text = Regex.Replace(wddmver, "[^0-9.]", "");
                                break;
                            }
                        }
                    }

                    if (lblDirectXCheck.Text == "12")
                    {
                        directgood.Visible = true;
                        directbad.Visible = false;
                    }
                    else
                    {
                        directgood.Visible = false;
                        directbad.Visible = true;

                        performCompatibilityCount += 1;
                    }

                    if (lblWDDMCheck.Text.StartsWith("2.") || lblWDDMCheck.Text.StartsWith("3."))
                    {
                        wddmbad.Visible = false;
                        wddmgood.Visible = true;
                    }
                    else
                    {
                        wddmbad.Visible = true;
                        wddmgood.Visible = false;

                        performCompatibilityCount += 1;
                    }
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }

            // GPU
            lblStatus.Text = Locales.Locale.assessmentGPU;
            lblWDDMCheck.Text += " (" + gpuInfo.Unit() + ")";

            // TPM, Ref. https://wutils.com/wmi/root/cimv2/security/microsofttpm/win32_tpm/cs-samples.html
            lblStatus.Text = Locales.Locale.assessmentTPM;
            ManagementScope scope = new ManagementScope("\\\\.\\ROOT\\CIMV2\\Security\\MicrosoftTpm");
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Tpm");
            ManagementObjectSearcher searcher =
                                    new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection queryCollection = searcher.Get();
            foreach (ManagementObject m in queryCollection)
            {
                string tpmver = m["SpecVersion"].ToString();
                string[] splitted = tpmver.Split(',');

                if (splitted[0].Contains("2.0"))
                {
                    lblTPMCheck.Text = splitted[0];

                    tpmgood.Visible = true;
                    tpmbad.Visible = false;
                    tpminfo.Visible = false;
                }
                if (splitted[0].Contains("1.2"))
                {
                    lblTPMCheck.Text = splitted[0] + " " + Locales.Locale.assessmentTPMLow;

                    tpmgood.Visible = false;
                    tpmbad.Visible = false;
                    tpminfo.Visible = true;

                    performCompatibilityCount += 1;
                }
            }
            if (lblTPMCheck.Text == Locales.Locale.assessmentTPMFail)
            {
                tpmbad.Visible = true;
                tpmgood.Visible = false;
                tpminfo.Visible = false;

                performCompatibilityCount += 1;
            }

            // Inet
            lblStatus.Text = Locales.Locale.assessmentInet;
            if (inetInfo.isINet())
            {
                lblInetCheck.Text = Locales.Locale.assessmentInetOK;
                inetgood.Visible = true;
                inetbad.Visible = false;
            }
            else
            {
                lblInetCheck.Text = Locales.Locale.assessmentInetFail;
                inetgood.Visible = false;
                inetbad.Visible = true;

                performCompatibilityCount += 1;
            }

            // Sum good and bad
            var sum = performCompatibilityCount;
            lblSumBad.Text = sum.ToString();

            if (sum == 0)
            {
                // You're ready for Sun Valley!
                lblSumBad.Text = Locales.Locale.assessmentSummaryOK;
                lblSumBad.ForeColor = Color.Green;
                lblStatus.Visible = false;
                lblSumBad.Font = new Font("Segeo UI", 24.0f);

                // It's all good, so hide bypass options
                lnkCompatibilityFix.Visible = false;
                menuBypassUndo.Visible = false;
                lnkCompatibilityFix.Visible = false;
            }
            else // Components not ready for Windows 11
            {
                // You're ready for Sun Valley!
                lblStatus.Text = Locales.Locale.assessmentSummaryFail;
                lnkCompatibilityFix.Visible = true;
                lblSumBad.ForeColor = Color.DeepPink;
            }

            this.Enabled = true;
        }

        private void lnkCompatibilityFix_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MessageBox.Show(Locales.Locale.optionBypass.Replace("\\n", "\n"), this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                bypassInfo.Windows11(EmbeddedResource.bypass);
                MessageBox.Show(Locales.Locale.infoBypassDone, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void menuBypassUndo_Click(object sender, EventArgs e)
        {
            bypassInfo.Windows11(EmbeddedResource.undo_bypass);
            MessageBox.Show(Locales.Locale.infoBypassUndo, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CaptureToShare()
        {
            Form f = ActiveForm;
            Bitmap bmp = new Bitmap(f.Width, f.Height);
            f.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

            SaveFileDialog dialog = new SaveFileDialog
            {
                InitialDirectory = Application.StartupPath,
                Title = "Location",
                Filter = "PNG Images|*.png",
                FileName = "Compatibility-Screen-Win11-" + osInfo.ComputerName
            };

            DialogResult result = dialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                bmp.Save(dialog.FileName);

                MessageBox.Show(Locales.Locale.infoCaptureHint + " " + dialog.FileName);
                Process.Start(Helpers.Strings.TweetIntent); // Tweet Web Intent post to Twitter
            }
        }

        private void lblMainMenu_Click(object sender, EventArgs e) => this.mainMenu.Show(Cursor.Position.X, Cursor.Position.Y);

        private void assetOpenGitHub_Click(object sender, EventArgs e) => Process.Start(Helpers.Strings.Uri.GitRepo);

        private void btnRecheck_Click(object sender, EventArgs e) => DoCompatibilityCheck();

        private void btnPnlShareScreen_Click(object sender, EventArgs e) => CaptureToShare();

        private void btnShareScreen_Click(object sender, EventArgs e) => CaptureToShare();

        private void btnCompareUtil_Click(object sender, EventArgs e) => GetCompareUtil();

        private void menuVote_Click(object sender, EventArgs e) => Process.Start(Helpers.Strings.Uri.VotePage);

        private void menuInfo_Click(object sender, EventArgs e) => MessageBox.Show("ReadySunValley" + "\nVersion " + Program.GetCurrentVersionTostring() + "\r\n" + Locales.Locale.AppInfo.Replace("\\n", "\n"), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void menuTestingRelease_Click(object sender, EventArgs e) => Process.Start(Helpers.Strings.Uri.GitTestingRelease);

        private void lnkMSRequirements_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start(Helpers.Strings.Uri.MSSystemRequirements);

        private void lnkPartitionTypeInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskPartition");

            StringBuilder message = new StringBuilder();

            foreach (ManagementObject queryObj in searcher2.Get())
            {
                message.AppendLine(Environment.NewLine + (string.Format("DiskIndex: {0}", queryObj["DiskIndex"].ToString()) + "\n" +
                                                          string.Format("Index: {0}", queryObj["Index"].ToString()) + "\n" +
                                                          string.Format(queryObj["Name"].ToString()) + "\n" +
                                                          string.Format(queryObj["Type"].ToString())));
            }

            MessageBox.Show(message.ToString(), Locales.Locale.lblDiskType);
        }

        private void cpuinfo_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.cpuinfo, Locales.Locale.hoveCPUInfo);
        }

        private void cpubad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.cpubad, Locales.Locale.hoverCPUBad);
        }

        private void freqbad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.freqbad, Locales.Locale.hoverCPUSpeedBad);
        }

        private void coresbad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.coresbad, Locales.Locale.hoverCPUCoresBad);
        }

        private void bootbad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.bootbad, Locales.Locale.hoverBootBad);
        }

        private void securebootbad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.securebootbad, Locales.Locale.hoverSecureBootBad);
        }

        private void partbad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.partbad, Locales.Locale.hoverPartitionBad);
        }

        private void screenbad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.screenbad, Locales.Locale.hoverDisplayBad);
        }

        private void rambad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.rambad, Locales.Locale.hoverRAMBad);
        }

        private void hddbad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.hddbad, Locales.Locale.hoverStorageBad);
        }

        private void freespaceinfo_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.freespacebad, Locales.Locale.hoverFreeSpaceBad);
        }

        private void directbad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.directbad, Locales.Locale.hoverDiectXBad);
        }

        private void wddmbad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.wddmbad, Locales.Locale.hoverWDDMBad);
        }

        private void tpminfo_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.tpminfo, Locales.Locale.hoverTPMInfo);
        }

        private void tpmbad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.tpmbad, Locales.Locale.hoverTPMBad);
        }

        private void inetbad_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.inetbad, Locales.Locale.hoverInetBad);
        }

        private void BtnRecheck_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.btnRecheck, Locales.Locale.hoverRecheck);
        }

        private void AssetOpenGitHub_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.assetOpenGitHub, Locales.Locale.hoverAssetInfo);
        }

        private void GetCompareUtil()
        {
            if (MessageBox.Show(Locales.Locale.infoCompareUtil, this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                pBar.Visible = true;

                WebRequest hreq = WebRequest.Create(Helpers.Strings.Uri.UtilVersionCheck);
                hreq.Timeout = 10000;
                hreq.Headers.Set("Cache-Control", "no-cache, no-store, must-revalidate");

                WebResponse hres = hreq.GetResponse();
                StreamReader sr = new StreamReader(hres.GetResponseStream());

                Helpers.Utils.uriUtilLatestVersion = new Version(sr.ReadToEnd().Trim());

                sr.Dispose();
                hres.Dispose();

                var pkg = Helpers.Strings.Uri.CompareUtil + Helpers.Utils.uriUtilLatestVersion + "/" + "WhyNotWin11.exe";

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
            pBar.Value = e.ProgressPercentage;
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

                pBar.Visible = false;

                MessageBox.Show(Locales.Locale.infoCompareUtilDownloadMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text);
                pBar.Visible = false;
            }
        }

        private void CheckCompareMS_CheckedChanged(object sender, EventArgs e)
        {
            if (checkCompareMS.Checked)
            {
                lblStatus.Visible = false;
                lnkMSRequirements.Visible = true;
                lblSumBad.Visible = false;
                lnkCompatibilityFix.Visible = false;
                pbCompare.Visible = true;
                checkCompareMS.Text = Locales.Locale.checkCompareMSBack;

                var request = WebRequest.Create(Helpers.Strings.Uri.CompareMS);

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())

                    pbCompare.Image = Bitmap.FromStream(stream);
            }
            else if (!checkCompareMS.Checked)
            {
                checkCompareMS.Text = Locales.Locale.checkCompareMS;
                pbCompare.Visible = false;
                lblStatus.Visible = true;
                lnkMSRequirements.Visible = false;
                lblSumBad.Visible = true;
                lnkCompatibilityFix.Visible = true;
            }
        }
    }
}