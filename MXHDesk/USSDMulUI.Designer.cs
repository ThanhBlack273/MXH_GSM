namespace MXH
{
    partial class USSDMulUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(USSDMulUI));
            this.txtUSSD = new DevExpress.XtraEditors.TextEdit();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSubmit = new DevExpress.XtraEditors.SimpleButton();
            this.waitScreen = new DevExpress.XtraWaitForm.ProgressPanel();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.viewGSM = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn19 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this.txtUSSD.Properties)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewGSM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtUSSD
            // 
            this.txtUSSD.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtUSSD.EditValue = "*101#";
            this.txtUSSD.Location = new System.Drawing.Point(0, 0);
            this.txtUSSD.Margin = new System.Windows.Forms.Padding(4);
            this.txtUSSD.Name = "txtUSSD";
            this.txtUSSD.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUSSD.Properties.Appearance.Options.UseFont = true;
            this.txtUSSD.Size = new System.Drawing.Size(787, 30);
            this.txtUSSD.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtUSSD);
            this.panel1.Controls.Add(this.btnSubmit);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(883, 32);
            this.panel1.TabIndex = 4;
            // 
            // btnSubmit
            // 
            this.btnSubmit.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSubmit.ImageOptions.Image = global::MXH.Properties.Resources.phone_3_icon_16;
            this.btnSubmit.Location = new System.Drawing.Point(787, 0);
            this.btnSubmit.Margin = new System.Windows.Forms.Padding(4);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnSubmit.Size = new System.Drawing.Size(96, 32);
            this.btnSubmit.TabIndex = 0;
            this.btnSubmit.Text = "USSD ALL";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // waitScreen
            // 
            this.waitScreen.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.waitScreen.Appearance.Options.UseBackColor = true;
            this.waitScreen.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.waitScreen.Caption = "Đang xử lý...";
            this.waitScreen.ContentAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.waitScreen.Description = "Vui lòng đợi";
            this.waitScreen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waitScreen.FrameInterval = 400;
            this.waitScreen.Location = new System.Drawing.Point(2, 2);
            this.waitScreen.Margin = new System.Windows.Forms.Padding(4);
            this.waitScreen.Name = "waitScreen";
            this.waitScreen.Size = new System.Drawing.Size(879, 449);
            this.waitScreen.TabIndex = 6;
            this.waitScreen.Text = "progressPanel1";
            this.waitScreen.Visible = false;
            this.waitScreen.WaitAnimationType = DevExpress.Utils.Animation.WaitingAnimatorType.Ring;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.viewGSM;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(879, 449);
            this.gridControl1.TabIndex = 7;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.viewGSM});
            // 
            // viewGSM
            // 
            this.viewGSM.Appearance.HeaderPanel.Options.UseTextOptions = true;
            this.viewGSM.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.viewGSM.ColumnPanelRowHeight = 49;
            this.viewGSM.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn19,
            this.gridColumn4,
            this.gridColumn1,
            this.gridColumn2});
            this.viewGSM.DetailHeight = 431;
            this.viewGSM.GridControl = this.gridControl1;
            this.viewGSM.Name = "viewGSM";
            this.viewGSM.OptionsClipboard.CopyColumnHeaders = DevExpress.Utils.DefaultBoolean.False;
            this.viewGSM.OptionsSelection.MultiSelect = true;
            this.viewGSM.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CellSelect;
            this.viewGSM.OptionsView.ShowAutoFilterRow = true;
            this.viewGSM.OptionsView.ShowFooter = true;
            this.viewGSM.OptionsView.ShowGroupPanel = false;
            this.viewGSM.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.False;
            // 
            // gridColumn19
            // 
            this.gridColumn19.Caption = "Cổng";
            this.gridColumn19.FieldName = "DisplayName";
            this.gridColumn19.MaxWidth = 70;
            this.gridColumn19.MinWidth = 70;
            this.gridColumn19.Name = "gridColumn19";
            this.gridColumn19.OptionsColumn.AllowEdit = false;
            this.gridColumn19.OptionsColumn.ReadOnly = true;
            this.gridColumn19.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Count, "DisplayName", "{0}")});
            this.gridColumn19.Visible = true;
            this.gridColumn19.VisibleIndex = 0;
            this.gridColumn19.Width = 70;
            // 
            // gridColumn4
            // 
            this.gridColumn4.Caption = "Số điện thoại";
            this.gridColumn4.FieldName = "PhoneNumber";
            this.gridColumn4.MaxWidth = 93;
            this.gridColumn4.MinWidth = 93;
            this.gridColumn4.Name = "gridColumn4";
            this.gridColumn4.OptionsColumn.AllowEdit = false;
            this.gridColumn4.OptionsColumn.ReadOnly = true;
            this.gridColumn4.Visible = true;
            this.gridColumn4.VisibleIndex = 1;
            this.gridColumn4.Width = 93;
            // 
            // gridColumn1
            // 
            this.gridColumn1.Caption = "USSD";
            this.gridColumn1.FieldName = "LastUSSDCommand";
            this.gridColumn1.MinWidth = 70;
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.OptionsColumn.AllowEdit = false;
            this.gridColumn1.OptionsColumn.ReadOnly = true;
            this.gridColumn1.Visible = true;
            this.gridColumn1.VisibleIndex = 2;
            this.gridColumn1.Width = 70;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Caption = "Kết quả";
            this.gridColumn2.FieldName = "LastUSSDResult";
            this.gridColumn2.MinWidth = 23;
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.OptionsColumn.AllowEdit = false;
            this.gridColumn2.OptionsColumn.ReadOnly = true;
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 3;
            this.gridColumn2.Width = 229;
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.gridControl1);
            this.panelControl1.Controls.Add(this.waitScreen);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 32);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(4);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(883, 453);
            this.panelControl1.TabIndex = 8;
            // 
            // USSDMulUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(883, 485);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.panel1);
            this.IconOptions.Icon = ((System.Drawing.Icon)(resources.GetObject("USSDMulUI.IconOptions.Icon")));
            this.LookAndFeel.SkinName = "Visual Studio 2013 Dark";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "USSDMulUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "USSD";
            ((System.ComponentModel.ISupportInitialize)(this.txtUSSD.Properties)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewGSM)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.TextEdit txtUSSD;
        private System.Windows.Forms.Panel panel1;
        private DevExpress.XtraEditors.SimpleButton btnSubmit;
        private DevExpress.XtraWaitForm.ProgressPanel waitScreen;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView viewGSM;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn19;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn4;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraEditors.PanelControl panelControl1;
    }
}