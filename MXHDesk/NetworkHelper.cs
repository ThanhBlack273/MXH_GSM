using DevExpress.DocumentServices.ServiceModel.DataContracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MXH
{
    public static class NetworkHelper
    {

      
        private static object lockChangeIP = new object();
        public static string RasEntryName { get; set; }
        public static void ResetDcomConnection()
        {
            lock (lockChangeIP)
            {
            loop:
                if (GlobalVar.IsApplicationExit)
                    return;
                DisconnectWifi();
                DisconnectDcom();
                ConnectDcom();
                if (!IsConnectedToInternet())
                {
                    GlobalEvent.OnGlobalMessaging($"Could not rasdial to {NetworkHelper.RasEntryName}");
                    goto loop;
                }
            }
        }
        public static void DisconnectWifi()
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "CMD.exe";
                proc.StartInfo.Arguments = "/c netsh wlan disconnect";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }
            catch { }
        }

        public static void DisconnectDcom()
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "CMD.exe";
                proc.StartInfo.Arguments = "/c rasdial /disconnect";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }
            catch { }
        }
        public static void ConnectDcom()
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "CMD.exe";
                proc.StartInfo.Arguments = "/c %Windir%\\system32\\rasdial \"" + RasEntryName + "\"";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
                Thread.Sleep(1000);
            }
            catch { }
        }
        public static void ConnectWifi()
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "CMD.exe";
                proc.StartInfo.Arguments = "/c %Windir%\\system32\\rasdial \"Mobifone\"";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }
            catch { }
        }

        public static bool IsConnectedToInternet()
        {
            string host = "google.com";
            bool result = false;
            Ping p = new Ping();
            try
            {
                PingReply reply = p.Send(host, 3000);
                if (reply.Status == IPStatus.Success)
                    return true;
            }
            catch 
            { }
            return result;
        }
    }
}
