using System;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Media;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
//using MXH.Account;
using MXH.MMF;
using MXH.MVT;

namespace MXH
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Test();
            //return;
            ServicePointManager.ServerCertificateValidationCallback
                += (sender, certificate, chain, sslPolicyErrors) => { return true; };
            //if (!NetworkHelper.IsConnectedToInternet())
            //{
            //    MessageBox.Show("Kết nối internet không ổn định, vui lòng kiểm tra lại", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            //if (!new MXHPortal().IsPortalReady())
            //{
            //    MessageBox.Show("Kết nối tới máy chủ thất bại, vui lòng kiểm tra lại thiết lập ngày giờ hệ thống hoặc liên hệ https://www.facebook.com/soul.keeper79 để được hỗ trợ", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            //MakeSureAutoupdateNewest();
            //VersionManager.CheckForUpdate(true);
            GlobalVar.InitProxy();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //UserLookAndFeel.Default.SkinName = "Visual Studio 2013 Dark";
            Application.Run(new MainUI());
            //Application.Run(new MainUINew());


        }

        static void MakeSureAutoupdateNewest()
        {
            if (!File.Exists(Application.StartupPath + "\\MXHDeskAutoUpdater.exe"))
            {
                using (var webClient = new WebClient())
                {
                    try
                    {
                        webClient.DownloadFile(new Uri(""),
                            Application.StartupPath + "\\MXHDeskAutoUpdater.exe");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

            }
        }


        public static void Test()
        {
            var ports = SerialPort.GetPortNames();
            SerialPort Port = null;
            if (Port == null)
            {
                Port = new SerialPort()
                {
                    PortName = "COM6",
                    BaudRate = 115200,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    DataBits = 8,
                    Handshake = Handshake.RequestToSend,
                    DtrEnable = true,
                    RtsEnable = true,
                    NewLine = "\r",
                    Encoding = Encoding.UTF8,
                    WriteTimeout = 60000,
                    ReadTimeout = 60000
                };
            }
            if (!Port.IsOpen)
            {
                Port = new SerialPort()
                {
                    PortName = "COM6",
                    BaudRate = 115200,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    DataBits = 8,
                    Handshake = Handshake.RequestToSend,
                    DtrEnable = true,
                    RtsEnable = true,
                    NewLine = "\r",
                    Encoding = Encoding.UTF8,
                    WriteTimeout = 60000,
                    ReadTimeout = 60000
                };
                Port.Open();
            }
            //Port.WriteLine("AT+CPIN?");
            //System.Threading.Thread.Sleep(1000);
            //string response = Port.ReadExisting();

            Port.WriteLine("ATI\r");
            System.Threading.Thread.Sleep(1000);
            string response = Port.ReadExisting();


            Port.WriteLine("AT+CMGF=1");
            Port.WriteLine("AT+CLIP=1");
            response = WaitResultOrTimeout(Port, "CMGF", 500);
            Port.WriteLine("AT+CPMS=\"SM\"");
            response = WaitResultOrTimeout(Port, "CPMS", 500);

            Port.WriteLine("AT+COPS?");
            Thread.Sleep(1000);
            response = Port.ReadExisting();

            Port.Write("AT+CSCS=\"GSM\"\r");
            response = Port.ReadExisting();



            Port.Write("AT+CREG?\r");
            System.Threading.Thread.Sleep(1000);
            response = Port.ReadExisting();

            Port.Write("AT+CSCS=?\r");
            System.Threading.Thread.Sleep(1000);
            response = Port.ReadExisting();



            Port.Write("ATDT*111#;\r");
            //Port.Write("AT+CUSD=1\r");

            response = WaitResultOrTimeout(Port, "CUSD", 20000);

            Port.Write("AT+CUSD=2\r");
            response = WaitResultOrTimeout(Port, "CUSD", 3000);



        }

        private static string WaitResultOrTimeout(SerialPort Port, string containSucceed, int timeout)
        {
            
            try
            {
                DateTime startTime = DateTime.Now;
                string result = string.Empty;
            loop:
                if ((DateTime.Now - startTime).TotalMilliseconds > timeout)
                {
                    return result;
                }

                string response = Port.ReadExisting();
                result += response;
                //LogResponseCommand(response);
                if (result.Contains(containSucceed) && result.Contains("OK"))
                {
                    if (string.IsNullOrEmpty(response))
                        return result;
                    else
                    {
                        Thread.Sleep(500);
                        goto loop;
                    }
                }
                else
                {
                    goto loop;
                }

            }
            catch { }
            return string.Empty;
        }
    }

    public class SerialPortInfo
    {
        public SerialPortInfo(ManagementObject property)
        {
            this.Availability = property.GetPropertyValue("Availability") as int? ?? 0;
            this.Caption = property.GetPropertyValue("Caption") as string ?? string.Empty;
            this.ClassGuid = property.GetPropertyValue("ClassGuid") as string ?? string.Empty;
            this.CompatibleID = property.GetPropertyValue("CompatibleID") as string[] ?? new string[] { };
            this.ConfigManagerErrorCode = property.GetPropertyValue("ConfigManagerErrorCode") as int? ?? 0;
            this.ConfigManagerUserConfig = property.GetPropertyValue("ConfigManagerUserConfig") as bool? ?? false;
            this.CreationClassName = property.GetPropertyValue("CreationClassName") as string ?? string.Empty;
            this.Description = property.GetPropertyValue("Description") as string ?? string.Empty;
            this.DeviceID = property.GetPropertyValue("DeviceID") as string ?? string.Empty;
            this.ErrorCleared = property.GetPropertyValue("ErrorCleared") as bool? ?? false;
            this.ErrorDescription = property.GetPropertyValue("ErrorDescription") as string ?? string.Empty;
            this.HardwareID = property.GetPropertyValue("HardwareID") as string[] ?? new string[] { };
            this.InstallDate = property.GetPropertyValue("InstallDate") as DateTime? ?? DateTime.MinValue;
            this.LastErrorCode = property.GetPropertyValue("LastErrorCode") as int? ?? 0;
            this.Manufacturer = property.GetPropertyValue("Manufacturer") as string ?? string.Empty;
            this.Name = property.GetPropertyValue("Name") as string ?? string.Empty;
            this.PNPClass = property.GetPropertyValue("PNPClass") as string ?? string.Empty;
            this.PNPDeviceID = property.GetPropertyValue("PNPDeviceID") as string ?? string.Empty;
            this.PowerManagementCapabilities = property.GetPropertyValue("PowerManagementCapabilities") as int[] ?? new int[] { };
            this.PowerManagementSupported = property.GetPropertyValue("PowerManagementSupported") as bool? ?? false;
            this.Present = property.GetPropertyValue("Present") as bool? ?? false;
            this.Service = property.GetPropertyValue("Service") as string ?? string.Empty;
            this.Status = property.GetPropertyValue("Status") as string ?? string.Empty;
            this.StatusInfo = property.GetPropertyValue("StatusInfo") as int? ?? 0;
            this.SystemCreationClassName = property.GetPropertyValue("SystemCreationClassName") as string ?? string.Empty;
            this.SystemName = property.GetPropertyValue("SystemName") as string ?? string.Empty;
        }

        int Availability;
        string Caption;
        string ClassGuid;
        string[] CompatibleID;
        int ConfigManagerErrorCode;
        bool ConfigManagerUserConfig;
        string CreationClassName;
        string Description;
        string DeviceID;
        bool ErrorCleared;
        string ErrorDescription;
        string[] HardwareID;
        DateTime InstallDate;
        int LastErrorCode;
        string Manufacturer;
        string Name;
        string PNPClass;
        string PNPDeviceID;
        int[] PowerManagementCapabilities;
        bool PowerManagementSupported;
        bool Present;
        string Service;
        string Status;
        int StatusInfo;
        string SystemCreationClassName;
        string SystemName;

    }
}
