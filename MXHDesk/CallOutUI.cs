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
    public partial class CallOutUI : XtraForm
    {
        private List<string> Callers = new List<string>();
        public string DialNo { get; set; }
        public bool Loop { get { return ckLoop.Checked; } }
        bool Stop = false;


        public CallOutUI()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
        }

        public CallOutUI(List<string> sender)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            Callers = sender;
            this.FormClosing += CallOutUI_FormClosing;
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
                    txtDuration.Enabled = false;
                    txtTo.Enabled = false;
                }
                catch { }

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
                                            /*this.Invoke(new MethodInvoker(() =>
                                            {
                                                try
                                                {

                                                    lblCallInfo.Text = $"{com.PhoneNumber} -> Đang gọi...";
                                                }
                                                catch { }
                                            }));*/
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
                                        /*Task.WaitAll(tasks.ToArray());*/
                                    }
                                    catch { }
                                }).Start();

                                if (Loop)
                                {
                                    try
                                    {
                                        btnCall_Click(null, null);
                                    }
                                    catch { }
                                }
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
                                    if (Loop)
                                    {
                                        try
                                        {
                                            btnCall_Click(null, null);
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
    }
}
