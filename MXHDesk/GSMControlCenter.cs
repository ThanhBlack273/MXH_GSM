using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MXH
{
    public static class GSMControlCenter
    {
        public static Action<GSMMessage> OnNewMessage = (data) => { };
        public static Thread PortHandler { get; set; }
        private static bool Stop = false;
        public static void Dispose()
        {
            Stop = true;
            foreach (var gsmCom in GSMComs)
            {
                try
                {
                    gsmCom.Dispose();
                }
                catch { }
            }
        }
        
        public static BindingList<GSMCom> GSMComs = new BindingList<GSMCom>();
        public static BindingList<GSMMessage> GSMMessages = new BindingList<GSMMessage>();
        private static object lockGSMComs = new object();
        public static void MyRegisterNotifySuccess(string phone, MyRegisterState myRegisterState)
        {
            lock (lockGSMComs)
            {
                var com = GSMComs.FirstOrDefault(_com => _com.PhoneNumber == phone);
                if (com != null && com.IsPortConnected && com.IsSIMConnected)
                    com.MyRegisterState = myRegisterState;
            }
        }
        public static object LockGSMMessages = new object();
        public static void Start()
        {
            PortHandler = new Thread(new ThreadStart(PortHanding));
            PortHandler.Start();
        }
        private static int CurrentPortDisplayIndex = 1;
        public static void PortHanding()
        {
            while (!Stop)
            {
                try
                {
                    string[] ports = SerialPort.GetPortNames();
                    if (ports != null && ports.Any())
                    {
                        foreach (string port in ports)
                        {
                            if (GSMComs.Any(com => com.PortName == port))
                                continue;
                            var gsmCom = new GSMCom() { DisplayName = $"Cổng {CurrentPortDisplayIndex}" };
                            GSMComs.Add(gsmCom);
                            gsmCom.Start(port);
                            CurrentPortDisplayIndex++;
                            Thread.Sleep(200);
                        }
                    }
                    else
                    {
                        foreach (var gsmCom in GSMComs)
                        {
                            gsmCom.Stop = true;
                        }
                        GSMComs.Clear();
                    }
                }
                catch { }
                Thread.Sleep(5000);
            }
        }
    }
}
