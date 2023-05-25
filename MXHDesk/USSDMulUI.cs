using DevExpress.ReportServer.ServiceModel.Native.RemoteOperations;
using DevExpress.Utils.DirectXPaint.Svg;
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
    public partial class USSDMulUI : XtraForm
    {
        List<GSMCom> COMS = new List<GSMCom>();

        public USSDMulUI(List<GSMCom> coms)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            COMS = coms;
            this.Load += USSDUI_Load;
        }

        private void USSDUI_Load(object sender, EventArgs e)
        {
            Init();
        }
        private void Init()
        {
            foreach (var com in COMS)
            {
                com.LastUSSDCommand = string.Empty;
                com.LastResult = string.Empty;
            }
            gridControl1.DataSource = COMS;
        }
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtUSSD.Text.Trim()))
                    return;
                string ussd = txtUSSD.Text.Trim();
                var tasks = new List<Task>();
                foreach (var com in COMS)
                {
                    tasks.Add(new Task(() =>
                    {
                        com.ExecuteSingleUSSD(ussd);
                    }));
                    Thread.Sleep(5000);
                }
                waitScreen.Visible = true;
                new Task(() =>
                {
                    tasks.ForEach(task => task.Start());
                    Task.WaitAll(tasks.ToArray());

                    this.Invoke(new MethodInvoker(() =>
                    {
                        gridControl1.RefreshDataSource();
                        waitScreen.Visible = false;
                    }));
                }).Start();
            }
            catch { }

        }
    }
}
