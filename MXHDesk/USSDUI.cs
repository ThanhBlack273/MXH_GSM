using DevExpress.ReportServer.ServiceModel.Native.RemoteOperations;
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
    public partial class USSDUI : XtraForm
    {
        GSMCom COM = null;
        Thread hookThread = null;

        public USSDUI(GSMCom com)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            this.FormClosing += CallOutUI_FormClosing;
            COM = com;
            this.Load += USSDUI_Load;
        }

        private void USSDUI_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void CallOutUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            COM.USSDCancel();
        }
        private void Init()
        {
            COM.ResetUSSDEvent();
            hookThread = new Thread(new ThreadStart(() =>
            {
                COM.USSDHook();
                try
                {
                    Thread.CurrentThread.Abort();
                }
                catch { }
            }));
            hookThread.Start();
            COM.USSDResponse += (response) => { txtScreen.Text = response; HideWait(); };
            btnNum1.Click += BtnNum_Click;

            btnNum2.Click += BtnNum_Click; btnNum3.Click += BtnNum_Click; btnNum4.Click += BtnNum_Click; btnNum5.Click += BtnNum_Click; btnNum6.Click
            += BtnNum_Click; btnNum7.Click += BtnNum_Click; btnNum8.Click += BtnNum_Click; btnNum9.Click += BtnNum_Click; btnNum10.Click += BtnNum_Click; btnNum11.Click += BtnNum_Click;
            btnNum0.Click += BtnNum_Click;
        }

        private void BtnNum_Click(object sender, EventArgs e)
        {
            ShowWait();
            new Task(() =>
            {
                COM.USSDRequest(((SimpleButton)sender).Text.Trim());
            }).Start();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUSSD.Text.Trim()))
                return;

            ShowWait();
            new Task(() =>
            {
                COM.USSDReset();
                COM.USSDRequest(txtUSSD.Text.Trim());
            }).Start();
        }
        private void ShowWait()
        {
            waitScreen.Visible = true;
        }
        private void HideWait()
        {
            waitScreen.Visible = false;
        }

    }
}
