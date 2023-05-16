using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MXH.MMF
{
    public class MMFAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int MainBalance
        {
            get
            {
                return BalanceInfos == null ? 0
                    : !BalanceInfos.Any() ? 0
                    : BalanceInfos.FirstOrDefault(@object => @object.type == 1) == null ? 0
                    : BalanceInfos.FirstOrDefault(@object => @object.type == 1).balance;
            }
        }
        public string MainBalanceExpire
        {
            get
            {
                return BalanceInfos == null ? string.Empty
                    : !BalanceInfos.Any() ? string.Empty
                    : BalanceInfos.FirstOrDefault(@object => @object.type == 1) == null ? string.Empty
                    : BalanceInfos.FirstOrDefault(@object => @object.type == 1).expireDate;
            }
        }
        public string FullName { get; set; }
        public string Token { get; set; }
        public int UserID { get; set; }
        public bool HasPass { get; set; }
        public string IDNumber
        {
            get
            {
                return PersonInfo == null ? string.Empty
                    : PersonInfo.idNumber;
            }
        }

        public string IDPlace
        {
            get
            {
                return PersonInfo == null ? string.Empty
                    : PersonInfo.issuePlace;
            }
        }

        public string RankName
        {
            get
            {
                return MembershipInfo == null ? string.Empty
                    : MembershipInfo.rankName;
            }
        }
        public int CurrentPoint
        {
            get
            {
                return MembershipInfo == null ? 0
                    : MembershipInfo.point;
            }
        }

        public List<MMFAccountInfo> BalanceInfos { get; set; }
        public MMFPersonInfo PersonInfo { get; set; }
        public MMFMembershipInfo MembershipInfo { get; set; }
        public MMFAccount Register(string username = "")
        {
            MMFAccount result = null;
            int failRemain = 15;
            string lastFailedMessage = string.Empty;
            try
            {
                var queue = MMFGlobarVar.RegisterVar.GetQueue();

                loop:
                if (GlobalVar.IsApplicationExit)
                    return null;
                if (queue == null || queue.QueueState != MMFRegisterQueueState.Processing)
                    return null;
                if (failRemain == 0)
                {
                    GlobalEvent.OnGlobalMessaging($"{Username} -> đăng ký thất bại");
                    return result;
                }

                try
                {
                    username = queue.PhoneNumber;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing);

                    using (HttpClientHandler handler = new HttpClientHandler())
                    {
                        handler.CookieContainer = new System.Net.CookieContainer();
                        handler.UseCookies = true;
                        handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.None;
                        var proxyInfo = new MXHPortal().RequestProxy(ProxyFor.MMF);
                        if (proxyInfo == null)
                            goto loop;
                        var webProxy = new WebProxy("zproxy.lum-superproxy.io:22225")
                        {
                            Credentials = new NetworkCredential(proxyInfo.IP, "j8706zdttbxm"),
                        };
                        handler.UseProxy = true;
                        handler.Proxy = webProxy;
                        using (HttpClient client = new HttpClient(handler))
                        {
                            if (queue == null || queue.QueueState != MMFRegisterQueueState.Processing)
                                return null;
                            string otp = string.Empty;
                            GSMControlCenter.OnNewMessage += (gsmMessage)
                                =>
                            {
                                if (gsmMessage.Receiver == username)
                                {
                                    otp = gsmMessage.OTP;
                                }
                            };

                            client.DefaultRequestHeaders.Clear();
                            client.DefaultRequestHeaders.TryAddWithoutValidation("phone", username);
                            client.DefaultRequestHeaders.TryAddWithoutValidation("apisecret", "UEJ34gtH345DFG45G3ht1");

                            Dictionary<string, string> body = new Dictionary<string, string>();
                            body.Add("phone", username);

                            var request = client.PostAsync("https://api.mobifone.vn/api/auth/getloginotp", new FormUrlEncodedContent(body)).Result;
                            string response = request.Content.ReadAsStringAsync().Result;
                            if (queue == null || queue.QueueState != MMFRegisterQueueState.Processing)
                                return null;
                            bool hasError = JObject.Parse(response).SelectToken("errors").HasValues;
                            if (!hasError)
                            {
                                DateTime otpRequestTime = DateTime.Now;
                                while ((DateTime.Now - otpRequestTime).TotalSeconds <
                                    MMFGlobarVar.RegisterVar.OTPTimeout && !GlobalVar.IsApplicationExit && !MMFGlobarVar.RegisterVar.Stop)
                                {
                                    if (GlobalVar.IsApplicationExit)
                                        break;
                                    if (queue == null ||
                                        queue.QueueState != MMFRegisterQueueState.Processing)
                                        break;
                                    if (!string.IsNullOrEmpty(otp))
                                        break;
                                    Thread.Sleep(1000);
                                }
                                if (queue == null || queue.QueueState != MMFRegisterQueueState.Processing)
                                    return null;

                                if (string.IsNullOrEmpty(otp))
                                {
                                    queue.Resolved = true;
                                    queue.QueueState = MMFRegisterQueueState.Failed;
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.NoOTP);
                                    return null;
                                }

                                body.Clear();
                                body.Add("phone", username);
                                body.Add("otp", otp);

                                request = client.PostAsync("https://api.mobifone.vn/api/auth/otplogin", new FormUrlEncodedContent(body)).Result;
                                if (queue == null || queue.QueueState != MMFRegisterQueueState.Processing)
                                    return null;
                                response = request.Content.ReadAsStringAsync().Result;
                                hasError = JObject.Parse(response).SelectToken("errors").HasValues;

                                if (!hasError)
                                {
                                    Username = username;
                                    Token = JObject.Parse(response).SelectToken("data.apiSecret").Value<string>();
                                    UserID = JObject.Parse(response).SelectToken("data.userId").Value<int>();
                                    HasPass = JObject.Parse(response).SelectToken("data.hasPass").Value<int>() == 1;
                                    FullName = JObject.Parse(response).SelectToken("data.name").Value<string>();
                                    if (!HasPass)
                                    {
                                        client.DefaultRequestHeaders.Clear();
                                        client.DefaultRequestHeaders.TryAddWithoutValidation("phone", Username.StartsWith("0") ? Username.Remove(0, 1) : Username);
                                        client.DefaultRequestHeaders.TryAddWithoutValidation("userid", UserID.ToString());
                                        client.DefaultRequestHeaders.TryAddWithoutValidation("apisecret", Token);

                                        body.Clear();
                                        body.Add("password", SecureHelper.Sha256Hash(MMFGlobarVar.RegisterVar.Password));

                                        request = client.PostAsync("https://api.mobifone.vn/api/auth/changepassword", new FormUrlEncodedContent(body)).Result;
                                        response = request.Content.ReadAsStringAsync().Result;
                                        hasError = JObject.Parse(response).SelectToken("errors").HasValues;
                                        if (!hasError)
                                        {
                                            Password = MMFGlobarVar.RegisterVar.Password;
                                            GetBalanceInfo();
                                            GetPersonInfo();
                                            GetMembershipInfo();
                                            result = this;
                                            MMFGlobarVar.Accounts.Add(result);
                                            queue.QueueState = MMFRegisterQueueState.Succeed;
                                            queue.Resolved = true;
                                            GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Succeed);
                                        }
                                        else
                                        {
                                            string errorMsg = GetLastError(response);
                                            GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {errorMsg}");
                                            queue.Resolved = true;
                                            queue.QueueState = MMFRegisterQueueState.Failed;
                                            GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed);

                                        }
                                    }
                                    else
                                    {
                                        otp = string.Empty;

                                        client.DefaultRequestHeaders.Clear();
                                        client.DefaultRequestHeaders.TryAddWithoutValidation("phone", Username.StartsWith("0") ? Username.Remove(0, 1) : Username);
                                        client.DefaultRequestHeaders.TryAddWithoutValidation("userid", UserID.ToString());
                                        client.DefaultRequestHeaders.TryAddWithoutValidation("apisecret", "UEJ34gtH345DFG45G3ht1");

                                        body.Clear();
                                        body.Add("phone", Username.StartsWith("0") ? Username.Remove(0, 1) : Username);
                                        body.Add("prefix", "forgetOTP");

                                        request = client.PostAsync("https://api.mobifone.vn/api/auth/getotp", new FormUrlEncodedContent(body)).Result;
                                        response = request.Content.ReadAsStringAsync().Result;
                                        hasError = JObject.Parse(response).SelectToken("errors").HasValues;
                                        if (!hasError)
                                        {
                                            otpRequestTime = DateTime.Now;
                                            while ((DateTime.Now - otpRequestTime).TotalSeconds <
                                                MMFGlobarVar.RegisterVar.OTPTimeout && !GlobalVar.IsApplicationExit && !MMFGlobarVar.RegisterVar.Stop)
                                            {
                                                if (GlobalVar.IsApplicationExit)
                                                    break;
                                                if (queue == null ||
                                                    queue.QueueState != MMFRegisterQueueState.Processing)
                                                    break;
                                                if (!string.IsNullOrEmpty(otp))
                                                    break;
                                                Thread.Sleep(1000);
                                            }
                                            if (queue == null || queue.QueueState != MMFRegisterQueueState.Processing)
                                                return null;

                                            if (string.IsNullOrEmpty(otp))
                                            {
                                                queue.Resolved = true;
                                                queue.QueueState = MMFRegisterQueueState.Failed;
                                                GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.NoOTP);

                                            }

                                            body.Clear();
                                            body.Add("phone", Username.StartsWith("0") ? Username.Remove(0, 1) : Username);
                                            body.Add("otp", otp);
                                            body.Add("password", SecureHelper.Sha256Hash(MMFGlobarVar.RegisterVar.Password));

                                            request = client.PostAsync("https://api.mobifone.vn/api/auth/recoverpass", new FormUrlEncodedContent(body)).Result;
                                            response = request.Content.ReadAsStringAsync().Result;
                                            hasError = JObject.Parse(response).SelectToken("errors").HasValues;
                                            if (!hasError)
                                            {
                                                Password = MMFGlobarVar.RegisterVar.Password;
                                                GetBalanceInfo();
                                                GetPersonInfo();
                                                GetMembershipInfo();
                                                result = this;
                                                MMFGlobarVar.Accounts.Add(result);
                                                queue.QueueState = MMFRegisterQueueState.Succeed;
                                                queue.Resolved = true;
                                                GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Succeed);

                                            }
                                            else
                                            {
                                                string errorMsg = GetLastError(response);
                                                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {errorMsg}");
                                                queue.Resolved = true;
                                                queue.QueueState = MMFRegisterQueueState.Failed;
                                                GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed);
                                            }
                                        }
                                        else
                                        {
                                            string errorMsg = GetLastError(response);
                                            GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {errorMsg}");
                                            queue.Resolved = true;
                                            queue.QueueState = MMFRegisterQueueState.Failed;
                                            GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed);
                                        }

                                    }
                                }
                                else
                                {
                                    string errorMsg = GetLastError(response);
                                    GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {errorMsg}");
                                    queue.Resolved = true;
                                    queue.QueueState = MMFRegisterQueueState.Failed;
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed);

                                }
                            }
                            else
                            {
                                string errorMsg = GetLastError(response);
                                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {errorMsg}");
                                queue.Resolved = true;
                                queue.QueueState = MMFRegisterQueueState.Failed;
                                GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed);

                            }
                        }
                    }
                }
                catch
                {
                    Token = string.Empty;
                    failRemain--;
                    goto loop;
                }
            }
            catch
            {

            }
            finally
            {
                MMFGlobarVar.RegisterVar.OnEachCompleted();
            }
            return result;
        }
        public MMFAccount Login(bool loadFullInfo = true)
        {
            MMFAccount result = null;
            try
            {
                if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Username))
                {
                    GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> Username/Password required for login");
                    return null;
                }

                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    handler.CookieContainer = new System.Net.CookieContainer();
                    handler.UseCookies = true;
                    handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.None;
                    loop:
                    var proxyInfo = new MXHPortal().RequestProxy(ProxyFor.MMF);
                    if (proxyInfo == null)
                        goto loop;
                    var webProxy = new WebProxy("zproxy.lum-superproxy.io:22225")
                    {
                        Credentials = new NetworkCredential(proxyInfo.IP, "j8706zdttbxm"),
                    };
                    handler.UseProxy = true;
                    handler.Proxy = webProxy;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        Dictionary<string, string> body = new Dictionary<string, string>();
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.TryAddWithoutValidation("phone", Username);
                        client.DefaultRequestHeaders.TryAddWithoutValidation("apisecret", "UEJ34gtH345DFG45G3ht1");

                        body.Add("phone", Username);
                        body.Add("password", SecureHelper.Sha256Hash(Password));

                        var request = client.PostAsync("https://api.mobifone.vn/api/auth/passwordlogin", new FormUrlEncodedContent(body)).Result;
                        string response = request.Content.ReadAsStringAsync().Result;
                        bool hasError = JObject.Parse(response).SelectToken("errors").HasValues;
                        if (!hasError)
                        {
                            Token = JObject.Parse(response).SelectToken("data.apiSecret").Value<string>();
                            UserID = JObject.Parse(response).SelectToken("data.userId").Value<int>();
                            HasPass = JObject.Parse(response).SelectToken("data.hasPass").Value<int>() == 1;
                            FullName = JObject.Parse(response).SelectToken("data.name").Value<string>();
                            if (loadFullInfo)
                            {
                                GetBalanceInfo();
                                GetPersonInfo();
                                GetMembershipInfo();
                            }
                            result = this;
                        }
                        else
                        {
                            string errorMsg = GetLastError(response);
                            GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {errorMsg}");
                        }
                    }
                }

            }
            catch (Exception exception)
            {
                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {exception.Message}");
            }
            return result;
        }
        private string GetLastError(string response)
        {
            string result = string.Empty;
            try
            {
                result = JObject.Parse(response).SelectToken("errors[0].message").Value<string>();
            }
            catch { }
            return result;
        }
        public bool ChangePassword(string newPassword)
        {
            try
            {
                MMFAccount account = null;
                if (string.IsNullOrEmpty(Token))
                {
                    account = Login();
                }
                if (account != null)
                {
                    using (HttpClientHandler handler = new HttpClientHandler())
                    {
                        handler.CookieContainer = new System.Net.CookieContainer();
                        handler.UseCookies = true;
                        handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.None;
                        loop:
                        var proxyInfo = new MXHPortal().RequestProxy(ProxyFor.MMF);
                        if (proxyInfo == null)
                            goto loop;
                        var webProxy = new WebProxy("zproxy.lum-superproxy.io:22225")
                        {
                            Credentials = new NetworkCredential(proxyInfo.IP, "j8706zdttbxm"),
                        };
                        handler.UseProxy = true;
                        handler.Proxy = webProxy;
                        using (HttpClient client = new HttpClient(handler))
                        {
                            Dictionary<string, string> body = new Dictionary<string, string>();

                            client.DefaultRequestHeaders.Clear();
                            client.DefaultRequestHeaders.TryAddWithoutValidation("phone", Username.StartsWith("0") ? Username.Remove(0, 1) : Username);
                            client.DefaultRequestHeaders.TryAddWithoutValidation("userid", UserID.ToString());
                            client.DefaultRequestHeaders.TryAddWithoutValidation("apisecret", Token);

                            body.Clear();
                            body.Add("password", SecureHelper.Sha256Hash(newPassword));
                            body.Add("old_password", SecureHelper.Sha256Hash(Password));

                            var request = client.PostAsync("https://api.mobifone.vn/api/auth/changepassword2", new FormUrlEncodedContent(body)).Result;
                            string response = request.Content.ReadAsStringAsync().Result;
                            bool hasError = JObject.Parse(response).SelectToken("errors").HasValues;
                            if (!hasError)
                            {
                                Password = newPassword;
                                return true;
                            }
                            else
                            {
                                string errorMsg = GetLastError(response);
                                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {errorMsg}");
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {exception.Message}");
            }
            return false;
        }
        private void GetBalanceInfo()
        {
            try
            {
                if (!string.IsNullOrEmpty(Token))
                {
                    using (HttpClientHandler handler = new HttpClientHandler())
                    {
                        handler.CookieContainer = new System.Net.CookieContainer();
                        handler.UseCookies = true;
                        handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.None;
                        loop:
                        var proxyInfo = new MXHPortal().RequestProxy(ProxyFor.MMF);
                        if (proxyInfo == null)
                            goto loop;
                        var webProxy = new WebProxy("zproxy.lum-superproxy.io:22225")
                        {
                            Credentials = new NetworkCredential(proxyInfo.IP, "j8706zdttbxm"),
                        };
                        handler.UseProxy = true;
                        handler.Proxy = webProxy;
                        using (HttpClient client = new HttpClient(handler))
                        {
                            Dictionary<string, string> body = new Dictionary<string, string>();

                            client.DefaultRequestHeaders.Clear();
                            client.DefaultRequestHeaders.TryAddWithoutValidation("phone", Username.StartsWith("0") ? Username.Remove(0, 1) : Username);
                            client.DefaultRequestHeaders.TryAddWithoutValidation("userid", UserID.ToString());
                            client.DefaultRequestHeaders.TryAddWithoutValidation("apisecret", Token);

                            body.Clear();
                            var request = client.PostAsync("https://api.mobifone.vn/api/user/getprofile", new FormUrlEncodedContent(body)).Result;
                            string response = request.Content.ReadAsStringAsync().Result;
                            bool hasError = JObject.Parse(response).SelectToken("errors").HasValues;
                            if (!hasError)
                            {
                                BalanceInfos = JObject.Parse(response).SelectToken("data").ToObject<List<MMFAccountInfo>>();
                            }
                            else
                            {
                                string errorMsg = GetLastError(response);
                                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {errorMsg}");
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {exception.Message}");
            }
        }
        public void GetPersonInfo()
        {
            try
            {
                if (!string.IsNullOrEmpty(Token))
                {
                    using (HttpClientHandler handler = new HttpClientHandler())
                    {
                        handler.CookieContainer = new System.Net.CookieContainer();
                        handler.UseCookies = true;
                        handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.None;
                    loop:
                        if (GlobalVar.IsApplicationExit)
                            return;
                        var proxyInfo = new MXHPortal().RequestProxy(ProxyFor.MMF);
                        if (proxyInfo == null)
                            goto loop;
                        var webProxy = new WebProxy("zproxy.lum-superproxy.io:22225")
                        {
                            Credentials = new NetworkCredential(proxyInfo.IP, "j8706zdttbxm"),
                        };
                        handler.UseProxy = true;
                        handler.Proxy = webProxy;
                        using (HttpClient client = new HttpClient(handler))
                        {
                            Dictionary<string, string> body = new Dictionary<string, string>();

                            client.DefaultRequestHeaders.Clear();
                            client.DefaultRequestHeaders.TryAddWithoutValidation("phone", Username.StartsWith("0") ? Username.Remove(0, 1) : Username);
                            client.DefaultRequestHeaders.TryAddWithoutValidation("userid", UserID.ToString());
                            client.DefaultRequestHeaders.TryAddWithoutValidation("apisecret", Token);

                            body.Clear();
                            var request = client.PostAsync("https://api.mobifone.vn/api/user/getphoneinfo", new FormUrlEncodedContent(body)).Result;
                            string response = request.Content.ReadAsStringAsync().Result;
                            bool hasError = JObject.Parse(response).SelectToken("errors").HasValues;
                            if (!hasError)
                            {
                                PersonInfo = JObject.Parse(response).SelectToken("data").ToObject<MMFPersonInfo>();
                            }
                            else
                            {
                                string errorMsg = GetLastError(response);
                                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {errorMsg}");
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {exception.Message}");
            }
        }
        private void GetMembershipInfo()
        {
            try
            {
                if (!string.IsNullOrEmpty(Token))
                {
                    using (HttpClientHandler handler = new HttpClientHandler())
                    {
                        handler.CookieContainer = new System.Net.CookieContainer();
                        handler.UseCookies = true;
                        handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.None;
                        loop:
                        if (GlobalVar.IsApplicationExit)
                            return;
                        var proxyInfo = new MXHPortal().RequestProxy(ProxyFor.MMF);
                        if (proxyInfo == null)
                            goto loop;
                        var webProxy = new WebProxy("zproxy.lum-superproxy.io:22225")
                        {
                            Credentials = new NetworkCredential(proxyInfo.IP, "j8706zdttbxm"),
                        };
                        handler.UseProxy = true;
                        handler.Proxy = webProxy;
                        using (HttpClient client = new HttpClient(handler))
                        {
                            Dictionary<string, string> body = new Dictionary<string, string>();

                            client.DefaultRequestHeaders.Clear();
                            client.DefaultRequestHeaders.TryAddWithoutValidation("phone", Username.StartsWith("0") ? Username.Remove(0, 1) : Username);
                            client.DefaultRequestHeaders.TryAddWithoutValidation("userid", UserID.ToString());
                            client.DefaultRequestHeaders.TryAddWithoutValidation("apisecret", Token);

                            body.Clear();
                            var request = client.PostAsync("https://api.mobifone.vn/api/user/getmemberinfo", new FormUrlEncodedContent(body)).Result;
                            string response = request.Content.ReadAsStringAsync().Result;
                            bool hasError = JObject.Parse(response).SelectToken("errors").HasValues;
                            if (!hasError)
                            {
                                MembershipInfo = JObject.Parse(response).SelectToken("data").ToObject<MMFMembershipInfo>();
                            }
                            else
                            {
                                string errorMsg = GetLastError(response);
                                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {errorMsg}");
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {exception.Message}");
            }
        }
    }

    public class MMFAccountInfo
    {
        public string phone { get; set; }
        public string title { get; set; }
        public int balance { get; set; }
        public int type { get; set; }
        public string period { get; set; }
        public string expireDate { get; set; }
    }

    public class MMFPersonInfo
    {
        public string phone { get; set; }
        public string fullname { get; set; }
        public string typeText { get; set; }
        public string birthDate { get; set; }
        public string idNumber { get; set; }
        public string genderText { get; set; }
        public string issuePlace { get; set; }
    }
    public class MMFMembershipInfo
    {
        public string memberName { get; set; }
        public string rankName { get; set; }
        public int point { get; set; }
        public int type { get; set; }
    }
}
