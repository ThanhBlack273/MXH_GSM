namespace MXH
{
    partial class CallOutUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CallOutUI));
            this.label1 = new System.Windows.Forms.Label();
            this.txtTo = new DevExpress.XtraEditors.TextEdit();
            this.label3 = new System.Windows.Forms.Label();
            this.pbSendProcess = new DevExpress.XtraEditors.ProgressBarControl();
            this.label99 = new System.Windows.Forms.Label();
            this.btnCall = new DevExpress.XtraEditors.SimpleButton();
            this.ckLoop = new DevExpress.XtraEditors.CheckEdit();
            this.txtDuration = new System.Windows.Forms.NumericUpDown();
            this.lblCallInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.txtTo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSendProcess.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ckLoop.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDuration)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Gọi đến số";
            // 
            // txtTo
            // 
            this.txtTo.Location = new System.Drawing.Point(100, 15);
            this.txtTo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtTo.Name = "txtTo";
            this.txtTo.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtTo.Properties.NullValuePrompt = "Receiver number";
            this.txtTo.Size = new System.Drawing.Size(128, 20);
            this.txtTo.TabIndex = 4;
            this.txtTo.EditValueChanged += new System.EventHandler(this.txtTo_EditValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 48);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 16);
            this.label3.TabIndex = 0;
            this.label3.Text = "Gác máy sau";
            // 
            // pbSendProcess
            // 
            this.pbSendProcess.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pbSendProcess.Location = new System.Drawing.Point(0, 186);
            this.pbSendProcess.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pbSendProcess.Name = "pbSendProcess";
            this.pbSendProcess.Properties.ProgressViewStyle = DevExpress.XtraEditors.Controls.ProgressViewStyle.Solid;
            this.pbSendProcess.Properties.ShowTitle = true;
            this.pbSendProcess.ShowProgressInTaskBar = true;
            this.pbSendProcess.Size = new System.Drawing.Size(301, 22);
            this.pbSendProcess.TabIndex = 0;
            this.pbSendProcess.Visible = false;
            // 
            // label99
            // 
            this.label99.AutoSize = true;
            this.label99.Location = new System.Drawing.Point(144, 48);
            this.label99.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label99.Name = "label99";
            this.label99.Size = new System.Drawing.Size(30, 16);
            this.label99.TabIndex = 0;
            this.label99.Text = "giây";
            // 
            // btnCall
            // 
            this.btnCall.ImageOptions.Image = global::MXH.Properties.Resources.phone_3_icon_16;
            this.btnCall.Location = new System.Drawing.Point(100, 116);
            this.btnCall.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCall.Name = "btnCall";
            this.btnCall.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnCall.Size = new System.Drawing.Size(128, 33);
            this.btnCall.TabIndex = 6;
            this.btnCall.Text = "Bắt đầu gọi";
            this.btnCall.Click += new System.EventHandler(this.btnCall_Click);
            // 
            // ckLoop
            // 
            this.ckLoop.EditValue = true;
            this.ckLoop.Location = new System.Drawing.Point(100, 76);
            this.ckLoop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ckLoop.Name = "ckLoop";
            this.ckLoop.Properties.AllowFocused = false;
            this.ckLoop.Properties.Caption = "Lặp lại danh sách";
            this.ckLoop.Properties.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.SvgToggle1;
            this.ckLoop.Properties.ValueGrayed = false;
            this.ckLoop.Size = new System.Drawing.Size(128, 26);
            this.ckLoop.TabIndex = 9;
            // 
            // txtDuration
            // 
            this.txtDuration.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.txtDuration.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.txtDuration.Location = new System.Drawing.Point(100, 46);
            this.txtDuration.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtDuration.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.txtDuration.Name = "txtDuration";
            this.txtDuration.Size = new System.Drawing.Size(36, 23);
            this.txtDuration.TabIndex = 10;
            // 
            // lblCallInfo
            // 
            this.lblCallInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblCallInfo.ForeColor = System.Drawing.Color.Lime;
            this.lblCallInfo.Location = new System.Drawing.Point(0, 153);
            this.lblCallInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCallInfo.Name = "lblCallInfo";
            this.lblCallInfo.Size = new System.Drawing.Size(301, 33);
            this.lblCallInfo.TabIndex = 11;
            this.lblCallInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CallOutUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(301, 208);
            this.Controls.Add(this.lblCallInfo);
            this.Controls.Add(this.txtDuration);
            this.Controls.Add(this.ckLoop);
            this.Controls.Add(this.pbSendProcess);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label99);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtTo);
            this.Controls.Add(this.btnCall);
            this.IconOptions.Icon = ((System.Drawing.Icon)(resources.GetObject("CallOutUI.IconOptions.Icon")));
            this.LookAndFeel.SkinName = "Visual Studio 2013 Dark";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "CallOutUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quay số | Gọi đi";
            ((System.ComponentModel.ISupportInitialize)(this.txtTo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSendProcess.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ckLoop.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDuration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.TextEdit txtTo;
        private DevExpress.XtraEditors.SimpleButton btnCall;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraEditors.ProgressBarControl pbSendProcess;
        private System.Windows.Forms.Label label99;
        private DevExpress.XtraEditors.CheckEdit ckLoop;
        private System.Windows.Forms.NumericUpDown txtDuration;
        private System.Windows.Forms.Label lblCallInfo;
    }
}