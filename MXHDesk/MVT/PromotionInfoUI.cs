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
    public partial class PromotionInfoUI : XtraForm
    {
        private MVTPromotionInfo MVTPromotionInfo { get; set; }
        public PromotionInfoUI(MVTPromotionInfo _MVTPromotionInfo)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            this.DialogResult = DialogResult.Cancel;
            MVTPromotionInfo = _MVTPromotionInfo;
        }

        private void PromotionInfoUI_Load(object sender, EventArgs e)
        {
            if (MVTPromotionInfo == null)
                this.Close();
            else
            {
                lblName.Text = MVTPromotionInfo.name;
                txtContent.Text = MVTPromotionInfo.description;
                lblTime.Text = MVTPromotionInfo.endDate;
                lblPrice.Text = MVTPromotionInfo.value;
                lblPoint.Text = MVTPromotionInfo.exchangePoint.ToString() + " điểm";
                if(MVTPromotionInfo.images != null && MVTPromotionInfo.images.Any())
                {
                    pbOImg.Load(MVTPromotionInfo.images.FirstOrDefault());
                }    

            }    

        }
       
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
