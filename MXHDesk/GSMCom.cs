using DevExpress.Utils.MVVM.Services;
using DevExpress.XtraEditors.Native;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MXH.MMF;
using MXH.MVT;

namespace MXH
{
    public class SMSMemory
    {
        public string Name { get; set; }
        public bool Readed = false;
    }

    public enum ModemType
    {
        CINTERION, WAVECOME, QUECTEL, UNSUPPORTED, SIEMENS
    }
    public class GSMCom
    {
        public ModemType ModemType = ModemType.UNSUPPORTED;

        public string DisplayName { get; set; }
        public SMSMemory[] SMSMemory = new SMSMemory[]
        {
            new SMSMemory(){ Name = "SM" },
            new SMSMemory(){ Name = "ME" },
        };
        private const string ENTER = @"
";
        private const string Ctrl_Z = "";


        public bool Stop = false;
        public bool DoNotConnect { get; set; }
        public bool Log { get; set; }
        public string PortName { get; private set; }
        public string PhoneNumber { get; set; }
        public bool IsPortConnected { get; set; }
        public bool IsSIMConnected { get; set; }
        private bool ComStarted = false;
        public int MainBalance { get; set; }
        public string Expire { get; set; }
        public RingStatus RingStatus { get; set; }
        public Action<GSMCom, RingStatus> RingStatusChanged = (com, ringStatus) => { };
        public SIMCarrier SIMCarrier { get; set; }
        private Thread PortConnectionHandler { get; set; }
        private Thread SIMConnectionHandler { get; set; }
        private Thread GSMMessageHandler { get; set; }
        private Thread AlertSMSHandler { get; set; }
        public MyRegisterState MyRegisterState { get; set; }
        public string MyRegisterStateText
        {
            get
            {
                return MyRegisterState == MyRegisterState.None ? (SIMCarrier == SIMCarrier.NO_SIM_CARD ? string.Empty : string.Empty)
                   : MyRegisterState == MyRegisterState.Processing ? "PROCESSING"
                   : MyRegisterState == MyRegisterState.NoOTP ? "OTP TIMEOUT"
                   : MyRegisterState == MyRegisterState.Failed ? "FALIED"
                   : MyRegisterState == MyRegisterState.Succeed ? "SUCCEED"
                   : "UNKNOW";

            }
        }
        public string Serial { get; set; }
        private Thread AlertCallHandler { get; set; }
        public string LastUSSDCommand { get; set; }
        public string LastUSSDResult { get; set; }

        private object RequestLocker = new object();
        private Semaphore Semaphore = new Semaphore(2,4);
        private void LogResponseCommand(string response)
        {
            if (!string.IsNullOrEmpty(response) && Log)
            {
                GlobalEvent.ONATCommandResponse($"====[ {PortName} ]====\n\n{response}=============\n");
            }
            if (SIMCarrier == SIMCarrier.VietnamMobile && !string.IsNullOrEmpty(response) && response.Contains("TK Chinh:"))
            {
                try
                {
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    //string date = Regex.Match("((\\d{4}\\/\\d{2}\\/\\d{2} \\d{2}:\\d{2}:\\d{2}\\+\\d{2}))", response).Value;
                    //GlobalEvent.ONATCommandResponse($"LOGG{date}");
                    //date = date.Substring(0, date.IndexOf("+"));
                    //var accountDate = DateTime.ParseExact(date, "yyyy/MM/dd HH:mm:ss", provider);
                    //if (accountDate > VNMBUpdateDate)
                    //{
                    string accountInfo = Regex.Match(response, "(TK Chinh: (.*?)het han (\\d{2}\\/\\d{2}\\/\\d{4}))").Value;
                    //GlobalEvent.ONATCommandResponse($"LOGG{accountInfo}");

                    var matchBalance = Regex.Match(accountInfo, "(TK Chinh: (.*?)d,)");
                    var matchExpire = Regex.Match(accountInfo, "(\\d{2}\\/\\d{2}\\/\\d{4})");
                    if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                    {
                        MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TK Chinh: ", "").Replace("d,", ""));
                    }
                    if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
                    {
                        Expire = matchExpire.Value;
                    }
                    //VNMBUpdateDate = accountDate;
                    //}
                }
                catch { }
            }
        }
        DateTime LastRing = DateTime.Now;
        private void RingDetector(string response)
        {
            if (response.Contains("RING"))
            {
                RingStatus = RingStatus.Ringing;
                LastRing = DateTime.Now;
                RingStatusChanged(this, RingStatus);
                //string info = ExecuteCommand(GSMCommand.GETCALLERID);
                GlobalEvent.OnGlobalMessaging($"{this.PhoneNumber} -> Ring Ring");
                ExecuteCommand(GSMCommand.ANSWER_CALL, null);
            }
            else
            {
                if (RingStatus == RingStatus.Ringing)
                {
                    if ((DateTime.Now - LastRing).TotalSeconds > 5)
                    {
                        RingStatus = RingStatus.Idle;
                        RingStatusChanged(this, RingStatus);
                        GlobalEvent.OnGlobalMessaging($"{this.PhoneNumber} -> Missed");
                    }
                }
            }
        }
        //CMT quan trọng
        public string ExecuteCommand(GSMCommand command, object data = null)
        {
            lock (RequestLocker)
            {
                string response = string.Empty;
                try
                {
                    if (Stop)
                        return string.Empty;

                    if (command != GSMCommand.CHECKRING)
                        WaitEmptyResponse();
                    if (IsPortConnected)
                    {
                        switch (command)
                        {
                            case GSMCommand.CPIN:
                                {
                                    Port.WriteLine("AT+CPIN?");
                                    Thread.Sleep(100);
                                    response = Port.ReadExisting();
                                    return response;
                                }
                            case GSMCommand.SWITCH_TEXT_MODE:
                                {
                                    Port.WriteLine("AT+CMGF=1");
                                    Port.WriteLine("AT+CLIP=1");
                                    WaitResultOrTimeout("CMGF", 500);
                                    switch (ModemType)
                                    {
                                        case ModemType.CINTERION:
                                            {
                                                Port.WriteLine("AT+CPMS=\"ME\"");
                                                WaitResultOrTimeout("\r\nOK\r\n", 500);
                                                break;
                                            }
                                        case ModemType.SIEMENS:
                                            {
                                                Port.WriteLine("AT+CPMS=\"ME\"");
                                                WaitResultOrTimeout("\r\nOK\r\n", 500);
                                                break;
                                            }
                                        default:
                                            {
                                                Port.WriteLine("AT+CPMS=\"SM\"");
                                                WaitResultOrTimeout("CPMS", 500);
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case GSMCommand.GET_CARRIER:
                                {
                                    Port.WriteLine("AT+COPS?");
                                    Thread.Sleep(1000);
                                    response = Port.ReadExisting();
                                    LogResponseCommand(response);
                                    break;
                                }
                            case GSMCommand.GET_ICCID:
                                {
                                    switch (ModemType)
                                    {
                                        case ModemType.CINTERION:
                                            {
                                                Port.WriteLine("AT^SCID");
                                                response = WaitResultOrTimeout("SCID", 2000);
                                                break;
                                            }
                                        case ModemType.SIEMENS:
                                            {
                                                Port.WriteLine("AT^SCID");
                                                response = WaitResultOrTimeout("SCID", 2000);
                                                break;
                                            }
                                        default:
                                            {
                                                Port.WriteLine("AT+ICCID?");
                                                response = WaitResultOrTimeout("ICCID", 2000);
                                                break;
                                            }
                                    }
                                    LogResponseCommand(response);
                                    break;
                                }
                            case GSMCommand.GET_PHONE_NUMBER:
                                {
                                    switch (SIMCarrier)
                                    {
                                        case SIMCarrier.NO_SIM_CARD: { break; }
                                        case SIMCarrier.VietnamMobile:
                                            {
                                                switch (this.ModemType)
                                                {
                                                    case ModemType.CINTERION:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    case ModemType.SIEMENS:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            Port.Write("AT+CUSD=1,\"*101#\",15\r");
                                                            break;
                                                        }
                                                }
                                                response = WaitResultOrTimeout("CUSD", 15000);
                                                Port.Write("AT+CUSD=2\r");
                                                WaitResultOrTimeout("CUSD", 3000);
                                                break;
                                            }
                                        case SIMCarrier.Vinaphone:
                                            {
                                                switch (this.ModemType)
                                                {
                                                    case ModemType.CINTERION:
                                                        {
                                                            Port.Write("ATDT*110#;\r");
                                                            break;
                                                        }
                                                    case ModemType.SIEMENS:
                                                        {
                                                            Port.Write("ATDT*110#;\r");
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            Port.Write("AT+CUSD=1,\"*110#\",15\r");
                                                            break;
                                                        }
                                                }
                                                response = WaitResultOrTimeout("CUSD", 20000);
                                                Port.Write("AT+CUSD=2\r");
                                                WaitResultOrTimeout("CUSD", 3000);
                                                break;
                                            }
                                        case SIMCarrier.Viettel:
                                            {
                                                switch (this.ModemType)
                                                {
                                                    case ModemType.CINTERION:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    case ModemType.SIEMENS:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            Port.Write("AT+CUSD=1,\"*101#\",15\r");
                                                            break;
                                                        }
                                                }
                                                response = WaitResultOrTimeout("CUSD", 20000);
                                                Port.Write("AT+CUSD=2\r");
                                                WaitResultOrTimeout("CUSD", 3000);
                                                break;
                                            }
                                        case SIMCarrier.Mobifone:
                                            {
                                                switch (this.ModemType)
                                                {
                                                    case ModemType.CINTERION:
                                                        {
                                                            Port.Write("ATDT*0#;\r");
                                                            break;
                                                        }
                                                    case ModemType.SIEMENS:
                                                        {
                                                            Port.Write("ATDT*0#;\r");
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            Port.Write("AT+CUSD=1,\"*0#\",15\r");
                                                            break;
                                                        }
                                                }
                                                response = WaitResultOrTimeout("CUSD", 15000);
                                                LogResponseCommand(response);
                                                break;
                                            }
                                        default: { break; }
                                    }
                                    break;
                                }
                            case GSMCommand.CHECKBALANCE:
                                {
                                    switch (SIMCarrier)
                                    {
                                        case SIMCarrier.Vinaphone:
                                            {
                                                Port.Write("AT+CUSD=2\r");
                                                WaitResultOrTimeout("CUSD", 3000);

                                                switch (this.ModemType)
                                                {
                                                    case ModemType.CINTERION:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    case ModemType.SIEMENS:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            Port.Write("AT+CUSD=1,\"*101#\",15\r");
                                                            break;
                                                        }
                                                }
                                                response = WaitResultOrTimeout("CUSD", 20000);

                                                Port.Write("AT+CUSD=2\r");
                                                WaitResultOrTimeout("CUSD", 3000);
                                                break;
                                            }
                                        case SIMCarrier.Mobifone:
                                            {
                                                switch (this.ModemType)
                                                {
                                                    case ModemType.CINTERION:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    case ModemType.SIEMENS:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            Port.Write("AT+CUSD=1,\"*101#\",15\r");
                                                            break;
                                                        }
                                                }
                                                response = WaitResultOrTimeout("CUSD", 15000);
                                                Port.Write("AT+CUSD=2\r");
                                                WaitResultOrTimeout("CUSD", 3000);
                                                break;
                                            }
                                        case SIMCarrier.Viettel:
                                            {
                                                switch (this.ModemType)
                                                {
                                                    case ModemType.CINTERION:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    case ModemType.SIEMENS:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            Port.Write("AT+CUSD=1,\"*101#\",15\r");
                                                            break;
                                                        }
                                                }
                                                response = WaitResultOrTimeout("CUSD", 15000);
                                                Port.Write("AT+CUSD=2\r");
                                                WaitResultOrTimeout("CUSD", 3000);
                                                break;
                                            }
                                        case SIMCarrier.VietnamMobile:
                                            {
                                                switch (this.ModemType)
                                                {
                                                    case ModemType.CINTERION:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    case ModemType.SIEMENS:
                                                        {
                                                            Port.Write("ATDT*101#;\r");
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            Port.Write("AT+CUSD=1,\"*101#\",15\r");
                                                            break;
                                                        }
                                                }
                                                response = WaitResultOrTimeout("CUSD", 15000);
                                                WaitResultOrTimeout("CUSD", 3000);
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case GSMCommand.RESET_INBOX:
                                {
                                    //Port.Write("AT+CMGL=\"REC UNREAD\"\r");
                                    //Thread.Sleep(1000);
                                    //GlobalEvent.ONATCommandResponse(Port.ReadExisting());

                                    Port.WriteLine("AT+CMGD=1,3\r");
                                    Thread.Sleep(1000);
                                    LogResponseCommand(Port.ReadExisting());
                                    //GlobalEvent.ONATCommandResponse(Port.ReadExisting());

                                    //Port.Write("AT+CMGL=\"REC UNREAD\"\r");
                                    //Thread.Sleep(1000);
                                    //GlobalEvent.ONATCommandResponse(Port.ReadExisting());

                                    //Port.WriteLine("AT+CMGD=1,3\r");
                                    //Thread.Sleep(1000);
                                    //GlobalEvent.ONATCommandResponse(Port.ReadExisting());

                                    response = "RESETED";
                                    break;
                                }
                            case GSMCommand.GET_NEW_MSG:
                                {
                                    if (IsSIMConnected && !string.IsNullOrEmpty(PhoneNumber))
                                    {


                                        response = string.Empty;
                                        //Port.WriteLine("AT+CPMS=\"ME\"");
                                        //WaitResultOrTimeout("CPMS", 500);
                                        //Port.Write("AT+CMGL=\"REC UNREAD\"\r");
                                        //response += "" + WaitResultOrTimeout("CMGL", 1000);
                                        //Port.WriteLine("AT+CMGD=1,3\r");
                                        //WaitResultOrTimeout("CMGD", 500);

                                        //Port.WriteLine("AT+CPMS=\"SM\"");
                                        //WaitResultOrTimeout("CPMS", 500);
                                        Port.Write("AT+CMGL=\"REC UNREAD\"\r");
                                        response += "" + WaitResultOrTimeout("CMGL", 1000);
                                        Port.WriteLine("AT+CMGD=1,3\r");
                                        WaitResultOrTimeout("CMGD", 500);

                                        //Port.WriteLine("AT+CPMS=\"MT\"");
                                        //WaitResultOrTimeout("CPMS", 500);
                                        //Port.Write("AT+CMGL=\"REC UNREAD\"\r");
                                        //response += "" + WaitResultOrTimeout("CMGL", 1000);
                                        //Port.WriteLine("AT+CMGD=1,3\r");
                                        //WaitResultOrTimeout("CMGD", 500);

                                        //Port.WriteLine("AT+CPMS=\"SM\"");
                                        //WaitResultOrTimeout("CPMS", 500);
                                        break;
                                    }
                                    return string.Empty;
                                }
                            case GSMCommand.DIAL:
                                {
                                    if (IsSIMConnected)
                                    {
                                        DialInfo dialInfo = (DialInfo)data;

                                        Port.WriteLine("ATH");
                                        Thread.Sleep(100);
                                        LogResponseCommand(Port.ReadExisting());

                                        Port.WriteLine("ATD" + dialInfo.DialNo + ";");
                                        Thread.Sleep(1000);
                                        response = Port.ReadExisting();
                                        LogResponseCommand(response);

                                    loop:
                                        Port.WriteLine("AT+CPAS");
                                        Thread.Sleep(100);
                                        response = Port.ReadExisting();
                                        if (response.Contains("CPAS: 4"))
                                        {
                                            Thread.Sleep(dialInfo.DurationLimit * 1000);
                                            Port.WriteLine("ATH");
                                            Thread.Sleep(100);
                                            LogResponseCommand(Port.ReadExisting());
                                        }
                                        else
                                        {
                                            if (response.Contains("CPAS: 3"))
                                            {
                                                goto loop;
                                            }
                                        }
                                    }
                                    break;
                                }
                            case GSMCommand.DROPCALL:
                                {
                                    if (IsSIMConnected)
                                    {
                                        Port.WriteLine("ATH");
                                        Thread.Sleep(100);
                                        response = Port.ReadExisting();
                                        LogResponseCommand(response);
                                    }
                                    break;
                                }
                            case GSMCommand.ANSWER_CALL:
                                {
                                    if (RingStatus == RingStatus.Ringing)
                                    {
                                        if (GlobalVar.AutoAnswerIncomingCall && GlobalVar.EnableVoiceRecognitionToText)
                                        {

                                            Port.WriteLine("AT+QAUDRD=0;\r");
                                            Port.WriteLine("AT+QFDEL=\"RAM:*\"\r");
                                            Thread.Sleep(1000);
                                            response = Port.ReadExisting();
                                            LogResponseCommand(response);


                                            Port.WriteLine("AT+CLIP=1");
                                            Thread.Sleep(1000);
                                            response = Port.ReadExisting();
                                            LogResponseCommand(response);
                                            Thread.Sleep(2000);
                                            response = Port.ReadExisting();
                                            LogResponseCommand(response);

                                            string callerID = Regex.Match(response, "(CLIP: \"(.*?)\")").Value.Replace("CLIP: \"", string.Empty).Replace("\"", string.Empty);


                                            DateTime answerDate = DateTime.Now;

                                            Port.WriteLine("ATA");
                                            Thread.Sleep(1000);
                                            response = Port.ReadExisting();
                                            LogResponseCommand(response);
                                            RingStatus = RingStatus.Idle;
                                            RingStatusChanged(this, RingStatus);


                                            Port.WriteLine("AT+QAUDRD=1,\"RAM:voice.wav\",13;\r");
                                            response = Port.ReadExisting();
                                            LogResponseCommand(response);

                                            DateTime startTime = DateTime.Now;
                                            while (!Stop && (DateTime.Now - startTime).TotalSeconds < 16 && IsPortConnected && IsSIMConnected && !string.IsNullOrEmpty(PhoneNumber))
                                            {
                                                response = Port.ReadExisting();
                                                LogResponseCommand(response);
                                                if (response.Contains("QAUDRIND") || response.Contains("NO CARRIER"))
                                                {
                                                    break;
                                                }
                                            }

                                            Port.WriteLine("AT+QAUDRD=0;\r");
                                            Thread.Sleep(500);
                                            response = Port.ReadExisting();
                                            LogResponseCommand(response);

                                            Port.WriteLine("AT+QFDWL=\"RAM:voice.wav\";\r");
                                            Thread.Sleep(1000);
                                        read:
                                            //Thread.Sleep(200);
                                            string audioString = "";
                                            byte[] buffer = new byte[Port.ReadBufferSize];
                                            int bytesRead = 0;
                                            try
                                            {
                                                bytesRead = Port.Read(buffer, 0, buffer.Length);
                                                if (bytesRead == 0)
                                                    break;
                                            }
                                            catch
                                            {
                                            }
                                            audioString = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                                            if (!string.IsNullOrEmpty(audioString) && !audioString.Contains("+QFLDS") && !audioString.Contains("+CPMS") && !audioString.Contains("CMT")
                                                && !audioString.Contains("ERROR"))
                                            {
                                                if (audioString.Contains("RIFF"))
                                                {
                                                    //Restart record
                                                    RIFFTimeline.Clear();
                                                    RIFFTimeline.Add(buffer.Skip(38).Take(bytesRead - 38).ToArray(), string.Empty);
                                                    goto read;
                                                }
                                                else
                                                {
                                                    if (audioString.Contains("+QFDWL"))
                                                    {
                                                        RIFFTimeline.Add(buffer.Take(bytesRead - 29).ToArray(), string.Empty);
                                                        List<byte[]> recordValue = new List<byte[]>();
                                                        foreach (var item in RIFFTimeline)
                                                            recordValue.Add(item.Key);
                                                        byte[] record = recordValue.SelectMany(s => s).ToArray();

                                                        string voiceContent = new MXHPortal().VoiceRecognitionToText(record);

                                                        //var temp = Path.GetTempFileName();
                                                        //File.WriteAllBytes(temp, record);
                                                        //string voiceContent = new SpeechToTextHelper().SpeechToText(temp);
                                                        List<string> ListNumberArray = new List<string>();
                                                        string NumberArray = string.Empty;
                                                        string AllNumbers = string.Empty;
                                                        foreach (var character in voiceContent)
                                                        {
                                                            if (Char.IsDigit(character))
                                                            {
                                                                NumberArray += Convert.ToString(character);
                                                                AllNumbers += Convert.ToString(character);
                                                            }
                                                            else
                                                            {
                                                                ListNumberArray.Add(NumberArray);
                                                                NumberArray = string.Empty;
                                                            }
                                                        }
                                                        ListNumberArray.Add(NumberArray);

                                                        string otp = string.Empty;
                                                        if (string.IsNullOrEmpty(otp))
                                                            otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 6);
                                                        if (string.IsNullOrEmpty(otp))
                                                            otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 5);
                                                        if (string.IsNullOrEmpty(otp))
                                                            otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 4);
                                                        if (string.IsNullOrEmpty(otp))
                                                            if (AllNumbers.Length >= 4 && AllNumbers.Length <= 6)
                                                                otp = AllNumbers;


                                                        GSMMessage message = new GSMMessage()
                                                        {
                                                            Receiver = PhoneNumber,
                                                            DisplayCOMName = DisplayName,
                                                            COM = PortName,
                                                            Sender = callerID,
                                                            Content = voiceContent,
                                                            Date = answerDate.ToString("dd/MM/yyyy HH:mm:ss"),
                                                            OTP = otp
                                                        };

                                                        lock (GSMControlCenter.LockGSMMessages)
                                                        {
                                                            GSMControlCenter.GSMMessages.Add(message);
                                                            GSMControlCenter.OnNewMessage(message);
                                                            NotifyAlert();
                                                        }

                                                        //File.WriteAllBytes("C:\\Users\\duong\\Desktop\\test\\111.wav", record);
                                                        int Subchunk1Size = BitConverter.ToInt32(record, 0);
                                                        break;
                                                    }
                                                    RIFFTimeline.Add(buffer.Take(bytesRead).ToArray(), string.Empty);
                                                    goto read;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (GlobalVar.AutoAnswerIncomingCall)
                                            {
                                                Port.WriteLine("ATA");
                                                Thread.Sleep(1000);
                                                response = Port.ReadExisting();
                                                LogResponseCommand(response);
                                                RingStatus = RingStatus.Idle;
                                                RingStatusChanged(this, RingStatus);
                                            }
                                        }

                                    }
                                    break;
                                }
                            case GSMCommand.CHECKRING:
                                {
                                    if (IsSIMConnected && !string.IsNullOrEmpty(PhoneNumber))
                                    {
                                        response = Port.ReadExisting();
                                        LogResponseCommand(response);
                                    }
                                    break;
                                }
                            case GSMCommand.GETCALLERID:
                                {
                                    Port.WriteLine("AT+CLIP=1");
                                    Thread.Sleep(1000);
                                    response = Port.ReadExisting();
                                    LogResponseCommand(response);

                                    break;
                                }
                            case GSMCommand.SEND_MESSAGE:
                                {
                                    if (IsSIMConnected && !string.IsNullOrEmpty(PhoneNumber))
                                    {
                                        Port.WriteLine("AT+CMGS=\"" + ((SendMessageData)data).Receiver + "\""
                                            + "\n" + ((SendMessageData)data).Content + "");
                                        WaitResultOrTimeout("CMGS", 10000);
                                    }
                                    break;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogResponseCommand(ex.Message);
                }
                return response;
            }
        }
        private SerialPort Port { get; set; }
        public List<GSMMessage> GetNewMessage()
        {
            List<GSMMessage> messages = new List<GSMMessage>();
            try
            {
                if (IsPortConnected && IsSIMConnected && !string.IsNullOrEmpty(PhoneNumber))
                {
                    try
                    {
                        string[] responsees = ExecuteCommand(GSMCommand.GET_NEW_MSG).Split(new char[] { '' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var res in responsees)
                        {
                            string response = res.Replace("\n", " ");
                            var match = Regex.Match(response, "(\\+CMGL: (.*)OK)");
                            if (match.Success && !string.IsNullOrEmpty(match.Value))
                            {
                                string[] msgs = match.Value.Split(new string[] { "+CMGL" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var msg in msgs)
                                {
                                    string msgContent = msg.Substring(msg.LastIndexOf("\""), msg.Length - msg.LastIndexOf("\"")).Replace("\"", string.Empty).Replace("\r", string.Empty);
                                    string msgHeader = msg.Substring(0, msg.LastIndexOf("\""));
                                    string[] headerAtts = msgHeader.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    string sender = headerAtts[2].Replace("\"", string.Empty);
                                    string receiveDate = headerAtts[4].Replace("\"", string.Empty);
                                    string otpDetector = string.Empty;


                                    if (msgContent.StartsWith(" "))
                                        msgContent = msgContent.Remove(0, 1);

                                    if (msgContent.EndsWith(" OK"))
                                        msgContent = msgContent.Remove(msgContent.Length - 3, 3);
                                    try
                                    {
                                        if (Regex.IsMatch(msgContent.Trim(), "(?:0[xX])?[0-9a-fA-F]+") && msgContent.Length > 3)
                                        {
                                            string hex = msgContent.Trim();
                                            byte[] bytes = Enumerable.Range(0, hex.Length)
                                                     .Where(x => x % 2 == 0)
                                                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                                     .ToArray();
                                            UTF8Encoding utf8 = new UTF8Encoding();
                                            string content = Encoding.GetEncoding("utf-16BE").GetString(bytes);
                                            if (!string.IsNullOrEmpty(content))
                                                msgContent = content;
                                        }
                                    }
                                    catch { }

                                    List<string> ListNumberArray = new List<string>();
                                    string NumberArray = string.Empty;
                                    string AllNumbers = string.Empty;
                                    foreach (var character in msgContent)
                                    {
                                        if (Char.IsDigit(character))
                                        {
                                            NumberArray += Convert.ToString(character);
                                            AllNumbers += Convert.ToString(character);
                                        }
                                        else
                                        {
                                            ListNumberArray.Add(NumberArray);
                                            NumberArray = string.Empty;
                                        }
                                    }
                                    ListNumberArray.Add(NumberArray);

                                    string otp = string.Empty;
                                    if (string.IsNullOrEmpty(otp))
                                        otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 6);
                                    if (string.IsNullOrEmpty(otp))
                                        otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 5);
                                    if (string.IsNullOrEmpty(otp))
                                        otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 4);
                                    if (string.IsNullOrEmpty(otp))
                                        if (AllNumbers.Length >= 4 && AllNumbers.Length <= 6)
                                            otp = AllNumbers;
                                    messages.Add(new GSMMessage()
                                    {
                                        Receiver = PhoneNumber,
                                        DisplayCOMName = DisplayName,
                                        COM = PortName,
                                        Sender = sender,
                                        Content = msgContent,
                                        Date = receiveDate,
                                        OTP = otp
                                    });
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return messages;
        }
        public void Call(string dialNumber, int duration = 0)
        {
            ExecuteCommand(GSMCommand.DIAL, new DialInfo() { DialNo = dialNumber, DurationLimit = (duration > 5 ? 5 : duration) });
        }
        public void Answer()
        {
            ExecuteCommand(GSMCommand.ANSWER_CALL);
        }
        public void CheckBalance(string referenceData = "")
        {
            if (Port != null && Port.IsOpen && IsSIMConnected)
            {
                string response = string.Empty;
                if (!string.IsNullOrEmpty(referenceData))
                    response = referenceData;
                else
                    response = ExecuteCommand(GSMCommand.CHECKBALANCE);
                switch (SIMCarrier)
                {
                    case SIMCarrier.Vinaphone:
                        {
                            var matchBalance = Regex.Match(response, "(TK chinh=(.*?)VND)");
                            var matchExpire = Regex.Match(response, "(\\d{2}\\/\\d{2}\\/\\d{4})");
                            if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                            {
                                MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TK chinh=", "").Replace("VND", ""));
                            }
                            if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
                            {
                                Expire = matchExpire.Value;
                            }
                            break;
                        }
                    case SIMCarrier.Mobifone:
                        {
                            var matchBalance = Regex.Match(response, "(TKC (.*?) d)");
                            var matchExpire = Regex.Match(response, "(\\d{2}\\-\\d{2}\\-\\d{4})");
                            if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                            {
                                MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TKC ", "").Replace(" d", ""));
                            }
                            if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
                            {
                                Expire = matchExpire.Value;
                            }
                            break;
                        }
                    case SIMCarrier.Viettel:
                        {
                            var matchBalance = Regex.Match(response, "(TKG: (.*?)d)");
                            var matchExpire = Regex.Match(response, "(\\d{2}\\/\\d{2}\\/\\d{4})");
                            if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                            {
                                MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TKG: ", "").Replace(".","").Replace("d", ""));
                            }
                            if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
                            {
                                Expire = matchExpire.Value;
                            }
                            break;
                        }
                    case SIMCarrier.VietnamMobile:
                        {
                            var matchBalance = Regex.Match(response, "(TKG:(.*?)d)");
                            var matchExpire = Regex.Match(response, "(\\d{2}\\/\\d{2}\\/\\d{4})");
                            if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                            {
                                MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TKG:", "").Replace("d", ""));
                            }
                            if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
                            {
                                Expire = matchExpire.Value;
                            }
                            break;
                        }
                }
                GlobalEvent.OnGlobalMessaging($"[{PhoneNumber}] -> Đã kiểm tra xong");
            }
        }
        internal void Start(string portName)
        {
            if (!ComStarted)
            {
                /*Semaphore.WaitOne();*/
                PortName = portName;
                PortConnectionHandler = new Thread(new ThreadStart(PortConnectionHanding));
                SIMConnectionHandler = new Thread(new ThreadStart(SIMConnectionHanding));
                /*GSMMessageHandler = new Thread(new ThreadStart(GSMMessageHanding));*/
                AlertSMSHandler = new Thread(new ThreadStart(AlertSMSHanding));
                AlertCallHandler = new Thread(new ThreadStart(AlertCallHanding));


                PortConnectionHandler.Start();
                SIMConnectionHandler.Start();
                /*GSMMessageHandler.Start();*/
                AlertSMSHandler.Start();
                AlertCallHandler.Start();
                ComStarted = true;

            }
        }

        Dictionary<byte[], string> RIFFTimeline = new Dictionary<byte[], string>();

        private void PortConnectionHanding()
        {
            while (!Stop)
            {
                if (DoNotConnect)
                {
                    ResetInfo();
                    if (Port != null && Port.IsOpen)
                    {
                        Port.Close();
                        Port = null;
                    }
                    continue;
                }
                try
                {
                    if (Port == null)
                    {
                        IsSIMConnected = false;
                        SIMCarrier = SIMCarrier.NO_SIM_CARD;
                        PhoneNumber = string.Empty;
                        Port = new SerialPort()
                        {
                            PortName = PortName,
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
                        IsSIMConnected = false;
                        SIMCarrier = SIMCarrier.NO_SIM_CARD;
                        PhoneNumber = string.Empty;
                        Port = new SerialPort()
                        {
                            PortName = PortName,
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
                        try
                        {
                            Port.RtsEnable = true;
                        }
                        catch { }

                        lock (RequestLocker)
                        {

                            Port.WriteLine("AT+CMGF=1");
                            Port.WriteLine("AT+CLIP=1");
                            WaitResultOrTimeout("CMGF", 500);
                            Thread.Sleep(1000);
                            Port.ReadExisting();
                            Port.WriteLine("ATI");
                            Thread.Sleep(2000);
                            string response = Port.ReadExisting();
                            ModemType = response.ToUpper().Contains(ModemType.CINTERION.ToString()) ? ModemType.CINTERION
                                : response.ToUpper().Contains(ModemType.SIEMENS.ToString()) ? ModemType.SIEMENS
                                : response.ToUpper().Contains(ModemType.QUECTEL.ToString()) ? ModemType.QUECTEL
                                : response.ToUpper().Contains(ModemType.WAVECOME.ToString()) ? ModemType.WAVECOME
                                : ModemType.UNSUPPORTED;
                        }
                        IsPortConnected = true;
                        GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Mở kết nối");
                        Thread.Sleep(1000); //CMT SLEEP
                    }
                    else
                    {
                        IsPortConnected = true;
                    }
                }
                catch
                {
                    IsPortConnected = false;
                    ResetInfo();
                }
                Thread.Sleep(1000);
            }
            ResetInfo();
            if (Port != null && Port.IsOpen)
            {
                Port.Close();
                Port = null;
            }
        }
        private void SIMConnectionHanding()
        {
            int reconnect = 0;
            while (!Stop)
            {
            reconnect:
                try
                {
                    if (Port != null && Port.IsOpen)
                    {
                        string checkRing = ExecuteCommand(GSMCommand.CHECKRING);
                        if (IsSIMConnected)
                            RingDetector(checkRing);
                        int remain = 5;
                    loop:
                        string cpin = ExecuteCommand(GSMCommand.CPIN);
                        if (IsSIMConnected && (!cpin.Contains("READY") || !cpin.Contains("OK")) && remain > 0)
                        {
                            remain--;
                            goto loop;
                        }

                        if (cpin.Contains("OK") && cpin.Contains("READY"))
                        {
                            reconnect = 0;
                            if (!IsSIMConnected)
                            {
                                lock (RequestLocker)
                                {
                                    ExecuteCommand(GSMCommand.SWITCH_TEXT_MODE);

                                    string carrierMsg = ExecuteCommand(GSMCommand.GET_CARRIER).ToUpper();
                                    SIMCarrier = carrierMsg.ToUpper().Contains("VIETNAMOBILE") ? SIMCarrier.VietnamMobile
                                        : (carrierMsg.ToUpper().Contains("VIETTEL") || carrierMsg.ToUpper().Contains("45204")) ? SIMCarrier.Viettel
                                        : carrierMsg.ToUpper().Contains("MOBIFONE") ? SIMCarrier.Mobifone
                                        : (carrierMsg.ToUpper().Contains("VINAPHONE") || carrierMsg.ToUpper().Contains("45202")) ? SIMCarrier.Vinaphone
                                        : SIMCarrier.NO_SIM_CARD;
                                    if (SIMCarrier != SIMCarrier.NO_SIM_CARD)
                                    {
                                        if (ModemType == ModemType.UNSUPPORTED)
                                        {
                                            Port.WriteLine("ATI");
                                            Thread.Sleep(500);
                                            string response = Port.ReadExisting();
                                            ModemType = response.ToUpper().Contains(ModemType.CINTERION.ToString()) ? ModemType.CINTERION
                                                : response.ToUpper().Contains(ModemType.SIEMENS.ToString()) ? ModemType.SIEMENS
                                                : response.ToUpper().Contains(ModemType.QUECTEL.ToString()) ? ModemType.QUECTEL
                                                : response.ToUpper().Contains(ModemType.WAVECOME.ToString()) ? ModemType.WAVECOME
                                                : ModemType.UNSUPPORTED;
                                        }
                                        Serial = Regex.Match(ExecuteCommand(GSMCommand.GET_ICCID), "([A-Za-z0-9]{20,22})").Value;
                                    }
                                    string phoneNumberMSG = ExecuteCommand(GSMCommand.GET_PHONE_NUMBER);
                                    switch (SIMCarrier)
                                    {
                                        case SIMCarrier.VietnamMobile:
                                            {
                                                var match = Regex.Match(phoneNumberMSG, "(CUSD: 1,\"Xin chao (.*?)\n)");

                                                if (match.Success && !string.IsNullOrEmpty(match.Value))
                                                {
                                                    string phoneNumber = match.Value.Replace("CUSD: 1,\"Xin chao ", string.Empty).Replace("\n", string.Empty);

                                                    phoneNumber = phoneNumber.Replace("+", string.Empty);
                                                    if (phoneNumber.Length == 9)
                                                        phoneNumber = phoneNumber.Insert(0, "0");
                                                    if (phoneNumber.StartsWith("84"))
                                                        phoneNumber = phoneNumber.Remove(0, 2).Insert(0, "0");

                                                    this.PhoneNumber = phoneNumber;
                                                    GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Đã nhận SIM số [{PhoneNumber}]");
                                                    IsSIMConnected = true;
                                                    CheckBalance(PhoneNumber);
                                                }
                                                else ResetInfo();
                                                break;
                                            }
                                        case SIMCarrier.Vinaphone:
                                            {
                                                var match = Regex.Match(phoneNumberMSG, "(\\d{9})");
                                                if (match.Success && !string.IsNullOrEmpty(match.Value))
                                                {
                                                    string phoneNumber = match.Value;
                                                    phoneNumber = phoneNumber.Replace("+", string.Empty);
                                                    if (phoneNumber.Length == 9)
                                                        phoneNumber = phoneNumber.Insert(0, "0");
                                                    if (phoneNumber.StartsWith("84"))
                                                        phoneNumber = phoneNumber.Remove(0, 2).Insert(0, "0");
                                                    this.PhoneNumber = phoneNumber;
                                                    GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Đã nhận SIM số [{PhoneNumber}]");
                                                    IsSIMConnected = true;
                                                    CheckBalance(PhoneNumber);
                                                }
                                                else ResetInfo();
                                                break;
                                            }
                                        case SIMCarrier.Viettel:
                                            {
                                                var match = Regex.Match(phoneNumberMSG, "(\\d{11})");
                                                if (match.Success && !string.IsNullOrEmpty(match.Value))
                                                {
                                                    string phoneNumber = match.Value;
                                                    phoneNumber = phoneNumber.Replace("+", string.Empty);
                                                    if (phoneNumber.Length == 9)
                                                        phoneNumber = phoneNumber.Insert(0, "0");
                                                    if (phoneNumber.StartsWith("84"))
                                                        phoneNumber = phoneNumber.Remove(0, 2).Insert(0, "0");
                                                    this.PhoneNumber = phoneNumber;
                                                    GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Đã nhận SIM số [{PhoneNumber}]");
                                                    IsSIMConnected = true;
                                                    CheckBalance();
                                                    /*MVTGlobalVar.RegisterVar.OnSIMInjected(phoneNumber);*/
                                                }
                                                else ResetInfo();
                                                break;
                                            }
                                        case SIMCarrier.Mobifone:
                                            {
                                                var match = Regex.Match(phoneNumberMSG, "(\\d{11})");
                                                if (match.Success && !string.IsNullOrEmpty(match.Value))
                                                {
                                                    string phoneNumber = match.Value;
                                                    phoneNumber = phoneNumber.Replace("+", string.Empty);
                                                    if (phoneNumber.Length == 9)
                                                        phoneNumber = phoneNumber.Insert(0, "0");
                                                    if (phoneNumber.StartsWith("84"))
                                                        phoneNumber = phoneNumber.Remove(0, 2).Insert(0, "0");
                                                    this.PhoneNumber = phoneNumber;
                                                    GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Đã nhận SIM số [{PhoneNumber}]");
                                                    IsSIMConnected = true;
                                                    CheckBalance();
                                                    /*MMFGlobarVar.RegisterVar.OnSIMInjected(phoneNumber);*/
                                                }
                                                else ResetInfo();
                                                break;
                                            }
                                        default:
                                            {
                                                //if (!string.IsNullOrEmpty(PhoneNumber))
                                                //    GlobalEvent.OnGlobalMessaging($"{PortName} -> {PhoneNumber} -> Disconnected");
                                                this.PhoneNumber = string.Empty;
                                                GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Đã nhận SIM số [{PhoneNumber}]");
                                                break;
                                            }
                                    }
                                }
                            }
                            else
                            {
                                RingDetector(cpin);
                            }
                        }
                        else
                        {
                            RingDetector(cpin);
                            if (IsSIMConnected)
                            {
                                if ((reconnect < 7 && ModemType == ModemType.CINTERION) || (reconnect < 2 && ModemType != ModemType.CINTERION))
                                {
                                    reconnect++;
                                    goto reconnect;
                                }
                                else
                                {
                                    //GlobalEvent.OnGlobalMessaging($"{PortName} -> {PhoneNumber} -> Disconnected");
                                }
                            }
                            ResetInfo();
                        }
                    }
                    else
                    {
                        if (IsSIMConnected)
                        {
                            if ((reconnect < 7 && ModemType == ModemType.CINTERION) || (reconnect < 2 && ModemType != ModemType.CINTERION))
                            {
                                reconnect++;
                                goto reconnect;
                            }
                            else
                            {
                                //GlobalEvent.OnGlobalMessaging($"{PortName} -> {PhoneNumber} -> Disconnected");
                            }
                        }
                        ResetInfo();
                    }
                }
                catch
                {
                    if (IsSIMConnected)
                    {
                        if ((reconnect < 7 && ModemType == ModemType.CINTERION) || (reconnect < 2 && ModemType != ModemType.CINTERION))
                        {
                            reconnect++;
                            goto reconnect;
                        }
                        else
                        {
                            //GlobalEvent.OnGlobalMessaging($"{PortName} -> {PhoneNumber} -> Disconnected");
                        }
                    }
                    ResetInfo();
                }
                Thread.Sleep(500); //cmt sleep 500
            }
            //if (IsSIMConnected)
            //    GlobalEvent.OnGlobalMessaging($"{PortName} -> {PhoneNumber} -> Disconnected");
            ResetInfo();
        }

        private DateTime VNMBUpdateDate = new DateTime(1900, 01, 01);
        private void GSMMessageHanding()
        {
            while (!Stop)
            {
                try
                {
                    if (IsPortConnected && IsSIMConnected && GlobalVar.RealtimeSMSTracking)
                    {
                        var messages = GetNewMessage();
                        foreach (var message in messages)
                        {
                            lock (GSMControlCenter.LockGSMMessages)
                            {
                                GSMControlCenter.GSMMessages.Add(message);
                                GSMControlCenter.OnNewMessage(message);
                                NotifyAlert();
                            }
                            if (SIMCarrier == SIMCarrier.VietnamMobile && (message.Sender == "123" || message.Sender == "+123") && Regex.IsMatch(message.Content, "(TK Chinh: (.*?)het han (\\d{2}\\/\\d{2}\\/\\d{4}))"))
                            {
                                try
                                {
                                    CultureInfo provider = CultureInfo.InvariantCulture;
                                    string date = message.Date.Substring(0, message.Date.IndexOf("+"));
                                    var accountDate = DateTime.ParseExact(date, "yyyy/MM/dd HH:mm:ss", provider);
                                    if (accountDate > VNMBUpdateDate)
                                    {
                                        string accountInfo = Regex.Match(message.Content, "(TK Chinh: (.*?)het han (\\d{2}\\/\\d{2}\\/\\d{4}))").Value;
                                        var matchBalance = Regex.Match(accountInfo, "(TK Chinh: (.*?)d,)");
                                        var matchExpire = Regex.Matches(accountInfo, "(\\d{2}\\/\\d{2}\\/\\d{4})");
                                        if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                                        {
                                            MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TK Chinh: ", "").Replace("d,", "").Replace(".", string.Empty).Replace(",", string.Empty));
                                        }
                                        if (matchExpire != null && matchExpire.Count > 0)
                                        {
                                            Expire = matchExpire[0].Value;
                                        }
                                        VNMBUpdateDate = accountDate;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                catch { }
                Thread.Sleep(3000);
            }
        }
        public void SendMessage(string receiver, string content)
        {
            if (IsSIMConnected && !string.IsNullOrEmpty(PhoneNumber))
            {
                ExecuteCommand(GSMCommand.SEND_MESSAGE, new SendMessageData() { Receiver = receiver, Content = content });
            }
        }
        internal void Dispose()
        {
           /* MVTGlobalVar.RegisterVar.OnSIMRejected(PhoneNumber);
            MMFGlobarVar.RegisterVar.OnSIMRejected(PhoneNumber);*/
            Stop = true;
        }
        private void ResetInfo()
        {
           /* MVTGlobalVar.RegisterVar.OnSIMRejected(PhoneNumber);
            MMFGlobarVar.RegisterVar.OnSIMRejected(PhoneNumber);*/
            SIMCarrier = SIMCarrier.NO_SIM_CARD;
            PhoneNumber = string.Empty;
            IsSIMConnected = false;
            Expire = string.Empty;
            MainBalance = 0;
            MyRegisterState = MyRegisterState.None;
            VNMBUpdateDate = new DateTime(1900, 01, 01);
            Serial = string.Empty;
            LastUSSDCommand = string.Empty;
            LastUSSDResult  = string.Empty;
        }
        public void Reconnect()
        {
            try
            {
                if (Port != null && Port.IsOpen)
                {
                    Port.Close();
                    Port = null;
                }
                DoNotConnect = false;
                IsPortConnected = false;
                ResetInfo();
                //GlobalEvent.OnGlobalMessaging($"[{PortName} -> Disconnected]");
            }
            catch { }
        }
        private string WaitResultOrTimeout(string containSucceed, int timeout)
        {
            try
            {
                DateTime startTime = DateTime.Now;
                string result = string.Empty;
            loop:
                if ((DateTime.Now - startTime).TotalMilliseconds > timeout
                    || !IsPortConnected || Stop)
                {
                    LogResponseCommand(result);
                    return string.Empty;
                }

                string response = Port.ReadExisting();
                result += response;
                //LogResponseCommand(response);
                if (result.Contains(containSucceed) && result.Contains("OK") || (string.IsNullOrEmpty(containSucceed) && result.Contains("OK")))
                {
                    if (string.IsNullOrEmpty(response))
                    {
                        LogResponseCommand(result);
                        return result;
                    }
                    else
                    {
                        Thread.Sleep(500);
                        goto loop;
                    }
                }
                else goto loop;
            }
            catch { }
            return string.Empty;
        }
        private void WaitEmptyResponse()
        {
            try
            {
                DateTime startTime = DateTime.Now;
            loop:
                if (Stop)
                    return;

                string response = Port.ReadExisting();
                LogResponseCommand(response);
                if (string.IsNullOrEmpty(response))
                    return;
                else goto loop;
            }
            catch { }
        }
        public bool SignalAlert { get; set; }
        DateTime AlertTime = new DateTime();
        private void NotifyAlert()
        {
            AlertTime = DateTime.Now;
        }
        private void AlertSMSHanding()
        {
            while (!Stop)
            {
                try
                {
                    if ((DateTime.Now - AlertTime).TotalSeconds < 1)
                    {
                        SignalAlert = !SignalAlert;
                        if (SignalAlert)
                        {
                            if (GlobalVar.EnableSMSRing)
                            {
                                GlobalVar.Beep(1500, 100);
                                GlobalVar.Beep(1500, 100);
                            }
                        }
                    }
                    else
                    {
                        SignalAlert = false;
                    }
                 
                }
                catch { }
                Thread.Sleep(500);
            }
        }
        private void AlertCallHanding()
        {
            while (!Stop)
            {
                try
                {
                    if (RingStatus == RingStatus.Ringing)
                    {
                        if (GlobalVar.EnableIncomingCallRing)
                        {
                            GlobalVar.Beep(1500, 300);
                            GlobalVar.Beep(1500, 300);
                        }
                    }
                }
                catch { }
                Thread.Sleep(500);
            }
        }

        public string ExecuteSingleUSSD(string ussd)
        {
            LastUSSDCommand = ussd;
            LastUSSDResult = string.Empty;
            lock (RequestLocker)
            {
                WaitEmptyResponse();
                Port.Write("AT+CUSD=2\r");
                WaitResultOrTimeout("CUSD", 3000);

                try
                {
                    GlobalEvent.OnGlobalMessaging($"[{PhoneNumber}] -> Đang chạy USSD");
                    string response = string.Empty;
                    switch (ModemType)
                    {
                        case ModemType.CINTERION:
                            {
                                Port.Write("ATDT" + ussd + ";\r");
                                response = WaitResultOrTimeout("CUSD", 15000);
                                break;
                            }
                        case ModemType.SIEMENS:
                            {
                                Port.Write("ATDT" + ussd + ";\r");
                                response = WaitResultOrTimeout("CUSD", 15000);
                                break;
                            }
                        default:
                            {
                                Port.Write("AT+CUSD=1,\"" + ussd + "\",15\r");
                                response = WaitResultOrTimeout("CUSD", 15000);
                                break;
                            }
                    }
                    response = response.Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 1,\"", string.Empty)
                       .Replace(",15\r\n\r\nOK\r\n", string.Empty)
                       .Replace("\r\nOK\r\n", string.Empty)
                       .Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 2,\"", string.Empty)
                       .Replace("\"", string.Empty)
                       .Replace("+CUSD: 1,\"", string.Empty)
                       .Replace("+CUSD: 2,\"", string.Empty)

                        .Replace("\"", string.Empty)
                       .Replace("\n", string.Empty)
                       .Replace("\r", string.Empty);
                    response = response.Replace("+CUSD: 1,", string.Empty);
                    response = response.Replace("+CUSD: 2,", string.Empty);
                    LastUSSDResult = response;
                    GlobalEvent.OnGlobalMessaging($"[{PhoneNumber}] -> Đã nhận USSD thành công");
                }
                catch { GlobalEvent.OnGlobalMessaging($"[{PhoneNumber}] -> Đã nhận USSD thất bạt "); }
            }
            Thread.Sleep(300);
            return LastUSSDResult;
        }

        public Action<string> USSDRequest = (ussd) => { };
        public Action<string> USSDResponse = (response) => { };
        public Action USSDCancel = () => { };
        public Action USSDReset = () => { };

        public void ResetUSSDEvent()
        {
            USSDRequest = (ussd) => { };
            USSDResponse = (response) => { };
            USSDCancel = () => { };
            USSDReset = () => { };
        }

        public void USSDHook()
        {
            lock (RequestLocker)
            {
                bool hooking = true;
                WaitEmptyResponse();
                USSDCancel += () => { try { hooking = false; } catch { } };
                USSDRequest += (ussd) =>
                {
                    try
                    {
                        string response = string.Empty;
                        switch (ModemType)
                        {
                            case ModemType.CINTERION:
                                {
                                    Port.Write("ATDT" + ussd + ";\r");
                                    response = WaitResultOrTimeout("CUSD", 15000);
                                    break;
                                }
                            case ModemType.SIEMENS:
                                {
                                    Port.Write("ATDT" + ussd + ";\r");
                                    response = WaitResultOrTimeout("CUSD", 15000);
                                    break;
                                }
                            default:
                                {
                                    Port.Write("AT+CUSD=1,\"" + ussd + "\",15\r");
                                    response = WaitResultOrTimeout("CUSD", 15000);
                                    break;
                                }
                        }
                        response = response.Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 1,\"", string.Empty)
                        .Replace(",15\r\n\r\nOK\r\n", string.Empty)
                        .Replace("\r\nOK\r\n", string.Empty)
                        .Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 2,\"", string.Empty)
                        .Replace("\"", string.Empty)
                        .Replace("+CUSD: 1,\"", string.Empty)
                        .Replace("+CUSD: 2,\"", string.Empty)

                         .Replace("\"", string.Empty)
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty);
                        response = response.Replace("+CUSD: 1,", string.Empty);
                        response = response.Replace("+CUSD: 2,", string.Empty);

                        USSDResponse(response);
                    }
                    catch { }
                };
                USSDReset += () =>
                {
                    try
                    {
                        Port.Write("AT+CUSD=2\r");
                        WaitResultOrTimeout("CUSD", 3000);
                    }
                    catch { }
                };
                while (hooking && !GlobalVar.IsApplicationExit)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }

    public class CommandComplete
    {
        public CommandCompleteResult CommandCompleteResult { get; set; }
        public string Data { get; set; }
    }



    public enum CommandCompleteResult
    {
        Completed,
        Failure
    }

    public enum SIMCarrier
    {
        NO_SIM_CARD,
        Vinaphone,
        Mobifone,
        VietnamMobile,
        Viettel
    }
    public enum GSMCommand
    {
        AT = 0,
        CPIN = 1,
        GET_NEW_MSG = 2,
        GET_UNREAD_MSG = 3,
        DELETE_READED_MSG = 4,
        GET_CARRIER = 5,
        GET_PHONE_NUMBER = 6,
        SWITCH_TEXT_MODE = 7,
        RESET_INBOX = 8,
        DIAL = 9,
        CHECKRING = 10,
        GETCALLERID = 11,
        ANSWER_CALL = 12,
        CHECKBALANCE = 13,
        DROPCALL = 14,
        SEND_MESSAGE = 15,
        GET_ICCID = 16
    }

    public enum RingStatus
    {
        Idle,
        Ringing
    }

    public class DialInfo
    {
        public string DialNo { get; set; }
        public int DurationLimit { get; set; }
    }

    public enum MyRegisterState
    {
        None,
        Processing,
        Succeed,
        NoOTP,
        Failed
    }



}
