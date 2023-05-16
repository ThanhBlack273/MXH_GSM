using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MXH
{
    public static class CaptchaHelper
    {
        public static string Resolve(string base64)
        {
            if (!string.IsNullOrEmpty(base64))
            {
                using (HttpClient client = new HttpClient())
                {
                    Dictionary<string, string> postData = new Dictionary<string, string>();
                    postData.Add("key", "count_6cbfd15de4951d6a6dac521a56a43de6");
                    postData.Add("server", "1");
                    postData.Add("png_fixed", "0");
                    postData.Add("imagebase64", base64);
                    var formData = new FormUrlEncodedContent(postData);
                    var request = client.PostAsync("http://api.bycaptcha.com", formData).Result;
                    return JObject.Parse(request.Content.ReadAsStringAsync().Result).SelectToken("textdecode").Value<string>();
                }
            }
            return string.Empty;
        }
    }
}
