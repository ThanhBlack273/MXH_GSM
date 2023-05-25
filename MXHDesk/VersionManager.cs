using DevExpress.Data.Filtering;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MXH
{
    public static class VersionManager
    {
        public static VersionInfo CurrentVersion = new VersionInfo
        {
            VersionCode = "3.0",
            VersionName = "Phiên bản 3.0",
            Description = @"- Thông tin phiên bản:
    Fix lỗi và update hỗ trợ thêm nhiều loại modem mới",
            ForceUpgrade = false,
            IsLatest = true,
            ProductCode = "MXH",
            VersionDate = new DateTime(2023, 6, 01),
            VersionType = VersionType.Released,
            DownloadUrl = "",
            ProductName = "MXH"
        };

        public static void CheckForUpdate(bool forceUpdate = false)
        {
            try
            {
                var versionInfo = new MXHPortal().GetLatestVersionInfo();
                if (versionInfo == null)
                {
                    MessageBox.Show("Không kiểm tra được thông tin phiên bản!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    GlobalVar.ForceKillMyself();
                    return;
                }
                else
                {
                    if (CurrentVersion.VersionCode != versionInfo.VersionCode)
                    {
                        if (forceUpdate)
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.FileName = "MXHDeskAutoUpdater.exe";
                            startInfo.Arguments = "\"" + JsonConvert.SerializeObject(versionInfo).Replace("\"", "\\\"") + "\"";
                            Process.Start(startInfo);
                            GlobalVar.ForceKillMyself();
                            return;
                        }
                    }
                }
            }
            catch
            {
                GlobalVar.ForceKillMyself();
                return;
            }
        }
    }
    public class VersionInfo
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public bool IsLatest { get; set; }
        public string VersionName { get; set; }
        public DateTime VersionDate { get; set; }
        public VersionType VersionType { get; set; }
        public bool ForceUpgrade { get; set; }
        public string VersionCode { get; set; }
        public string Description { get; set; }
        public string DownloadUrl { get; set; }
    }
    public enum VersionType
    {
        Released,
        Beta
    }
}
