using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MXH
{
    public class MXHPortal
    {
        private static readonly string Server = "";
       
        public string VoiceRecognitionToText(byte[] audio)
        {
            string result = string.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jsonResponse = PostResponse("VoiceRecognitionToText", new
                    {
                        Sensor = new
                        {
                            Rds = Guid.NewGuid().ToString(),
                            ClientTime = DateTime.Now
                        },
                        Data = audio
                    });
                    result = JsonConvert.DeserializeObject<string>(jsonResponse);
                }
            }
            catch { }
            return result;
        }
        public VersionInfo GetLatestVersionInfo()
        {
            VersionInfo result = null;
            try
            {
                string jsonResponse = GetResponse("LatestVersionInfo", new { Sensor = new { Rds = Guid.NewGuid().ToString(), ClientTime = DateTime.Now } });
                result = JsonConvert.DeserializeObject<VersionInfo>(jsonResponse);
            }
            catch { }
            return result;
        }
        public ProxyInfo RequestProxy(ProxyFor proxyFor)
        {
            ProxyInfo result = null;
            try
            {
                switch (proxyFor)
                {
                    case ProxyFor.MMF:
                        {
                            result = GlobalVar.ProxiesInfo.OrderBy(proxy => proxy.MMFLastUsed).FirstOrDefault();
                            result.MMFLastUsed = DateTime.Now;
                            break;
                        }
                    case ProxyFor.MVT:
                        {
                            result = GlobalVar.ProxiesInfo.OrderBy(proxy => proxy.MVTLastUsed).FirstOrDefault();
                            result.MVTLastUsed = DateTime.Now;
                            break;
                        }
                    case ProxyFor.VNPT:
                        {
                            result = GlobalVar.ProxiesInfo.OrderBy(proxy => proxy.VNPTLastUsed).FirstOrDefault();
                            result.VNPTLastUsed = DateTime.Now;
                            break;
                        }
                }


                if (result == null)
                    result = new ProxyInfo();
            }
            catch { }
            return result;
        }
        public DateTime Now()
        {
            string jsonResponse = GetResponse("Now", new { Sensor = new { Rds = Guid.NewGuid().ToString(), ClientTime = DateTime.Now } });
            return JsonConvert.DeserializeObject<DateTime>(jsonResponse);
        }
        public bool IsPortalReady()
        {
            string jsonResponse = GetResponse("Ping", new { Sensor = new { Rds = Guid.NewGuid().ToString(), ClientTime = DateTime.Now } });
            return JsonConvert.DeserializeObject<string>(jsonResponse) == "Pong";
        }
        public string GetResponse(string endpoint, object data)
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
            {
            }
            return result;
        }
        public string PostResponse(string endpoint, object data)
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

    public class ProxyInfo
    {
        public string IP { get; set; }
        public DateTime MVTLastUsed { get; set; }
        public DateTime MMFLastUsed { get; set; }
        public DateTime VNPTLastUsed { get; set; }
    }
    public enum ProxyFor
    {
        MVT, MMF, VNPT
    }


}
