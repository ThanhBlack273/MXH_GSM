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

namespace MXH.MVT
{
    public partial class ExchangeOptionUI : XtraForm
    {
        public ExchangeOptionUI()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            string selected = rdMode.Properties.Items[rdMode.SelectedIndex].Description;
            switch (selected)
            {
                case "Dcom 3G (rasdial mode require)":
                    {
                        //if (string.IsNullOrEmpty(rasBook))
                        //{
                        //    MessageBox.Show("Please input your Dcom 3G device", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        //    return;
                        //}
                        MVTGlobalVar.VoucherExchangeVar.VoucherExchangeMode = VoucherExchangeMode.Dcom;
                        NetworkHelper.RasEntryName = cbRasConnections.Text; 
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        break;
                    }
                case "WIFI / Ethernet":
                    {
                    loop:
                        NetworkHelper.DisconnectDcom();
                        if (!NetworkHelper.IsConnectedToInternet())
                        {
                            if (MessageBox.Show("Please check your internet connection", "Warning", MessageBoxButtons.RetryCancel) == DialogResult.Retry)
                            {
                                goto loop;
                            }
                        }
                        else
                        {
                            MVTGlobalVar.VoucherExchangeVar.VoucherExchangeMode = VoucherExchangeMode.Normal;
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        break;
                    }
            }
        }

        private void ExchangeOptionUI_Load(object sender, EventArgs e)
        {
        }
     

        private void btnLoadRas_Click(object sender, EventArgs e)
        {
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
