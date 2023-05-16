using DevExpress.Utils.DirectXPaint;
using DevExpress.Utils.Internal;
using DevExpress.XtraPrinting;
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

namespace MXH.MVT
{
    public class MVTAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string Token { get; set; }

        #region Personal Info
        public string FullName { get { return PersonInfo == null ? string.Empty : PersonInfo.fullName; } }
        public string Birthday { get { return PersonInfo == null ? string.Empty : PersonInfo.birthday; } }
        public string IDNo { get { return PersonInfo == null ? string.Empty : PersonInfo.cmnd_number; } }
        public string IDPlace { get { return PersonInfo == null ? string.Empty : PersonInfo.cmnd_place; } }
        public string IDDate { get { return PersonInfo == null ? string.Empty : PersonInfo.cmnd_date; } }

        #endregion

        public int MainBalance { get { return ListBalanceInfo == null ? 0 : !ListBalanceInfo.Any(p => p.type == "1") ? 0 : ListBalanceInfo.FirstOrDefault(p => p.type == "1").value; } }
        public string MainBalanceExpire
        {
            get
            {
                return ListBalanceInfo == null ? string.Empty : !ListBalanceInfo.Any(p => p.type == "1") ? string.Empty
: ListBalanceInfo.FirstOrDefault(p => p.type == "1").expire;
            }
        }


        #region Data Info
        public string DataPackage { get { return ListDataPackageInfo == null ? string.Empty : !ListDataPackageInfo.Any() ? string.Empty : ListDataPackageInfo.FirstOrDefault().pack_name; } }

        public string DataRemain { get { return ListDataPackageInfo == null ? string.Empty : !ListDataPackageInfo.Any() ? string.Empty : ListDataPackageInfo.FirstOrDefault().remain; } }
        public string DataExpireDate { get { return ListDataPackageInfo == null ? string.Empty : !ListDataPackageInfo.Any() ? string.Empty : ListDataPackageInfo.FirstOrDefault().expireDate; } }

        #endregion

        #region Viettel++ Info
        public int CurrentPoint { get { return ViettelPlusInfo == null ? 0 : ViettelPlusInfo.point_current; } }
        public int PointCanUse { get { return ViettelPlusInfo == null ? 0 : ViettelPlusInfo.point_can_used; } }
        public string PointExpire { get { return ViettelPlusInfo == null ? string.Empty : ViettelPlusInfo.point_expired; } }
        public string RankName { get { return ViettelPlusInfo == null ? string.Empty : ViettelPlusInfo.rank_name; } }

        #endregion

        private MVTPersonInfo PersonInfo { get; set; }
        private List<MVTBalanceInfo> ListBalanceInfo { get; set; }
        private List<MVTDataPackageInfo> ListDataPackageInfo { get; set; }
        public MVTViettelPlusInfo ViettelPlusInfo { get; set; }
        public MVTAccount Login(string username = "", string password = "", bool GetFullInfo = true)
        {
            MVTAccount result = null;
            if (string.IsNullOrEmpty(username))
                username = Username;
            if (string.IsNullOrEmpty(password))
                password = Password;

            int failRemain = 15;
            string lastFailedMessage = string.Empty;
        loop:
            if (failRemain == 0)
            {
                GlobalEvent.OnGlobalMessaging($"{Username} -> Cập nhật thất bại");
                return result;
            }
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    handler.CookieContainer = new System.Net.CookieContainer();
                    handler.UseCookies = true;
                    handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.None;
                    var proxyInfo = new MXHPortal().RequestProxy(ProxyFor.MVT);
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
                        client.Timeout = TimeSpan.FromSeconds(30);
                        client.DefaultRequestHeaders.ExpectContinue = false;
                        var req = client.GetAsync("https://vietteltelecom.vn/dang-nhap").Result;
                        string deviceName = "webportal";
                        string deviceID = "webportal";
                        var request = client.PostAsync("https://vietteltelecom.vn/api/login-user-by-phone", new StringContent(
                               JsonConvert.SerializeObject(new
                               {
                                   account = username,
                                   password = password,
                                   account_target = string.Empty,
                                   device_id = "webportal"
                               })
                               , Encoding.UTF8, "application/json")).Result;

                        string response = request.Content.ReadAsStringAsync().Result;
                        response = response.Replace(@"\u0000*\u0000", string.Empty);

                        if (request.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Token = JObject.Parse(response).SelectToken("info.token").Value<string>();
                            DeviceID = deviceID;
                            DeviceName = deviceName;
                            Username = username;
                            Password = password;
                            PersonInfo = new MVTPersonInfo()
                            {
                                fullName = JObject.Parse(response).SelectToken("info.fullName").Value<string>(),
                                birthday = JObject.Parse(response).SelectToken("info.birthday").Value<string>(),
                                cmnd_number = JObject.Parse(response).SelectToken("info.cmnd_number").Value<string>(),
                                cmnd_date = JObject.Parse(response).SelectToken("info.cmnd_date").Value<string>(),
                                cmnd_place = JObject.Parse(response).SelectToken("info.cmnd_place").Value<string>()
                            };
                            try
                            {
                                var balancesInfo = JObject.Parse(response).SelectToken("info.extraInfo").ToObject<List<MVTBalanceInfo>>();

                                if (balancesInfo != null)
                                {
                                    if (ListBalanceInfo == null)
                                        ListBalanceInfo = new List<MVTBalanceInfo>();

                                    foreach (var balanceInfo in balancesInfo)
                                        ListBalanceInfo.Add(balanceInfo);
                                }
                            }
                            catch { }

                            ViettelPlusInfo = JObject.Parse(response).SelectToken("info.viettelPlusInfo").ToObject<MVTViettelPlusInfo>();

                            if (GetFullInfo)
                            {
                                //GetMainBalanceInfo();
                                //GetDataPackageInfo();
                                //GetVTPlusInfo();
                            }
                            result = this;
                        }
                        else
                        {
                            string message = JObject.Parse(response).SelectToken("errors.message[0]").Value<string>();
                            if (message != "Thông tin tài khoản không chính xác.")
                            {
                                Token = string.Empty;
                                failRemain--;
                                goto loop;
                            }
                            else
                            {
                                GlobalEvent.OnGlobalMessaging($"{username} -> {message}");
                            }
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
            return result;
        }
        private void GetMainBalanceInfo()
        {
            if (string.IsNullOrEmpty(Token))
                return;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Dictionary<string, string> body = new Dictionary<string, string>();
                    body.Add("token", Token);
                    var request = client.PostAsync($"https://apivtp.vietteltelecom.vn:6768/myviettel.php/viewAccountInfo3?device_name={WebUtility.UrlEncode(DeviceName)}&version_app=4.6.1&build_code=286&os_type=android&os_version=22&device_id={WebUtility.UrlEncode(DeviceID)}",
                        new FormUrlEncodedContent(body)).Result;
                    string response = request.Content.ReadAsStringAsync().Result;
                    string errorCode = JObject.Parse(response).SelectToken("errorCode").Value<string>();
                    string message = JObject.Parse(response).SelectToken("message").Value<string>();
                    if (errorCode == "0")
                    {
                        var balancesInfo = JObject.Parse(response).SelectToken("data").ToObject<List<MVTBalanceInfo>>();
                        if (balancesInfo != null)
                        {
                            if (ListBalanceInfo == null)
                                ListBalanceInfo = new List<MVTBalanceInfo>();

                            foreach (var balanceInfo in balancesInfo)
                                ListBalanceInfo.Add(balanceInfo);
                        }
                    }
                    else
                    {
                        GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name }" +
                            $"-> { Username } -> { message}");
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {ex.Message}");
            }
        }
        private void GetDataPackageInfo()
        {
            if (string.IsNullOrEmpty(Token))
                return;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Dictionary<string, string> body = new Dictionary<string, string>();
                    body.Add("token", Token);
                    var request = client.PostAsync($"https://apivtp.vietteltelecom.vn:6768/myviettel.php/getDataRemain?device_name={WebUtility.UrlEncode(DeviceName)}&version_app=4.6.1&build_code=286&os_type=android&os_version=22&device_id={WebUtility.UrlEncode(DeviceID)}",
                        new FormUrlEncodedContent(body)).Result;
                    string response = request.Content.ReadAsStringAsync().Result;
                    string errorCode = JObject.Parse(response).SelectToken("errorCode").Value<string>();
                    string message = JObject.Parse(response).SelectToken("message").Value<string>();
                    if (errorCode == "0")
                    {
                        var items = JObject.Parse(response).SelectToken("data").ToObject<List<MVTDataPackageInfo>>();
                        if (items != null)
                        {
                            if (ListDataPackageInfo == null)
                                ListDataPackageInfo = new List<MVTDataPackageInfo>();

                            foreach (var item in items)
                                ListDataPackageInfo.Add(item);
                        }
                    }
                    else
                    {
                        GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name }" +
                            $"-> { Username } -> { message}");
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {ex.Message}");
            }
        }
        private void GetVTPlusInfo()
        {
            if (string.IsNullOrEmpty(Token))
                return;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Dictionary<string, string> body = new Dictionary<string, string>();
                    body.Add("token", Token);
                    var request = client.PostAsync($"https://apivtp.vietteltelecom.vn:6768/myviettel.php/vtidGetAccountInfo?device_name={WebUtility.UrlEncode(DeviceName)}&version_app=4.6.1&build_code=286&os_type=android&os_version=22&device_id={WebUtility.UrlEncode(DeviceID)}",
                        new FormUrlEncodedContent(body)).Result;
                    string response = request.Content.ReadAsStringAsync().Result;
                    string errorCode = JObject.Parse(response).SelectToken("errorCode").Value<string>();
                    string message = JObject.Parse(response).SelectToken("message").Value<string>();
                    if (errorCode == "0")
                    {
                        ViettelPlusInfo = JObject.Parse(response).SelectToken("data").ToObject<MVTViettelPlusInfo>();
                    }
                    else
                    {
                        GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name }" +
                            $"-> { Username } -> { message}");
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {ex.Message}");
            }
        }
        public MVTAccount Register(string username = "")
        {
            MVTAccount result = null;
            int failRemain = 15;
            string lastFailedMessage = string.Empty;

            try
            {
                var queue = MVTGlobalVar.RegisterVar.GetQueue();
            loop:
                if (GlobalVar.IsApplicationExit)
                    return null;
                if (failRemain == 0)
                {
                    GlobalEvent.OnGlobalMessaging($"{Username} -> đăng ký thất bại");
                    return result;
                }
                try
                {
                    if (queue == null || queue.QueueState != MVTRegisterQueueState.Processing)
                        return null;
                    username = queue.PhoneNumber;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing);

                    using (HttpClientHandler handler = new HttpClientHandler())
                    {
                        handler.CookieContainer = new System.Net.CookieContainer();
                        handler.UseCookies = true;
                        handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.None;
                        var proxyInfo = new MXHPortal().RequestProxy(ProxyFor.MVT);
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
                            var req = client.GetAsync("https://vietteltelecom.vn/dang-nhap").Result;
                            if (queue == null || queue.QueueState != MVTRegisterQueueState.Processing)
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
                            var request = client.PostAsync("https://vietteltelecom.vn/api/get-otp", new StringContent(JsonConvert.SerializeObject(new { msisdn = username }), Encoding.UTF8, "application/json")).Result;
                            string response = request.Content.ReadAsStringAsync().Result;
                            if (queue == null || queue.QueueState != MVTRegisterQueueState.Processing)
                                return null;
                            string errorCode = JObject.Parse(response).SelectToken("errorCode").Value<string>();
                            string message = JObject.Parse(response).SelectToken("message").Value<string>();
                            if (errorCode == "0")
                            {
                                DateTime otpRequestTime = DateTime.Now;
                                while ((DateTime.Now - otpRequestTime).TotalSeconds <
                                    MVTGlobalVar.RegisterVar.OTPTimeout && !GlobalVar.IsApplicationExit && !MVTGlobalVar.RegisterVar.Stop)
                                {
                                    if (GlobalVar.IsApplicationExit)
                                        break;
                                    if (queue == null ||
                                        queue.QueueState != MVTRegisterQueueState.Processing)
                                        break;
                                    if (!string.IsNullOrEmpty(otp))
                                        break;
                                    Thread.Sleep(1000);
                                }
                                if (queue == null || queue.QueueState != MVTRegisterQueueState.Processing)
                                    return null;

                                if (string.IsNullOrEmpty(otp))
                                {
                                    queue.Resolved = true;
                                    queue.QueueState = MVTRegisterQueueState.Failed;
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.NoOTP);
                                }


                                request = client.PostAsync("https://vietteltelecom.vn/api/register-user-by-phone", new StringContent(JsonConvert.SerializeObject(new
                                {
                                    isdn = username,
                                    password = MVTGlobalVar.RegisterVar.Password,
                                    password_confirmation = MVTGlobalVar.RegisterVar.Password,
                                    otp = otp,
                                    device_id = "webportal",
                                    listAcc = string.Empty,
                                    regType = (object)null,
                                    captcha = (object)null,
                                    isWeb = 1,
                                    captcha_code = string.Empty,
                                    sid = (object)null
                                }
                                ), Encoding.UTF8, "application/json")).Result;
                                if (queue == null || queue.QueueState != MVTRegisterQueueState.Processing)
                                    return null;
                                response = request.Content.ReadAsStringAsync().Result;
                                errorCode = JObject.Parse(response).SelectToken("errorCode").Value<string>();
                                message = JObject.Parse(response).SelectToken("message").Value<string>();
                                if (errorCode == "0")
                                {
                                    Username = username;
                                    Password = MVTGlobalVar.RegisterVar.Password;
                                    Token = JObject.Parse(response).SelectToken("data.data.token").Value<string>();
                                    DeviceID = "webportal";
                                    DeviceName = "webportal";
                                    PersonInfo = new MVTPersonInfo()
                                    {
                                        fullName = JObject.Parse(response).SelectToken("data.data.fullName").Value<string>(),
                                        birthday = JObject.Parse(response).SelectToken("data.data.birthday").Value<string>(),
                                        cmnd_number = JObject.Parse(response).SelectToken("data.data.cmnd_number").Value<string>(),
                                        cmnd_date = JObject.Parse(response).SelectToken("data.data.cmnd_date").Value<string>(),
                                        cmnd_place = JObject.Parse(response).SelectToken("data.data.cmnd_place").Value<string>()
                                    };
                                    result = Login(username, MVTGlobalVar.RegisterVar.Password);
                                    MVTGlobalVar.Accounts.Add(result);
                                    queue.QueueState = MVTRegisterQueueState.Succeed;
                                    queue.Resolved = true;
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Succeed);
                                }
                                else
                                {
                                    queue.Resolved = true;
                                    queue.QueueState = MVTRegisterQueueState.Failed;
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed);

                                }
                            }
                            else
                            {
                                queue.Resolved = true;
                                queue.QueueState = MVTRegisterQueueState.Failed;
                                GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed);
                            }
                        }
                    }
                }
                catch (Exception ex)
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
                MVTGlobalVar.RegisterVar.OnEachCompleted();
            }
            return result;
        }
        public MVTVoucherInfo ExchangeVoucher(MVTPromotionInfo voucherInfo)
        {
            MVTVoucherInfo result = null;
            try
            {
                if (string.IsNullOrEmpty(Token))
                    Login("", "", false);
                if (string.IsNullOrEmpty(Token))
                    return result;

                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    handler.CookieContainer = new System.Net.CookieContainer();
                    handler.UseCookies = true;
                    handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.None;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        Dictionary<string, string> body = new Dictionary<string, string>();
                        body.Add("token", Token);
                        body.Add("voucher", voucherInfo.id.ToString());

                        var request = client.PostAsync("https://apivtp.vietteltelecom.vn:6768/myviettel.php/apiMediaOne/get-voucher-info?device_name=webportal&device_id={DeviceID}",
                            new FormUrlEncodedContent(body)).Result;
                        string response = request.Content.ReadAsStringAsync().Result;
                        voucherInfo = JObject.Parse(response).SelectToken("data.voucherInfo").ToObject<MVTPromotionInfo>();
                        if (voucherInfo != null)
                        {
                            voucherInfo.programTerm = JObject.Parse(response).SelectToken("programTerm").Value<string>();

                            //var req = client.GetAsync("https://cong.viettel.vn/chi-tiet-uu-dai/shop-gau-soc-kid-giam-5-tren-tong-hoa-don/15862").Result;
                            request = client.PostAsync($"https://apivtp.vietteltelecom.vn:6768/myviettel.php/apiMediaOne/get-code?device_name={DeviceName}&device_id={DeviceID}",
                                new FormUrlEncodedContent(body)).Result;
                            response = request.Content.ReadAsStringAsync().Result;

                            string message = JObject.Parse(response).SelectToken("message").Value<string>();
                            int code = JObject.Parse(response).SelectToken("code").Value<int>();
                            if (code == 0)
                            {
                                result = JObject.Parse(response).SelectToken("data").ToObject<MVTVoucherInfo>();
                                result.phone = Username;
                                result.name = voucherInfo.name;
                                MVTGlobalVar.VoucherExchangeVar.VoucherExchanged(result);
                                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {message}");
                            }
                            else
                            {
                                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {ex.Message}");
            }
            return result;
        }
        public void ChangePassword(string newPassword)
        {
            try
            {
            loop:
                if (string.IsNullOrEmpty(Token))
                    Login("", "", false);
                if (string.IsNullOrEmpty(Token))
                    return;

                using (HttpClient client = new HttpClient())
                {
                    Dictionary<string, string> body = new Dictionary<string, string>();
                    body.Add("token", Token);
                    var request = client.PostAsync($"https://apivtp.vietteltelecom.vn:6768/myviettel.php/getCaptcha?device_name={DeviceName}&version_app=4.6.1&build_code=286&os_type=android&os_version=22&device_id={DeviceID}",
                        new FormUrlEncodedContent(body)).Result;
                    string response = request.Content.ReadAsStringAsync().Result;
                    int code = JObject.Parse(response).SelectToken("errorCode").Value<int>();
                    string message = JObject.Parse(response).SelectToken("message").Value<string>();
                    if (code == 0)
                    {
                        string captchaImageUrl = JObject.Parse(response).SelectToken("data.url").Value<string>();
                        string sid = JObject.Parse(response).SelectToken("data.sid").Value<string>();
                        var bytes = client.GetByteArrayAsync(captchaImageUrl).Result;
                        string base64 = Convert.ToBase64String(bytes);
                        string captcha = CaptchaHelper.Resolve(base64);
                        body.Add("sid", sid);
                        body.Add("oldPassword", Password);
                        body.Add("newPassword", newPassword);
                        body.Add("captcha", captcha);
                        request = client.PostAsync($"https://apivtp.vietteltelecom.vn:6768/myviettel.php/changePassword",
                         new FormUrlEncodedContent(body)).Result;
                        response = request.Content.ReadAsStringAsync().Result;
                        code = JObject.Parse(response).SelectToken("errorCode").Value<int>();
                        message = JObject.Parse(response).SelectToken("message").Value<string>();
                        if (code == 1)
                        {
                            goto loop;
                            //sai captcha
                        }
                        if (code == -2)
                        {
                            //account login ở chỗ khác
                        }
                        if (code == 6)
                        {
                            //mât khẩu mới giống mật khẩu cũ
                        }
                        if (code == 0)
                        {
                            Password = newPassword;
                            //thành công
                        }
                    }
                    else
                    {
                        GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {Username} -> {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {ex.Message}");
            }
        }
        public void GetPaymentHistoies()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Dictionary<string, string> body = new Dictionary<string, string>();
                    body.Add("token", Token);
                    var request = client.PostAsync($"https://apivtp.vietteltelecom.vn:6768/myviettel.php/getPayHistory?device_name={DeviceName}&version_app=4.6.1&build_code=286&os_type=android&os_version=22&device_id={DeviceID}",
                        new FormUrlEncodedContent(body)).Result;
                    string response = request.Content.ReadAsStringAsync().Result;
                    int code = JObject.Parse(response).SelectToken("errorCode").Value<int>();
                    if (code == 0)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                GlobalEvent.OnGlobalMessaging($"{MethodBase.GetCurrentMethod().Name} -> {ex.Message}");
            }
        }
    }

    public class MVTVoucherInfo
    {
        public string phone { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string expiredAt { get; set; }
    }

    public class MVTPersonInfo
    {
        public string fullName { get; set; }
        public string birthday { get; set; }
        public string cmnd_number { get; set; }
        public string cmnd_date { get; set; }
        public string cmnd_place { get; set; }
    }

    public class MVTBalanceInfo
    {
        public string type { get; set; }
        public string name { get; set; }
        public int value { get; set; }
        public string unit { get; set; }
        public string expire { get; set; }
    }
    public class MVTDataPackageInfo
    {
        public string pack_name { get; set; }
        public string expireDate { get; set; }
        public double expireDate_timestamp { get; set; }
        public string remain { get; set; }
        public string remain_mb { get; set; }
        public string description { get; set; }
    }

    public class MVTViettelPlusInfo
    {
        public string rank_name { get; set; }
        public int rank_type { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public int point_can_used { get; set; }
        public string point_expired { get; set; }
        public int point_next_rank { get; set; }
        public int point_current { get; set; }
        public string name_next_rank { get; set; }
        public string description_next_rank { get; set; }
    }

}
