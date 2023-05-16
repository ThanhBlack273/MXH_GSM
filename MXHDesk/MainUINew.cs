using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MXH
{
    public partial class MainUINew : XtraForm
    {
        Thread paintThread;

        public MainUINew()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
        }

        private void MainUINew_Load(object sender, EventArgs e)
        {
            Application.ApplicationExit += (_sender, @event) =>
            {
                GlobalVar.IsApplicationExit = true;
                GSMControlCenter.Dispose();
                GlobalVar.ForceKillMyself();
            };


            gridGSM.DataSource = GSMControlCenter.GSMComs;
            paintThread = new Thread(new ThreadStart(() =>
            {
                while (!GlobalVar.IsApplicationExit)
                {
                    try
                    {
                        gridGSM.RefreshDataSource();
                    }
                    catch { }
                    Thread.Sleep(1000);
                }
            }));
            paintThread.Start();

            GSMControlCenter.Start();
            GSMControlCenter.GSMMessages.Add(new GSMMessage() { Sender = "Test" });
        }
    }
}
