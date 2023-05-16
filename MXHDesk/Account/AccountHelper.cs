using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MXH.Account
{
    public static class AccountHelper
    {
        private static readonly string Server = "http://maxclone.vn:6789/api/Account/";
        private static MXHAccount _CurrentAccount { get; set; }
        public static MXHAccount GetCurrentAccount()
        {
            if (_CurrentAccount == null)
            {
                var loginForm = new LoginUI() { TopMost = true };
                loginForm.Submit += (phone, password, submitType) =>
                {
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            string jsonResponse = PostResponse(submitType.ToString(), new
                            {
                                Sensor = new
                                {
                                    Rds = Guid.NewGuid().ToString(),
                                    ClientTime = DateTime.Now
                                },
                                Data = new { Phone = phone, Password = password }
                            });
                            bool success = JObject.Parse(jsonResponse).SelectToken("Code").Value<int>() == 0;
                            string message = JObject.Parse(jsonResponse).SelectToken("Message").Value<string>();

                            if (success)
                            {
                                _CurrentAccount = JObject.Parse(jsonResponse).SelectToken("Data").ToObject<MXHAccount>();
                                loginForm.Close();
                            }
                            else
                            {
                                loginForm.Failure(message);
                            }
                        }
                    }
                    catch { }
                };
                loginForm.FormClosed += (sender, @event) => { if (_CurrentAccount == null) { GlobalVar.ForceKillMyself(); return; }; };
                loginForm.ShowDialog();
            }
            return _CurrentAccount;
        }

        public static MXHAccount Register(string phone, string password)
        {
            return null;
        }
        public static string GetResponse(string endpoint, object data)
        {
            string result = string.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var encrypted = SecureHelper.EncryptString(JsonConvert.SerializeObject(data));
                    string responseEncrypted = client.GetAsync($"{Server}{endpoint}?data={WebUtility.UrlEncode(encrypted)}").Result.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(responseEncrypted))
                    {
                        result = SecureHelper.DecryptString(JsonConvert.DeserializeObject<string>(responseEncrypted));
                    }
                }
            }
            catch (Exception ex)
            { }
            return result;
        }
        public static string PostResponse(string endpoint, object data)
        {
            string result = string.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var encrypted = SecureHelper.EncryptString(JsonConvert.SerializeObject(data));

                    string responseEncrypted = client.PostAsync($"{Server}{endpoint}", new StringContent(JsonConvert.SerializeObject(encrypted), Encoding.UTF8,
                        "application/json")).Result.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(responseEncrypted))
                    {
                        result = SecureHelper.DecryptString(JsonConvert.DeserializeObject<string>(responseEncrypted));
                    }
                }
            }
            catch (Exception ex)
            { }
            return result;
        }
    }
    public class MXHAccount
    {
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string ProfilePicture { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool IsActivated { get; set; }
        public AccountType AccountType { get; set; }
    }

    public class AccountBalance
    {
        public int TotalRecharge { get; set; }
        public int TotalSpent { get; set; }
        public int TotalRemain { get; set; }
        public BindingList<BalanceTransactionHistory> TransactionHistories { get; set; }
    }

    public class BalanceTransactionHistory
    {
        public BalanceTransactionType TransactionType { get; set; }
        public int Amount { get; set; }
        public string Note { get; set; }
        public string TransactionBy { get; set; }
        public string ReferenceDoc { get; set; }
    }
    public enum BalanceTransactionType
    {
        Recharge, Spent
    }


    public enum AccountType
    {
        None,
        Normal,
        VIP
    }

    public class RegisterRequest
    {
        public string Phone { get; set; }
        public string Password { get; set; }
    }

    public class RegisterResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
