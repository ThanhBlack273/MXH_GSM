using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MXH.MVT;

namespace MXH
{
    public partial class AboutUI : XtraForm
    {
        public AboutUI()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
        }

        private void AboutUI_Load(object sender, EventArgs e)
        {
            lblProductName.Text = VersionManager.CurrentVersion.ProductName;
            lblVersionName.Text = $"{VersionManager.CurrentVersion.VersionName} ({VersionManager.CurrentVersion.VersionType.ToString()}) - {VersionManager.CurrentVersion.VersionDate.ToString("dd-MM-yyyy")}";
            lblVersionDescription.Text = VersionManager.CurrentVersion.Description;
        }

        private void label2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.facebook.com/soul.keeper79/");
        }
    }
}
