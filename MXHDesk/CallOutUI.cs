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
using DevExpress.XtraGrid;

namespace MXH
{
    public partial class CallOutUI : XtraForm
    {
        private List<string> Callers = new List<string>();
        private List<GSMCom> Coms = new List<GSMCom>();
        public string DialNo { get; set; }
        public bool Loop { get => ckLoop.Checked; set => ckLoop.Checked = value; }
        bool Stop = false;

        public CallOutUI()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            
        }

        public CallOutUI(List<GSMCom> coms)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            Coms = coms;
            this.FormClosing += CallOutUI_FormClosing;
            this.Load += CallOutUI_Load;
        }
        private void CallOutUI_Load(object sender, EventArgs e)
        {
            foreach (var com in Coms)
            {
                Callers.Add(com.PhoneNumber);
            }
        }

        private void CallOutUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop = true;
        }

        private void txtTo_EditValueChanged(object sender, EventArgs e)
        {
            DialNo = txtTo.Text;
        }

        private void btnCall_Click(object sender, EventArgs e)
        {
            var op = Application.OpenForms.OfType<MainUI>().Single();
            Stop = false;
            try
            {
                if (Stop)
                    return;

                if (string.IsNullOrEmpty(DialNo))
                {
                    MessageBox.Show("Please input dial phone number", "Warning", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                try
                {

                    btnCall.Enabled = false;
                    btnCall.Visible = false;
                    btnCallOff.Enabled = true;
                    btnCallOff.Visible = true;

                    txtDuration.Enabled = false;
                    txtTo.Enabled = false;
                   
                }
                catch { }
                foreach (var com in Coms)
                {
                    com.LastResult = string.Empty;
                }

                int duration = Convert.ToInt32(txtDuration.Value);
                string selected = rgCallMode.Properties.Items[rgCallMode.SelectedIndex].Description;


                var serialPorts = GSMControlCenter.GSMComs.Where(com => Callers.Contains(com.PhoneNumber));
                if (serialPorts.Any())
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            pbSendProcess.Visible = true;
                            pbSendProcess.Reset();
                            pbSendProcess.Properties.Maximum = serialPorts.Count();
                            pbSendProcess.Properties.Step = 1;
                            pbSendProcess.EditValue = 0;
                        }
                        catch { }
                    }));
                    switch (selected)
                    {
                        case "Đồng thời":
                            {
                                List<Task> tasks = new List<Task>();
                                foreach (var com in serialPorts)
                                {
                                    tasks.Add(new Task(() =>
                                    {
                                        try
                                        {
                                            if (Stop)
                                                return;
                                            lblCallInfo.Text = $"Tất cả các số đang gọi...";

                                            com.Call(DialNo, duration);
                                            this.Invoke(new MethodInvoker(() =>
                                            {
                                                try
                                                {
                                                    pbSendProcess.PerformStep();
                                                }
                                                catch { }
                                            }));
                                            if (Stop)
                                                return;
                                        }
                                        catch { }
                                    }));
                                }
                                new Task(() =>
                                {
                                    try
                                    {
                                        tasks.ForEach(task => task.Start());
                                        Task.WaitAll(tasks.ToArray());
                                        this.Invoke(new MethodInvoker(() =>
                                        {
                                            
                                            op.ReloadView();
        
                                        }));
                                        if (Loop)
                                        {
                                            try
                                            {
                                                btnCall_Click(null, null);
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                Stop = true;
                                                btnCall.Enabled = true;
                                                btnCall.Visible = true;
                                                btnCallOff.Enabled = false;
                                                btnCallOff.Visible = false;

                                                txtDuration.Enabled = true;
                                                txtTo.Enabled = true;
                                                lblCallInfo.Text = $"Hoàn Thành";
                                            }
                                            catch { }
                                        }
                                    }
                                    catch { }
                                }).Start();



                                break;
                            }
                        case "Tuần tự":
                            {
                                new Task(() =>
                                {
                                    foreach (var com in serialPorts)
                                    {
                                        try
                                        {
                                            if (Stop)
                                                return;
                                            this.Invoke(new MethodInvoker(() =>
                                            {
                                                try
                                                {

                                                    lblCallInfo.Text = $"{com.PhoneNumber} -> Đang gọi...";
                                                }
                                                catch { }
                                            }));
                                            com.Call(DialNo, duration);
                                            this.Invoke(new MethodInvoker(() =>
                                            {
                                                try
                                                {
                                                    pbSendProcess.PerformStep();
                                                }
                                                catch { }
                                            }));
                                            if (Stop)
                                                return;
                                        }
                                        catch { }
                                    }
                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        /*var op = Application.OpenForms.OfType<MainUI>().Single();*/
                                        op.ReloadView();

                                    }));
                                    if (Loop)
                                    {
                                        try
                                        {
                                            btnCall_Click(null, null);
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            Stop = true;
                                            btnCall.Enabled = true;
                                            btnCall.Visible = true;
                                            btnCallOff.Enabled = false;
                                            btnCallOff.Visible = false;

                                            txtDuration.Enabled = true;
                                            txtTo.Enabled = true;
                                            lblCallInfo.Text = $"Hoàn Thành";
                                        }
                                        catch { }
                                    }
                                }).Start();

                                break;
                            }
                    }

                }
            }
            catch { }
        }

        private void btnCallOff_Click(object sender, EventArgs e)
        {
            Stop = true;
            Loop = false;

            btnCall.Enabled = true;
            btnCall.Visible = true;
            btnCallOff.Enabled = false;
            btnCallOff.Visible = false;

            txtDuration.Enabled = true;
            txtTo.Enabled = true;
            lblCallInfo.Text = $"Đã dừng lại";
        }

        
    }
}
