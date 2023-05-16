using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using MXH.Account;
using MXH.MMF;
using MXH.MVT;

namespace MXH
{
    public partial class MainUI : XtraForm
    {
        Thread paintThread;

        public MainUI()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
        }

        //cmt load AT, GSM
        private void MainUI_Load(object sender, EventArgs e)
        {

            //btnUser.Text = $"{AccountHelper.GetCurrentAccount().Phone}";

            Application.ApplicationExit += (_sender, @event) =>
            {
                GlobalVar.IsApplicationExit = true;
                GSMControlCenter.Dispose();
                GlobalVar.ForceKillMyself();
            };


            gridControl1.DataSource = GSMControlCenter.GSMComs;
            gridSMS.DataSource = GSMControlCenter.GSMMessages;
            //DELETE CMT
            /*grdMVT.DataSource = MVTGlobalVar.Accounts;*//*
            grdMVTVoucherExchanged.DataSource = MVTGlobalVar.MVTVoucherInfos;
            grdMMF.DataSource = MMFGlobarVar.Accounts;*/


            //CMT load data gsm
            paintThread = new Thread(new ThreadStart(() =>
            {
                while (!GlobalVar.IsApplicationExit)
                {
                    try
                    {
                        gridControl1.RefreshDataSource();
                        /*//DELETE CMT
                        *//*grdMVT.RefreshDataSource();*//*
                        grdMVTVoucherExchanged.RefreshDataSource();*//*
                        grdMMF.RefreshDataSource();*/
                    }
                    catch { }
                    Thread.Sleep(1000);
                }
            }));
            paintThread.Start();

            GlobalEvent.OnGlobalMessaging += OnGlobalMessaging;
            GlobalEvent.ONATCommandResponse += ONATCommandResponse;

            GSMControlCenter.Start();

           /* MVTGlobalVar.StartHanding();
            MMFGlobarVar.StartHanding();*/
        }

        public void OnGlobalMessaging(string message)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                try
                {
                    if (chkRecord.Checked)
                    {
                        txtGlobalMessage.AppendText("\n" + message);
                        txtGlobalMessage.SelectionStart = txtGlobalMessage.Text.Length;
                        txtGlobalMessage.ScrollToCaret();
                    }
                }
                catch { }
            }));
        }


        // AT CMT
        public void ONATCommandResponse(string response)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                try
                {
                    if (chkRecord.Checked)
                    {
                        txtAtcommandLog.AppendText("\n" + response);
                        txtAtcommandLog.SelectionStart = txtAtcommandLog.Text.Length;
                        txtAtcommandLog.ScrollToCaret();
                    }
                }
                catch { }
            }));
        }


        private void btnCheckBalance_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            List<Task> tasks = new List<Task>();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    tasks.Add(new Task(
                        () => { ((GSMCom)row).CheckBalance(); }
                        ));
                }
            }
            new Task(() =>
            {
                tasks.ForEach(task => task.Start());
                /*Task.WaitAll(tasks.ToArray());*/
            }).Start();
        }

        private void btnClearConsole_Click(object sender, EventArgs e)
        {
            txtAtcommandLog.Clear();
            txtGlobalMessage.Clear();
        }

        private void btnReconnect_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            List<Task> tasks = new List<Task>();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    tasks.Add(new Task(
                        () => { ((GSMCom)row).Reconnect(); }
                        ));
                }
            }
            new Task(() =>
            {
                tasks.ForEach(task => task.Start());
                /*Task.WaitAll(tasks.ToArray());*/
            }).Start();
        }

        private void btnCopyLog_Click(object sender, EventArgs e)
        {
            try
            {
                if (tabControlLog.SelectedTabPage == tabPageGlobalLog)
                {
                    Clipboard.SetText(txtGlobalMessage.Text);
                }
                if (tabControlLog.SelectedTabPage == tabPageATCommandLog)
                {
                    Clipboard.SetText(txtAtcommandLog.Text);
                }
            }
            catch { }
        }

        private void ckRealtimeSMSTracking_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVar.RealtimeSMSTracking = ckRealtimeSMSTracking.Checked;
        }

        private void btnClearViewMessages_Click(object sender, EventArgs e)
        {
            lock (GSMControlCenter.LockGSMMessages)
            {
                GSMControlCenter.GSMMessages.Clear();
            }
        }

        // DELETE
       /* private void btnMVTRegister_Click(object sender, EventArgs e)
        {
            MVTGlobalVar.RegisterVar.Reset();
            if (string.IsNullOrEmpty(txtMVTRegPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu để [đăng ký/đặt lại mật khẩu]", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Regex.IsMatch(txtMVTRegPassword.Text, "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{6,}$"))
            {
                MessageBox.Show("Mật khẩu phải chứa ít nhất 6 ký tự, bao gồm [chữ in hoa, chữ thường, số, ký tự đặc biệt]", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            btnMVTRegister.Enabled = false;
            txtMVTRegPassword.Enabled = false;
            btnMVTSopRegister.Enabled = true;
            MVTGlobalVar.RegisterVar.Password = txtMVTRegPassword.Text;
            new Thread(new ThreadStart(() =>
            {
            loop:
                if (GlobalVar.IsApplicationExit)
                    return;
                if (MVTGlobalVar.RegisterVar.Stop)
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            btnMVTRegister.Enabled = true;
                            txtMVTRegPassword.Enabled = true;
                            btnMVTSopRegister.Enabled = false;
                        }
                        catch { }
                    }));
                    return;
                }




                while (MVTGlobalVar.RegisterVar.RunningThread
                < MVTGlobalVar.RegisterVar.TotalThread && !GlobalVar.IsApplicationExit)
                {
                    if (MVTGlobalVar.RegisterVar.Stop)
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            try
                            {
                                btnMVTRegister.Enabled = true;
                                btnMVTSopRegister.Enabled = false;
                                txtMVTRegPassword.Enabled = true;

                            }
                            catch { }
                        }));
                        return;
                    }

                    if (!MVTGlobalVar.RegisterVar.HasQueue())
                    {
                        Thread.Sleep(1000);
                        continue;
                    }


                    lock (MVTGlobalVar.RegisterVar.LockRunningThread)
                    {
                        MVTGlobalVar.RegisterVar.RunningThread++;
                    }
                    try
                    {
                        new Thread(new ThreadStart(() =>
                        {
                            try
                            {
                                new MVTAccount().Register();
                                Thread.CurrentThread.Abort();
                            }
                            catch { }
                        })).Start();
                    }
                    catch (Exception ex)
                    {
                        GlobalEvent.OnGlobalMessaging(MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                    }
                    Thread.Sleep(1000);
                }
                Thread.Sleep(1000);
                goto loop;
            })).Start();
        }
*/
        private void btnMVTSopRegister_Click(object sender, EventArgs e)
        {
            MVTGlobalVar.RegisterVar.Stop = true;
        }

        //DELETE CMT
        /*private void btnMVTCheckBalance_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewMVTAccounts.GetSelectedRows();
            if (MessageBox.Show($"Có {selectedRowsHandle.Count()} tài khoản sẽ được kiểm tra thông tin, Xác nhận?", "Xác nhận",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                == DialogResult.OK)
            {
                foreach (int rowHandle in selectedRowsHandle)
                {
                    var row = viewMVTAccounts.GetRow(rowHandle);
                    if (row != null)
                    {
                        GlobalVar.AddSequence(new ObjectSequence()
                        {
                            Object = (MVTAccount)row,
                            ObjectSequenceState = ObjectSequenceState.New,
                            ObjectSequenceType = ObjectSequenceType.MVTUpdateInfo,
                            SeqTime = DateTime.Now
                        });
                    }
                }
            }
        }
*/
        
        //DELETE CMT
        /*private void btnExchangeVoucher_Click(object sender, EventArgs e)
        {

            int[] selectedRowsHandle = viewMVTAccounts.GetSelectedRows();
            if (string.IsNullOrEmpty(txtMVTVoucherID.Text))
            {
                MessageBox.Show("Vui lòng nhập mã ưu đãi & kiểm tra trước khi thực hiện đổi mã", "Yêu cầu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!selectedRowsHandle.Any())
            {
                MessageBox.Show("Vui lòng chọn tài khoản sẽ tham gia đổi mã", "Yêu cầu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var promotionInfo = MVTHelper.GetPromotionInfo(txtMVTVoucherID.Text);
            if (promotionInfo == null)
            {
                MessageBox.Show($"Không tìm thấy thông tin ưu đãi {txtMVTVoucherID.Text}", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (new MVT.ExchangeOptionUI().ShowDialog() == DialogResult.Cancel)
                return;

            //15862
            List<ObjectSequence> ObjectSequences = new List<ObjectSequence>();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewMVTAccounts.GetRow(rowHandle);
                if (row != null)
                {
                    ObjectSequences.Add(new ObjectSequence()
                    {
                        Object = new ObjectMVTExchangeVoucherSequence() { MVTAccount = (MVTAccount)row, PromotionInfo = promotionInfo },
                        ObjectSequenceState = ObjectSequenceState.New,
                        ObjectSequenceType = ObjectSequenceType.MVTExchangeVoucher,
                        SeqTime = DateTime.Now
                    });
                }
            }
            GlobalVar.AddSequences(ObjectSequences);
        }
*/
        private void btnAbout_Click(object sender, EventArgs e)
        {
            new AboutUI().ShowDialog(this);
        }

        private void btnSendSMS_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            List<string> senders = new List<string>();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    var com = ((GSMCom)row);
                    if (com.IsPortConnected && com.IsSIMConnected
                        && !string.IsNullOrEmpty(com.PhoneNumber))
                    {
                        senders.Add(com.PhoneNumber);
                    }
                }
            }
            if (!senders.Any())
            {
                MessageBox.Show("Vui lòng chọn sim cần gửi tin nhắn (Giữ ctrl hoặc shift để chọn nhiều)", "Cảnh báo", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            new SendSMSUI(senders).ShowDialog(this);
        }

        private void txtMVTRegPassword_EditValueChanged(object sender, EventArgs e)
        {
            //MVTGlobalVar.ChangePasswordVar.NewPassword = txtMVTRegPassword.Text;
        }

        // DELETE CMT
       /* private void btnChangePassword_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(txtMVTRegPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu muốn đổi", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Regex.IsMatch(txtMVTRegPassword.Text, "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{6,}$"))
            {
                MessageBox.Show("Mật khẩu phải chứa ít nhất 6 ký tự, bao gồm [chữ in hoa, chữ thường, số, ký tự đặc biệt]", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MVTGlobalVar.ChangePasswordVar.NewPassword = txtMVTRegPassword.Text;
            int[] selectedRowsHandle = viewMVTAccounts.GetSelectedRows();

            if (MessageBox.Show($"{selectedRowsHandle.Count()} accounts will be change to " +
                $"\"{MVTGlobalVar.ChangePasswordVar.NewPassword}\", Are you sure?", "Question",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                == DialogResult.OK)
            {
                foreach (int rowHandle in selectedRowsHandle)
                {
                    var row = viewMVTAccounts.GetRow(rowHandle);
                    if (row != null)
                    {
                        GlobalVar.AddSequence(new ObjectSequence()
                        {
                            Object = (MVTAccount)row,
                            ObjectSequenceState = ObjectSequenceState.New,
                            ObjectSequenceType = ObjectSequenceType.MVTChangePassword,
                            SeqTime = DateTime.Now
                        });
                    }
                }
            }
        }
*/
        private void btnMVTImport_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog() { })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);
                    foreach (var line in lines)
                    {
                        string[] attributes = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        if (attributes.Length == 2)
                        {
                            string username = attributes[0];
                            string password = attributes[1];
                            MVTGlobalVar.Accounts.Add(new MVTAccount()
                            {
                                Username = username,
                                Password = password
                            });
                        }
                    }

                }
            }
        }

        //DELETE CMT
        /*private void btnMMFRegister_Click(object sender, EventArgs e)
        {
            MMFGlobarVar.RegisterVar.Reset();
            if (string.IsNullOrEmpty(txtMMFRegPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu để [đăng ký/đặt lại mật khẩu]", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (!Regex.IsMatch(txtMMFRegPassword.Text, "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{6,}$"))
            {
                MessageBox.Show("Mật khẩu phải chứa ít nhất 6 ký tự, bao gồm [chữ in hoa, chữ thường, số, ký tự đặc biệt]", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }



            btnMMFRegister.Enabled = false;
            txtMMFRegPassword.Enabled = false;
            btnMMFSopRegister.Enabled = true;
            MMFGlobarVar.RegisterVar.Password = txtMMFRegPassword.Text;
            new Thread(new ThreadStart(() =>
            {
            loop:
                if (GlobalVar.IsApplicationExit)
                    return;
                if (MMFGlobarVar.RegisterVar.Stop)
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            btnMMFRegister.Enabled = true;
                            txtMMFRegPassword.Enabled = true;
                            btnMMFSopRegister.Enabled = false;
                        }
                        catch { }
                    }));
                    return;
                }

                while (MMFGlobarVar.RegisterVar.RunningThread
                < MMFGlobarVar.RegisterVar.TotalThread && !GlobalVar.IsApplicationExit)
                {
                    if (MMFGlobarVar.RegisterVar.Stop)
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            try
                            {
                                btnMMFRegister.Enabled = true;
                                btnMMFSopRegister.Enabled = false;
                                txtMMFRegPassword.Enabled = true;
                            }
                            catch { }
                        }));
                        return;
                    }

                    if (!MMFGlobarVar.RegisterVar.HasQueue())
                    {
                        Thread.Sleep(1000);
                        continue;
                    }


                    lock (MMFGlobarVar.RegisterVar.LockRunningThread)
                    {
                        MMFGlobarVar.RegisterVar.RunningThread++;
                    }
                    try
                    {
                        new Thread(new ThreadStart(() =>
                        {
                            try
                            {
                                new MMFAccount().Register();
                                Thread.CurrentThread.Abort();
                            }
                            catch { }
                        })).Start();
                    }
                    catch (Exception ex)
                    {
                        GlobalEvent.OnGlobalMessaging(MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                    }
                }
                Thread.Sleep(1000);
                goto loop;
            })).Start();
        }
*/
        private void btnMMFSopRegister_Click(object sender, EventArgs e)
        {
            MMFGlobarVar.RegisterVar.Stop = true;
        }

        //DELETE CMT
        /*private void btnMMFCheckInfo_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewMMFAccounts.GetSelectedRows();
            if (MessageBox.Show($"Có {selectedRowsHandle.Count()} tài khoản sẽ được kiểm tra thông tin, Xác nhận?", "Xác nhận",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                == DialogResult.OK)
            {
                foreach (int rowHandle in selectedRowsHandle)
                {
                    var row = viewMMFAccounts.GetRow(rowHandle);
                    if (row != null)
                    {
                        GlobalVar.AddSequence(new ObjectSequence()
                        {
                            Object = (MMFAccount)row,
                            ObjectSequenceState = ObjectSequenceState.New,
                            ObjectSequenceType = ObjectSequenceType.MMFUpdateInfo,
                            SeqTime = DateTime.Now
                        });
                    }
                }
            }
        }
*/
        
        //DELETE CMT
        /*private void btnMMFChangePassword_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(txtMMFRegPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu để [đăng ký/đặt lại mật khẩu]", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (!Regex.IsMatch(txtMMFRegPassword.Text, "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{6,}$"))
            {
                MessageBox.Show("Mật khẩu phải chứa ít nhất 6 ký tự, bao gồm [chữ in hoa, chữ thường, số, ký tự đặc biệt]", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MMFGlobarVar.ChangePasswordVar.NewPassword = txtMMFRegPassword.Text;

            int[] selectedRowsHandle = viewMMFAccounts.GetSelectedRows();

            if (MessageBox.Show($"Có {selectedRowsHandle.Count()} tài khoản sẽ được đổi sang mật khẩu mới " +
                $"\"{MMFGlobarVar.ChangePasswordVar.NewPassword}\", Xác nhận?", "Xác nhận",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                == DialogResult.OK)
            {
                foreach (int rowHandle in selectedRowsHandle)
                {
                    var row = viewMMFAccounts.GetRow(rowHandle);
                    if (row != null)
                    {
                        GlobalVar.AddSequence(new ObjectSequence()
                        {
                            Object = (MMFAccount)row,
                            ObjectSequenceState = ObjectSequenceState.New,
                            ObjectSequenceType = ObjectSequenceType.MMFChangePassword,
                            SeqTime = DateTime.Now
                        });
                    }
                }
            }
        }
*/
        private void txtMMFRegPassword_EditValueChanged(object sender, EventArgs e)
        {
            //MMFGlobarVar.ChangePasswordVar.NewPassword = txtMMFRegPassword.Text;
        }

        private void btnMMFImportAccount_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog() { })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);
                    foreach (var line in lines)
                    {
                        string[] attributes = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        if (attributes.Length == 2)
                        {
                            string username = attributes[0];
                            string password = attributes[1];
                            MMFGlobarVar.Accounts.Add(new MMFAccount()
                            {
                                Username = username,
                                Password = password
                            });
                        }
                    }

                }
            }
        }

        private void btnEnableAudio_Click(object sender, EventArgs e)
        {
            GlobalVar.EnableIncomingCallRing = !GlobalVar.EnableIncomingCallRing;
            if (GlobalVar.EnableIncomingCallRing)
            {
                btnEnableAudio.ImageOptions.Image = Properties.Resources.speaker_icon_16;
            }
            else
            {
                btnEnableAudio.ImageOptions.Image = Properties.Resources.mute_icon_16;
            }
        }

        //DELETE CMT
       /* private void btnViewVoucher_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMVTVoucherID.Text))
            {
                MessageBox.Show("Vui lòng nhập mã ưu đãi & kiểm tra trước khi thực hiện đổi mã", "Yêu cầu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var promotionInfo = MVTHelper.GetPromotionInfo(txtMVTVoucherID.Text);
            if (promotionInfo == null)
            {
                MessageBox.Show($"Không tìm thấy thông tin ưu đãi {txtMVTVoucherID.Text}", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            new PromotionInfoUI(promotionInfo).ShowDialog();


        }
*/
        private void btnEnableSMSRing_Click(object sender, EventArgs e)
        {
            GlobalVar.EnableSMSRing = !GlobalVar.EnableSMSRing;
            if (GlobalVar.EnableSMSRing)
            {
                btnEnableSMSRing.ImageOptions.Image = Properties.Resources.speaker_icon_16;
            }
            else
            {
                btnEnableSMSRing.ImageOptions.Image = Properties.Resources.mute_icon_16;
            }
        }

        private void ckAutoAnswerIncomingCall_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVar.AutoAnswerIncomingCall = ckAutoAnswerIncomingCall.Checked;
        }

        private void ckEnableVoiceRecognitionTpText_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVar.EnableVoiceRecognitionToText = ckEnableVoiceRecognitionTpText.Checked;
        }

        //CMT XUẤT EXCEL
        //DELETE CMT
        /*private void btnMVTExportAccounts_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                viewMVTAccounts.ExportToXlsx(svd.FileName);
            }
        }
*/
       
        private void btnCallOut_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            List<string> senders = new List<string>();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    var com = ((GSMCom)row);
                    if (com.IsPortConnected && com.IsSIMConnected
                        && !string.IsNullOrEmpty(com.PhoneNumber))
                    {
                        senders.Add(com.PhoneNumber);
                    }
                }
            }

            if (!senders.Any())
            {
                MessageBox.Show("Vui lòng chọn sim thực hiện gọi đi (Giữ ctrl hoặc shift để chọn nhiều)", "Cảnh báo", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            new CallOutUI(senders).ShowDialog(this);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sv = new SaveFileDialog() { Title = "Chọn nơi lưu file" })
                {
                    sv.Filter = "Excel Files|*.xlsx;";
                    if (sv.ShowDialog() == DialogResult.OK)
                    {
                        viewGSM.ExportToXlsx(sv.FileName);
                        if (MessageBox.Show("Bạn có muốn mở file vừa lưu?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            Process.Start(sv.FileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "No application is associated with the specified file for this operation")
                {
                    MessageBox.Show("Bạn chưa cài đặt phần mềm hỗ trợ mở định dạng Excel(xlsx)", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        //EXCEL
        //DELETE CMT
        /*private void btnMMFExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                viewMMFAccounts.ExportToXlsx(svd.FileName);
            }
        }
*/
        
        //DELETE CMT
       /* private void btnMVTVoucherExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                gridView3.ExportToXlsx(svd.FileName);
            }
        }
*/
        private void btnMsgExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = ".xlsx Files (*.xlsx)|*.xlsx";
            if (svd.ShowDialog() == DialogResult.OK)
            {
                viewSMS.ExportToXlsx(svd.FileName);
            }
        }

        private void btnUSSD_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUSSD.Text.Trim()))
                return;
            string ussd = txtUSSD.Text.Trim();
            if (ussd == "*101#")
            {
                btnCheckBalance_Click(sender, e);
                
            }
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            if (selectedRowsHandle.Length == 0)
            {
                MessageBox.Show("SIM bạn chọn chưa sẵn sàng", "Cảnh báo", MessageBoxButtons.OK,
                   MessageBoxIcon.Warning);
                return;
            }

            if (selectedRowsHandle.Length >= 1)
            {
                List<GSMCom> coms = new List<GSMCom>();
                foreach (int rowHandle in selectedRowsHandle)
                {
                    var row = viewGSM.GetRow(rowHandle);
                    if (row != null)
                    {
                        var com = ((GSMCom)row);
                        if (com.IsPortConnected && com.IsSIMConnected
                            && !string.IsNullOrEmpty(com.PhoneNumber))
                        {
                            coms.Add(com);
                        }
                    }
                }

                foreach (var com in coms)
                {
                    com.LastUSSDCommand = string.Empty;
                    com.LastUSSDResult = string.Empty;
                }

                try
                {
                   
                    var tasks = new List<Task>();
                    foreach (var com in coms)
                    {
                        tasks.Add(new Task(() =>
                        {
                            com.ExecuteSingleUSSD(ussd);
                        }));
                    }

                    new Task(() =>
                    {
                        tasks.ForEach(task => task.Start());
                        /*Task.WaitAll(tasks.ToArray());*/

                        this.Invoke(new MethodInvoker(() =>
                        {
                            gridControl1.RefreshDataSource();
                        }));
                    }).Start();
                }
                catch { }
                //if (!senders.Any())
                //{
                //    MessageBox.Show("Vui lòng chọn sim cần gửi tin nhắn (Giữ ctrl hoặc shift để chọn nhiều)", "Cảnh báo", MessageBoxButtons.OK,
                //        MessageBoxIcon.Warning);
                //    return;
                //}


                /*new USSDMulUI(coms).ShowDialog(this);*/


            }
            /*else
            {
                var row = viewGSM.GetRow(selectedRowsHandle[0]);
                if (row != null)
                {
                    var com = ((GSMCom)row);
                    if (com.IsPortConnected && com.IsSIMConnected
                        && !string.IsNullOrEmpty(com.PhoneNumber))
                    {
                        new USSDUI(com).ShowDialog(this);
                    }
                }
            }*/

        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            int[] selectedRowsHandle = viewGSM.GetSelectedRows();
            foreach (int rowHandle in selectedRowsHandle)
            {
                var row = viewGSM.GetRow(rowHandle);
                if (row != null)
                {
                    ((GSMCom)row).DoNotConnect = true;
                }
            }
        }

        private void btnVoice_Click(object sender, EventArgs e)
        {

        }
    }
}
