using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MXH.Account
{
    public partial class LoginUI : XtraForm
    {
        public Action<string, string, AccountSubmit> Submit = (phone, password, submitType) => { };
        public Action<string> Failure = (message) => { };
        public LoginUI()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            lblFalure.Text = string.Empty;
            Submit(txtPhone.Text, txtPassword.Text, AccountSubmit.Login);
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            lblFalure.Text = string.Empty;
            Submit(txtPhone.Text, txtPassword.Text, AccountSubmit.Register);
        }

        private void LoginUI_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            Failure += (message) =>
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    lblFalure.Text = message;
                }));
            };
        }

        private void txtPhone_EditValueChanged(object sender, EventArgs e)
        {
            lblFalure.Text = string.Empty;
        }

        private void txtPassword_EditValueChanged(object sender, EventArgs e)
        {
            lblFalure.Text = string.Empty;
        }
    }
}
