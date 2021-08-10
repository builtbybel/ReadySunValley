using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Windows.Forms;

namespace ReadySunValley.Helpers
{
    public static class Utils
    {
        public static Version CurrentVersion = new Version(Application.ProductVersion);
        public static Version LatestVersion;

        public static void AppUpdate()
        {
            try
            {
                string versionContent = new WebClient().DownloadString(Strings.Uri.GitVersionHint);

                WebRequest hreq = WebRequest.Create(Strings.Uri.GitVersionCheck);
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
                    if (MessageBox.Show("A new app version " + LatestVersion + " is available.\nDo you want to goto the Github update page?" + Environment.NewLine + versionContent, Locales.Locale.infoUpdate, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) // New release available!
                    {
                        Process.Start(Strings.Uri.GitUpdateRepo + LatestVersion);
                    }
                }
            }
            catch
            { MessageBox.Show(Locales.Locale.errorUpdate + Environment.NewLine + Locales.Locale.errorInternet); }
        }

        public static String FormatBytes(long byteCount)
        {
            string[] suf = { " B", " KB", " MB", " GB", " TB", " PB", " EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        // Check elevation
        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Launch Urls in richSumming control
        /// </summary>
        /// <param name="url"></param>
        public static void LaunchUri(string url)
        {
            if (IsHttpURL(url)) Process.Start(url);
        }

        /// <summary>
        /// Check Urls in richSumming control
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsHttpURL(string url)
        {
            return
                ((!string.IsNullOrWhiteSpace(url)) &&
                (url.ToLower().StartsWith("http")));
        }
    }
}