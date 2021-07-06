using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ReadySunValley
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

            // GUI options
            this.Text = Helpers.Strings.Titles.AppName;     // Title
            lblMainMenu.Text = "\ue700";                    // Hamburger menu
            btnRecheck.Text = "\ue72c";                     // Refresh
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            DoCompatibilityCheck();
        }

        private void MainWindow_SizeChanged(object sender, EventArgs e)
        {
            int formWidth = this.Width;

            if (formWidth < 880 && PicCompare.Visible == false)
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
            lblStatus.Text = "Checking system requirements [1/13]";
            Helpers.Utils.AppUpdate();

            // CPU arch
            lblStatus.Text = "Checking CPU architecture [2/13]";
            lblBitnessCheck.Text = Assessment.CPU.Architecture();
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

            // Display size for each monitor, Ref. https://theezitguy.wordpress.com/category/c-sharp/
            lblStatus.Text = "Checking Display [3/13]";
            lblDisplayCheck.Text = "";
            int counter = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("\\root\\wmi", "SELECT * FROM WmiMonitorBasicDisplayParams").Get())
            {
                counter++;
                double width = (byte)item["MaxHorizontalImageSize"] / 2.54;
                double height = (byte)item["MaxVerticalImageSize"] / 2.54;
                double diagonal = Math.Sqrt(width * width + height * height);
                lblDisplayCheck.Text += counter + ". " + diagonal.ToString("0.00") + " inch ";
                if (diagonal <= 9)
                {
                    screengood.Visible = false;
                    screenbad.Visible = true;

                    performCompatibilityCount += 1;
                }
                else
                {
                    screengood.Visible = true;
                    screenbad.Visible = false;
                }
            }

            // Boot Method
            lblBootTypeCheck.Text = Assessment.Boot.IsUEFI();
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

            // CPU Clock speed
            lblStatus.Text = "Checking CPU speed [4/13]";
            var clockspeed = Assessment.CPU.ClockSpeed();
            lblMhzCheck.Text = clockspeed + " MHz Frequency";
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

            // CPU Core counts
            lblStatus.Text = "Getting Core counts [5/13]";
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            lblCoresCheck.Text = coreCount + " Cores, " + Environment.ProcessorCount + " Threads";

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

            // CPU Compatibility check
            lblStatus.Text = "Checking CPU Compatibility [6/13]";
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_Processor").Get())
            {
                lblCPU.Text = item["Name"].ToString();

                var amdbytes = Properties.Resources.amdsupport;
                string amdsupported = System.Text.Encoding.UTF8.GetString(amdbytes);

                var intelbytes = Properties.Resources.intelsupport;
                string intelsupported = System.Text.Encoding.UTF8.GetString(intelbytes);

                string supportedCPUs = amdsupported + "\n" + intelsupported;
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
            lblStatus.Text = "Checking Partition Types [7/13]";
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_DiskPartition WHERE BootPartition=True").Get())
            {
                if (item["Type"].ToString().Contains("System"))
                {
                    if (item["Type"].ToString().Contains("GPT"))
                    {
                        lblDiskTypeCheck.Text = "GPT";
                        partgood.Visible = true;
                        partbad.Visible = false;
                    }
                    else
                    {
                        lblDiskTypeCheck.Text = "MBR";
                        partgood.Visible = false;
                        partbad.Visible = true;

                        performCompatibilityCount += 1;
                        break;
                    }
                }
            }

            // Secure Boot
            lblStatus.Text = "Checking Secure Boot Status [8/13]";

            if (Assessment.Boot.IsSecureBoot())
            {
                lblSecureBootCheck.Text = "Supported";

                securebootgood.Visible = true;
                securebootbad.Visible = false;
            }
            else
            {
                securebootgood.Visible = false;
                securebootbad.Visible = true;
                lblSecureBootCheck.Text = "Unsupported";

                performCompatibilityCount += 1;
            }

            // RAM
            lblStatus.Text = "Checking RAM Compatibility [9/13]";
            long ram = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("select * from Win32_PhysicalMemory").Get())
            {
                string ramstr = item["Capacity"].ToString();
                ram = ram += long.Parse(ramstr);
            }
            lblRAMCheck.Text = Helpers.Utils.FormatBytes(ram).ToString();

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
            lblStatus.Text = "Checking Disk size [10/13]";
            var systemdrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

            long systemfreespace = Assessment.Storage.GetTotalFreeSpace(systemdrive);
            string systemfreespacestr = Helpers.Utils.FormatBytes(systemfreespace).Split(' ')[0];
            Double systemfreespacedouble = Convert.ToDouble(systemfreespacestr);
            lblFreeSpaceCheck.Text = Helpers.Utils.FormatBytes(systemfreespace).ToString();

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

            long systemtotalspace = Assessment.Storage.GetTotalSpace(systemdrive);
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
            }

            // DirectX & WDDM
            lblStatus.Text = "Getting DirectX && WDDM2 [11/13]";
            try
            {
                string directxver;
                string wddmver;
                string filepath = @"dxv.txt";
                string check;

                Process.Start("dxdiag", "/t " + filepath);
                do
                    System.Threading.Thread.Sleep(100);
                while (!File.Exists(filepath));
                using (var sr = new StreamReader(filepath))
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

                    if (lblWDDMCheck.Text.Contains("2") || lblWDDMCheck.Text.Contains("3.0"))
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
            catch { }

            // GPU
            lblStatus.Text = "Getting Graphics card [12/13]";
            lblWDDMCheck.Text += " (" + Assessment.GPU.Unit() + ")";

            // TPM, Ref. https://wutils.com/wmi/root/cimv2/security/microsofttpm/win32_tpm/cs-samples.html
            lblStatus.Text = "Getting TPM version... [10/11]";
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
                    lblTPMCheck.Text = splitted[0] + " (Not supported)";

                    tpmgood.Visible = false;
                    tpmbad.Visible = false;
                    tpminfo.Visible = true;

                    performCompatibilityCount += 1;
                }
            }
            if (lblTPMCheck.Text == "Not present")
            {
                tpmbad.Visible = true;
                tpmgood.Visible = false;
                tpminfo.Visible = false;

                performCompatibilityCount += 1;
            }

            // Inet
            lblStatus.Text = "Checking Internet connection [13/13]";
            if (Assessment.Inet.isINet())
            {
                lblInetCheck.Text = "Available";
                inetgood.Visible = true;
                inetbad.Visible = false;
            }
            else
            {
                lblInetCheck.Text = "No";
                inetgood.Visible = false;
                inetbad.Visible = true;

                performCompatibilityCount += 1;
            }

            // Sum good and bad
            var sum = performCompatibilityCount;
            LblSumBad.Text = sum.ToString();

            if (sum == 0)
            {
                LblSumBad.ForeColor = Color.Green;
                lblStatus.Visible = false;
                LblSumBad.Text = "You're ready for Sun Valley!";

                // It's all good, so hide bypass options
                lnkCompatibilityFix.Visible = false;
                menuBypassUndo.Visible = false;
            }
            else
            {
                lblStatus.Text = "Components not ready for Windows 11";
                LblSumBad.ForeColor = Color.DeepPink;
            }

            this.Enabled = true;
        }

        private void lnkCompatibilityFix_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MessageBox.Show(Helpers.Strings.Body.Bypass, this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Assessment.Bypass.Windows11(EmbeddedResource.bypass);
                MessageBox.Show(Helpers.Strings.Body.BypassOK, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void menuBypassUndo_Click(object sender, EventArgs e)
        {
            Assessment.Bypass.Windows11(EmbeddedResource.undo_bypass);
            MessageBox.Show(Helpers.Strings.Body.BypassUndo, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                FileName = "Compatibility-Screen-Win11-" + System.Environment.MachineName
            };

            DialogResult result = dialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                bmp.Save(dialog.FileName);

                Process.Start(Helpers.Strings.Uri.ShareTwitter);
            }
        }

        private void lblMainMenu_Click(object sender, EventArgs e) => this.MainMenu.Show(Cursor.Position.X, Cursor.Position.Y);

        private void assetOpenGitHub_Click(object sender, EventArgs e) => Process.Start(Helpers.Strings.Uri.GitRepo);

        private void btnRecheck_Click(object sender, EventArgs e) => DoCompatibilityCheck();

        private void btnPnlShareScreen_Click(object sender, EventArgs e) => CaptureToShare();

        private void btnShareScreen_Click(object sender, EventArgs e) => CaptureToShare();

        private void btnCompareUtil_Click(object sender, EventArgs e) => GetCompareUtil();

        private void menuInfo_Click(object sender, EventArgs e) => MessageBox.Show(Helpers.Strings.Body.AppInfo, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void menuVote_Click(object sender, EventArgs e) => Process.Start(Helpers.Strings.Uri.VotePage);

        private void cpuinfo_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.cpuinfo, Helpers.Strings.Hover.CPUInfo);
        }

        private void cpubad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.cpubad, Helpers.Strings.Hover.CPUBad);
        }

        private void freqbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.freqbad, Helpers.Strings.Hover.FreqBad);
        }

        private void coresbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.coresbad, Helpers.Strings.Hover.CoresBad);
        }

        private void bootbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.bootbad, Helpers.Strings.Hover.BootBad);
        }

        private void partbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.partbad, Helpers.Strings.Hover.PartBad);
        }

        private void screenbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.screenbad, Helpers.Strings.Hover.ScreenBad);
        }

        private void rambad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.rambad, Helpers.Strings.Hover.RAMBad);
        }

        private void hddbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.hddbad, Helpers.Strings.Hover.HDDBad);
        }

        private void freespaceinfo_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.freespaceinfo, Helpers.Strings.Hover.FreeSpaceInfo);
        }

        private void directbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.directbad, Helpers.Strings.Hover.DirectXBad);
        }

        private void wddmbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.wddmbad, Helpers.Strings.Hover.WDDMBad);
        }

        private void tpminfo_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.tpminfo, Helpers.Strings.Hover.TPMInfo);
        }

        private void tpmbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.tpmbad, Helpers.Strings.Hover.TPMBad);
        }

        private void securebootbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.securebootbad, Helpers.Strings.Hover.SecureBootBad);
        }

        private void inetbad_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.inetbad, Helpers.Strings.Hover.InetBad);
        }

        private void BtnRecheck_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.btnRecheck, Helpers.Strings.Hover.Recheck);
        }

        private void AssetOpenGitHub_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.assetOpenGitHub, Helpers.Strings.Hover.AssetInfo);
        }

        private void GetCompareUtil()
        {
            if (MessageBox.Show("Do you want to compare the results with the Utility \"WhyNotWin11\"?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
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

                MessageBox.Show("Ready! So now put the two apps next to each other and take a look at the results.");
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
                LblSumBad.Visible = false;
                lnkCompatibilityFix.Visible = false;
                PicCompare.Visible = true;
                checkCompareMS.Text = "Back to my results";

                var request = WebRequest.Create(Helpers.Strings.Uri.CompareMS);

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())

                    PicCompare.Image = Bitmap.FromStream(stream);
            }
            else if (!checkCompareMS.Checked)
            {
                checkCompareMS.Text = "Compare with Microsoft requirements";
                PicCompare.Visible = false;
                lblStatus.Visible = true;
                LblSumBad.Visible = true;
                lnkCompatibilityFix.Visible = true;
            }
        }
    }
}