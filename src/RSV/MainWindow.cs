using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

/*using System.Threading;
using System.Globalization; */

namespace ReadySunValley
{
    public partial class MainWindow : Form
    {
        // Using same apps settings storage root as for the App Store version
        // The name of <emPACKAGEID> is concatenated from the application package name and a signing certificate based postfix.
        public static string mAppDataDir = Environment.GetFolderPath
            (Environment.SpecialFolder.LocalApplicationData) + @"\Packages\36220Builtbybel.ReadySunValley_4mzjg7prtd9xe\Settings\";

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
            InitializeComponent();
            InitializeAppDataDir();

            // Uncomment lower line and add lang code to run localization test
           // Thread.CurrentThread.CurrentUICulture = new CultureInfo("de");

            // GUI localization
            Globalization();

            // User Interface
            UISelection();
        }

        private void UISelection()
        {
            // GUI options
            this.Text += Program.GetCurrentVersionTostring();
            this.MinimumSize = new Size(854, 750);
            lnkSubHeader.Text = Locales.Locale.lblSubHeader + "\x20"
                               + osInfo.GetOS() + "\x20"                        // OS Info
                               + osInfo.GetVersion() + "\x20"
                               + osInfo.Is64Bit();

            lblMainMenu.Text = "\ue700";                                       // Hamburger menu
            btnRecheck.Text = "\ue72c";                                        // Refresh

            // Some tooltip options
            ToolTip tt = new ToolTip();
            tt.AutoPopDelay = 15000;
            tt.IsBalloon = true;
        }

        private void Globalization()
        {
            btnPnlShareScreen.Text = Locales.Locale.btnPnlShareScreen;
            btnShareScreen.Text = Locales.Locale.btnShareScreen;
            checkCompareMS.Text = Locales.Locale.checkCompareMS;
            checkReport.Text = Locales.Locale.checkReport;
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
            lblTPM.Text = Locales.Locale.lblTPM;
            lblTPMCheck.Text = Locales.Locale.assessmentTPMFail;
            lblWDDM.Text = Locales.Locale.lblWDDM;
            lblWDDMCheck.Text = Locales.Locale.lblWDDMCheck;
            lnkCompatibilityFix.Text = Locales.Locale.lnkCompatibilityFix;
            menuVoteContent.Text = Locales.Locale.menuVoteContent;
            menuVote.Text = Locales.Locale.menuVote;
            menuBypassUndo.Text = Locales.Locale.menuBypassUndo;
        }

        private void InitializeAppDataDir()
        {
            try
            {
                if (!Directory.Exists(mAppDataDir))
                    Directory.CreateDirectory(mAppDataDir);
            }
            catch { }
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
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

        private void AddSumming(string Value)
        {
            checkCompareMS.Text = Locales.Locale.checkCompareMS;
            richSumming.Text += "\u2022" + "\x20" + Value + Environment.NewLine + Environment.NewLine;
        }

        /// <summary>
        ///  Run all assessments
        /// </summary>
        private void DoCompatibilityCheck()
        {
            int performCompatibilityCount = 0;          // Reset compatibility count
            richSumming.Text = null;                    // Reset report

            this.Enabled = false;

            // Checking System requirements (app and OS relevant)
            lblStatus.Text = Locales.Locale.assessmentSystemRequirements;

            AddSumming(Locales.Locale.lblHeader + "\x20\r\n" + lnkSubHeader.Text);      // Some OS Info
            Helpers.Utils.AppUpdate();                                                  // Run here also app update check

            // CPU bitness
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
                AddSumming(Locales.Locale.descDisplayBad);
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
                AddSumming(Locales.Locale.descBootBad);
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
                AddSumming(Locales.Locale.descCPUSpeedBad);
            }

            // CPU Core
            lblStatus.Text = Locales.Locale.assessmentCPUCores;
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            lblCoresCheck.Text = coreCount + "\x20" + Locales.Locale.lblCores + ", " + Environment.ProcessorCount + "\x20" + Locales.Locale.lblThreads;

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
                AddSumming(Locales.Locale.descCPUCoresBad);
            }

            // CPU Compatibility
            lblStatus.Text = Locales.Locale.assessmentCPUCompatibility;
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_Processor").Get())
            {
                lblCPU.Text = item["Name"].ToString();
            }

            string myCPU = lblCPU.Text;
            bool FoundCPU = false;

            File.WriteAllText(mAppDataDir + "SupportedProcessors.txt", Properties.Resources.supportedCPU);
            using (StreamReader sr = File.OpenText(mAppDataDir + "SupportedProcessors.txt"))
            {
                string[] lines = File.ReadAllLines(mAppDataDir + "SupportedProcessors.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    if (myCPU.ToString().Contains(lines[i]))
                    {
                        FoundCPU = true;
                        sr.Close();
                    }
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
                        AddSumming(Locales.Locale.descCPUInfo.Replace("\\n", Environment.NewLine));
                    }
                    else
                    {
                        cpugood.Visible = false;
                        cpubad.Visible = true;
                        cpuinfo.Visible = false;

                        performCompatibilityCount += 1;
                        AddSumming(Locales.Locale.descCPUBad);
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
            if (!FoundGPT)
            {
                performCompatibilityCount += 1;
                AddSumming(Locales.Locale.descPartitionBad);
            }

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
                AddSumming(Locales.Locale.descSecureBootBad);
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
                    AddSumming(Locales.Locale.descRAMBad);
                }
            }

            // Storage info
            lblStatus.Text = Locales.Locale.assessmentStorage;
            var systemdrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

            long systemfreespace = storageInfo.GetTotalFreeSpace(systemdrive);
            string systemfreespacestr = Helpers.Utils.FormatBytes(systemfreespace).Split(' ')[0];
            Double systemfreespacedouble = Convert.ToDouble(systemfreespacestr);
            lblFreeSpaceCheck.Text = Helpers.Utils.FormatBytes(systemfreespace).ToString();

            // Free space, min. 6 GB
            if (lblFreeSpaceCheck.Text.Contains("GB") && (systemfreespacedouble >= 6))
            {
                freespacegood.Visible = true;
                freespacebad.Visible = false;
            }
            else if (lblFreeSpaceCheck.Text.Contains("TB") && (systemfreespacedouble >= 1))
            {
                freespacegood.Visible = true;
                freespacebad.Visible = false;
            }
            else
            {
                freespacegood.Visible = false;
                freespacebad.Visible = true;
            }
            // Total space, min. 64 GB
            long systemtotalspace = storageInfo.GetTotalSpace(systemdrive);
            string systemspacestr = Helpers.Utils.FormatBytes(systemtotalspace).Split(' ')[0];
            Double systemspacedouble = Convert.ToDouble(systemspacestr);
            lblStorageCheck.Text = Helpers.Utils.FormatBytes(systemtotalspace).ToString();

            if (lblStorageCheck.Text.Contains("GB") && (systemspacedouble >= 64))
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
                AddSumming(Locales.Locale.descStorageBad);
            }

            // DirectX & WDDM
            lblStatus.Text = Locales.Locale.assessmentDirectXWDDM;
            var psi = new ProcessStartInfo();

            if (IntPtr.Size == 4 && Environment.Is64BitOperatingSystem) // 4 bytes for 32bit and 8 bytes for 64bit
            {
                psi.FileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "sysnative\\dxdiag");
            }
            else psi.FileName = System.IO.Path.Combine(Environment.SystemDirectory, "dxdiag");  // Native version

            try
            {
                string directxver;
                string wddmver;
                string check;
                string dxpath = mAppDataDir + "rsv.txt"; ;

                psi.Arguments = "/dontskip /t " + dxpath; // Don't bypass any diagnostics due to previous crashes in dxdiag
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
                        AddSumming(Locales.Locale.descDirectXBad);
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
                        AddSumming(Locales.Locale.descWDDMBad);
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
            try
            {
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
                        AddSumming(Locales.Locale.descTPMInfo);
                    }
                }
                if (lblTPMCheck.Text == Locales.Locale.assessmentTPMFail)
                {
                    tpmbad.Visible = true;
                    tpmgood.Visible = false;
                    tpminfo.Visible = false;

                    performCompatibilityCount += 1;
                    AddSumming(Locales.Locale.descTPMBad);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }

            // Inet
            lblStatus.Text = Locales.Locale.assessmentInet;
            if (inetInfo.IsInet())
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
                AddSumming(Locales.Locale.descInetBad);
            }

            // Sum good and bad
            var sum = performCompatibilityCount;
            lblSumBad.Text = sum.ToString();

            if (sum == 0)
            {
                // You're ready for Sun Valley!
                lblSumBad.Text = Locales.Locale.assessmentSummaryOK;
                AddSumming(Locales.Locale.assessmentSummaryOK);
                lblSumBad.ForeColor = Color.Green;
                lblStatus.Visible = false;
                lblSumBad.Font = new Font("Segeo UI", 24.0f);
                pbReady.Visible = true;

                // It's all good, so hide bypass options
                lnkCompatibilityFix.Visible = false;
                menuBypassUndo.Visible = false;
                lnkCompatibilityFix.Visible = false;
            }
            else // This PC can't run Sun Valley!
            {
                lblStatus.Text = Locales.Locale.assessmentSummaryFail;
                AddSumming(sum + "\x20" + Locales.Locale.assessmentSummaryFail);
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
                Process.Start(Helpers.Strings.TweetIntent); // Tweet Web Intent
            }
        }

        /// <summary>
        /// Buttons/Links and menu events
        /// </summary>
        private void lblMainMenu_Click(object sender, EventArgs e) => this.mainMenu.Show(Cursor.Position.X, Cursor.Position.Y);

        private void assetOpenGitHub_Click(object sender, EventArgs e) => Process.Start(Helpers.Strings.Uri.GitRepo);

        private void btnRecheck_Click(object sender, EventArgs e) => DoCompatibilityCheck();

        private void btnPnlShareScreen_Click(object sender, EventArgs e) => CaptureToShare();

        private void btnShareScreen_Click(object sender, EventArgs e) => CaptureToShare();

        private void menuVote_Click(object sender, EventArgs e) => Process.Start(Helpers.Strings.Uri.VotePage);

        private void menuInfo_Click(object sender, EventArgs e) => MessageBox.Show("ReadySunValley" + "\nVersion " + Program.GetCurrentVersionTostring() + "\r\n" + Locales.Locale.AppInfo.Replace("\\n", "\n"), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void lnkMSRequirements_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start(Helpers.Strings.Uri.MSSystemRequirements);

        private void lnkSubHeader_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start("ms-settings:about");

        private void richSumming_LinkClicked(object sender, LinkClickedEventArgs e) => Helpers.Utils.LaunchUri(e.LinkText);

        /// <summary>
        /// Tooltips
        /// </summary>
        private void BtnRecheck_MouseHover(object sender, EventArgs e) => tt.SetToolTip(this.btnRecheck, Locales.Locale.ttRecheck);

        private void AssetOpenGitHub_MouseHover(object sender, EventArgs e) => tt.SetToolTip(this.assetOpenGitHub, Locales.Locale.assetGithub);

        /// <summary>
        /// Show all partitions
        /// </summary>
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

                try
                {
                    var request = WebRequest.Create(Helpers.Strings.Uri.CompareMS);

                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())

                        pbCompare.Image = Bitmap.FromStream(stream);
                }
                catch { MessageBox.Show(Locales.Locale.errorInternet); }
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

        private void checkReport_CheckedChanged(object sender, EventArgs e)
        {
            if (checkReport.Checked)
            {
                richSumming.Visible = true;
                btnShareScreen.Enabled = false;
                btnPnlShareScreen.Enabled = false;
                checkReport.Text = Locales.Locale.checkReportBack;
            }
            else if (!checkReport.Checked)
            {
                checkReport.Text = Locales.Locale.checkReport;
                richSumming.Visible = false;
                btnShareScreen.Enabled = true;
                btnPnlShareScreen.Enabled = true;
            }
        }
    }
}