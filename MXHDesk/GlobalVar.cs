using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MXH
{
    public static class GlobalVar
    {
        
        public static bool AutoAnswerIncomingCall = true;
        public static bool EnableVoiceRecognitionToText = false;
        public static bool EnableIncomingCallRing = true;
        public static bool EnableSMSRing = true;

        public static bool IsApplicationExit { get; internal set; }
        public static bool RealtimeSMSTracking = true;


        [DllImport("kernel32.dll")]
        public static extern bool Beep(int freq, int fur);
        public static bool EnableNotifyWhenNoPhoneForRegister = false;
        private static List<ObjectSequence> ObjectSequences = new List<ObjectSequence>();
        private static object lockoSequence = new object();
        
        //CMT KHÔNG XÀI
        public static void AddSequence(ObjectSequence oSequence)
        {
            lock (lockoSequence)
            {
                oSequence.SeqTime = DateTime.Now;
                ObjectSequences.Add(oSequence);
            }
        }
        //CMT KHÔNG XÀI
        public static void AddSequences(List<ObjectSequence> oSequence)
        {
            lock (lockoSequence)
            {
                oSequence.ForEach(seq => { seq.SeqTime = DateTime.Now; });
                ObjectSequences.AddRange(oSequence);
            }
        }
        
        public static T GetSequence<T>(ObjectSequenceType type)
        {
            lock (lockoSequence)
            {
                var seq = ObjectSequences.Where(_seq => _seq.ObjectSequenceType == type && _seq.ObjectSequenceState == ObjectSequenceState.New)
                    .OrderBy(_seq => _seq.SeqTime).FirstOrDefault();
                if (seq != null && seq.@Object != null)
                {
                    seq.ObjectSequenceState = ObjectSequenceState.Processed;
                    return (T)seq.@Object;
                }
                else return default(T);
            }
        }
        public static void ForceKillMyself()
        {
            Process.GetCurrentProcess().Kill();
        }

        public static List<ProxyInfo> ProxiesInfo = new List<ProxyInfo>();
        static List<string> LuminatiIPs = new List<string>();
        public static void InitProxy()
        {
            ProxiesInfo = new List<ProxyInfo>();
            LuminatiIPs = File.ReadAllLines(Application.StartupPath +  "\\IPs.txt").ToList();
            foreach (string ip in LuminatiIPs.OrderBy(proxy => Guid.NewGuid()))
            {
                ProxiesInfo.Add(new ProxyInfo()
                {
                    IP = ip,
                    MVTLastUsed = DateTime.Now,
                    MMFLastUsed = DateTime.Now,
                    VNPTLastUsed = DateTime.Now
                });
            }
        }
    }
   
}
