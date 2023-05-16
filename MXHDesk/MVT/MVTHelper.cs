using DevExpress.Utils.CodedUISupport;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MXH.MVT
{
    public static class MVTHelper
    {
        public static string RandomDeviceName()
        {
            return "LGM-V300K";
        }
        public static string RandomDeviceID()
        {
            return "352003021602220";
        }
        public static MVTPromotionInfo GetPromotionInfo(string voucherid)
        {
            MVTPromotionInfo result = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "okhttp/3.12.0");
                    Dictionary<string, string> body = new Dictionary<string, string>();
                    body.Add("token", string.Empty);
                    body.Add("voucher", voucherid);

                    var request = client.PostAsync("https://apivtp.vietteltelecom.vn:6768/myviettel.php/apiMediaOne/get-voucher-info?device_name=webportal&version_app=4.6.1&build_code=286&os_type=android&os_version=22",
                        new FormUrlEncodedContent(body)).Result;
                    string response = request.Content.ReadAsStringAsync().Result;
                    string errorCode = JObject.Parse(response).SelectToken("errorCode").Value<string>();
                    string message = JObject.Parse(response).SelectToken("message").Value<string>();
                    if (errorCode == "0")
                    {
                        result = JObject.Parse(response).SelectToken("data.voucherInfo").ToObject<MVTPromotionInfo>();
                        if (result != null)
                        {
                            result.programTerm = JObject.Parse(response).SelectToken("programTerm").Value<string>();
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
    }
}
