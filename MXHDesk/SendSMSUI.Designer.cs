namespace MXH
{
    partial class SendSMSUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SendSMSUI));
            this.label1 = new System.Windows.Forms.Label();
            this.txtTo = new DevExpress.XtraEditors.TextEdit();
            this.txtContent = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textEdit1 = new DevExpress.XtraEditors.TextEdit();
            this.rgSendMode = new DevExpress.XtraEditors.RadioGroup();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pbSendProcess = new DevExpress.XtraEditors.ProgressBarControl();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSend = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.txtTo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rgSendMode.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSendProcess.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "SDT nhận";
            // 
            // txtTo
            // 
            this.txtTo.Location = new System.Drawing.Point(78, 9);
            this.txtTo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtTo.Name = "txtTo";
            this.txtTo.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtTo.Properties.NullValuePrompt = "Receiver number";
            this.txtTo.Size = new System.Drawing.Size(184, 20);
            this.txtTo.TabIndex = 4;
            this.txtTo.EditValueChanged += new System.EventHandler(this.txtTo_EditValueChanged);
            // 
            // txtContent
            // 
            this.txtContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.txtContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtContent.ForeColor = System.Drawing.SystemColors.Window;
            this.txtContent.Location = new System.Drawing.Point(78, 38);
            this.txtContent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtContent.Name = "txtContent";
            this.txtContent.Size = new System.Drawing.Size(184, 75);
            this.txtContent.TabIndex = 5;
            this.txtContent.Text = "";
            this.txtContent.TextChanged += new System.EventHandler(this.txtContent_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 38);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 16);
            this.label2.TabIndex = 0;
            this.label2.Text = "Nội dung";
            // 
            // textEdit1
            // 
            this.textEdit1.EditValue = "1";
            this.textEdit1.Location = new System.Drawing.Point(78, 158);
            this.textEdit1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Size = new System.Drawing.Size(29, 22);
            this.textEdit1.TabIndex = 8;
            // 
            // rgSendMode
            // 
            this.rgSendMode.Location = new System.Drawing.Point(78, 121);
            this.rgSendMode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rgSendMode.Name = "rgSendMode";
            this.rgSendMode.Properties.AllowFocused = false;
            this.rgSendMode.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Đồng thời"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Tuần tự")});
            this.rgSendMode.Size = new System.Drawing.Size(184, 30);
            this.rgSendMode.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 127);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 16);
            this.label5.TabIndex = 0;
            this.label5.Text = "Chế độ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 161);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 16);
            this.label3.TabIndex = 0;
            this.label3.Text = "Giản cách";
            // 
            // pbSendProcess
            // 
            this.pbSendProcess.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pbSendProcess.Location = new System.Drawing.Point(0, 188);
            this.pbSendProcess.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pbSendProcess.Name = "pbSendProcess";
            this.pbSendProcess.Properties.ProgressViewStyle = DevExpress.XtraEditors.Controls.ProgressViewStyle.Solid;
            this.pbSendProcess.Properties.ShowTitle = true;
            this.pbSendProcess.ShowProgressInTaskBar = true;
            this.pbSendProcess.Size = new System.Drawing.Size(266, 22);
            this.pbSendProcess.TabIndex = 0;
            this.pbSendProcess.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(111, 161);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 16);
            this.label4.TabIndex = 0;
            this.label4.Text = "giây";
            // 
            // btnSend
            // 
            this.btnSend.ImageOptions.Image = global::MXH.Properties.Resources.child_new_post_icon_16;
            this.btnSend.Location = new System.Drawing.Point(172, 158);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSend.Name = "btnSend";
            this.btnSend.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnSend.Size = new System.Drawing.Size(91, 25);
            this.btnSend.TabIndex = 6;
            this.btnSend.Text = "Gửi";
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // SendSMSUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 210);
            this.Controls.Add(this.textEdit1);
            this.Controls.Add(this.rgSendMode);
            this.Controls.Add(this.pbSendProcess);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtContent);
            this.Controls.Add(this.txtTo);
            this.Controls.Add(this.btnSend);
            this.IconOptions.Icon = ((System.Drawing.Icon)(resources.GetObject("SendSMSUI.IconOptions.Icon")));
            this.LookAndFeel.SkinName = "Visual Studio 2013 Dark";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SendSMSUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gửi tin nhắn";
            ((System.ComponentModel.ISupportInitialize)(this.txtTo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rgSendMode.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSendProcess.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.TextEdit txtTo;
        private System.Windows.Forms.RichTextBox txtContent;
        private System.Windows.Forms.Label label2;
        private DevExpress.XtraEditors.SimpleButton btnSend;
        private DevExpress.XtraEditors.RadioGroup rgSendMode;
        private DevExpress.XtraEditors.TextEdit textEdit1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraEditors.ProgressBarControl pbSendProcess;
        private System.Windows.Forms.Label label4;
    }
}