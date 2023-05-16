using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MXH.MVT
{
    public static class MVTGlobalVar
    {
        private static bool _START = false;
        public static BindingList<MVTAccount> Accounts = new BindingList<MVTAccount>();
        public static BindingList<MVTVoucherInfo> MVTVoucherInfos = new BindingList<MVTVoucherInfo>();
        private static Thread UpdateInfoHandler = null;
        private static Thread VoucherExchangeHandler = null;
        private static Thread ChangePasswordHandler = null;
        public static void StartHanding()
        {
            if (_START)
                return;
            UpdateInfoHandler = new Thread(new ThreadStart(UpdateInfoHanding));
            VoucherExchangeHandler = new Thread(new ThreadStart(VoucherExchangeHanding));
            ChangePasswordHandler = new Thread(new ThreadStart(ChangePasswordHanding));

            UpdateInfoHandler.Start();
            VoucherExchangeHandler.Start();
            ChangePasswordHandler.Start();

            _START = true;
        }
        private static void UpdateInfoHanding()
        {
            while (!GlobalVar.IsApplicationExit)
            {
            loop:
                lock (UpdateInfoVar.LockRunningThread)
                {
                    if (UpdateInfoVar.TotalThread < UpdateInfoVar.RunningThread)
                    {
                        Thread.Sleep(1000);
                        goto loop;
                    }
                }

                lock (UpdateInfoVar.LockRunningThread)
                    UpdateInfoVar.RunningThread++;

                new Task(() =>
                {
                    var account = GlobalVar.GetSequence<MVTAccount>(ObjectSequenceType.MVTUpdateInfo);

                    if (account != null)
                        account.Login();

                    lock (UpdateInfoVar.LockRunningThread)
                        UpdateInfoVar.RunningThread--;

                }).Start();
            }
        }
        private static void ChangePasswordHanding()
        {
            while (!GlobalVar.IsApplicationExit)
            {
            loop:
                lock (ChangePasswordVar.LockRunningThread)
                {
                    if (ChangePasswordVar.TotalThread < ChangePasswordVar.RunningThread)
                    {
                        Thread.Sleep(1000);
                        goto loop;
                    }
                }

                lock (ChangePasswordVar.LockRunningThread)
                    ChangePasswordVar.RunningThread++;

                new Task(() =>
                {
                    var account = GlobalVar.GetSequence<MVTAccount>(ObjectSequenceType.MVTChangePassword);

                    if (account != null)
                        account.ChangePassword(ChangePasswordVar.NewPassword);

                    lock (ChangePasswordVar.LockRunningThread)
                        ChangePasswordVar.RunningThread--;
                }).Start();
            }
        }
        private static void VoucherExchangeHanding()
        {
            while (!GlobalVar.IsApplicationExit)
            {
            loop:
                switch (VoucherExchangeVar.VoucherExchangeMode)
                {
                    case VoucherExchangeMode.Normal:
                        {
                            lock (VoucherExchangeVar.LockRunningThread)
                            {
                                if (VoucherExchangeVar.TotalThread < VoucherExchangeVar.RunningThread)
                                {
                                    Thread.Sleep(1000);
                                    goto loop;
                                }
                            }

                            lock (VoucherExchangeVar.LockRunningThread)
                                VoucherExchangeVar.RunningThread++;
                            new Task(() =>
                            {
                                var exchangeSequence = GlobalVar.GetSequence<ObjectMVTExchangeVoucherSequence>(ObjectSequenceType.MVTExchangeVoucher);
                                if (exchangeSequence != null && exchangeSequence.MVTAccount != null && exchangeSequence.PromotionInfo != null)
                                {
                                    exchangeSequence.MVTAccount.ExchangeVoucher(exchangeSequence.PromotionInfo);
                                }

                                lock (VoucherExchangeVar.LockRunningThread)
                                    VoucherExchangeVar.RunningThread--;
                            }).Start();

                            break;
                        }
                    case VoucherExchangeMode.Dcom:
                        {

                            new Task(() =>
                            {
                                List<Task> tasks = new List<Task>();
                                for (int i = 0; i < 10; i++)
                                {
                                    var seq = GlobalVar.GetSequence<ObjectMVTExchangeVoucherSequence>(ObjectSequenceType.MVTExchangeVoucher);
                                    if (seq != null && seq.MVTAccount != null && seq.PromotionInfo != null)
                                    {
                                        tasks.Add(new Task(() =>
                                        {
                                            seq.MVTAccount.ExchangeVoucher(seq.PromotionInfo);
                                        }));
                                    }
                                }

                                if (tasks.Any())
                                {
                                    lock (VoucherExchangeVar.lockDcom)
                                    {
                                        NetworkHelper.ResetDcomConnection();
                                        foreach (var task in tasks)
                                            task.Start();
                                        Task.WaitAll(tasks.ToArray());
                                    }
                                }
                            }).Start();
                            Thread.Sleep(3000);
                            break;
                        }
                }

            }
        }
        public static class RegisterVar
        {
            public static int OTPTimeout = 60;
            public static int TotalThread = 15;
            public static int RunningThread = 0;
            public static string Password { get; set; }
            public static bool Stop = false;
            private static BindingList<MVTRegisterQueue> Queues = new BindingList<MVTRegisterQueue>();

            public static object LockRunningThread = new object();
            public static void OnEachCompleted()
            {
                lock (LockRunningThread)
                {
                    RunningThread--;
                }
            }
            public static void Reset()
            {
                Password = string.Empty;
                TotalThread = 20;
                RunningThread = 0;
                Stop = false;
            }
            public static void OnSIMInjected(string phoneNumber)
            {
                lock (Queues)
                {
                    var existed = Queues.FirstOrDefault(queue => queue.PhoneNumber == phoneNumber);
                    if (existed != null)
                    {
                        if (existed.QueueState == MVTRegisterQueueState.Failed)
                        {
                            existed.Resolved = false;
                            existed.QueueState = MVTRegisterQueueState.None;
                        }
                        existed.QueueTime = DateTime.Now;
                    }
                    else
                    {
                        Queues.Add(new MVTRegisterQueue()
                        {
                            PhoneNumber = phoneNumber,
                            QueueState = MVTRegisterQueueState.None,
                            QueueTime = DateTime.Now,
                            Resolved = false
                        });
                    }
                }
            }
            public static void OnSIMRejected(string phoneNumber)
            {
                lock (Queues)
                {
                    var existed = Queues.FirstOrDefault(queue => queue.PhoneNumber == phoneNumber);
                    if (existed != null)
                    {
                        //if (existed.QueueState == MVTRegisterQueueState.None)
                        Queues.Remove(existed);
                    }
                }
            }
            public static bool HasQueue()
            {
                lock (Queues)
                {
                    return Queues.Any(queue => !queue.Resolved
                    && queue.QueueState == MVTRegisterQueueState.None);
                }
            }
            public static MVTRegisterQueue GetQueue()
            {
                lock (Queues)
                {
                    var _queue = Queues.Where(queue => !queue.Resolved
                    && queue.QueueState == MVTRegisterQueueState.None)
                        .OrderBy(queue => queue.QueueTime).FirstOrDefault();
                    if (_queue != null)
                        _queue.QueueState = MVTRegisterQueueState.Processing;
                    return _queue;
                }
            }
        }
        public static class UpdateInfoVar
        {
            public static int TotalThread = 20;
            public static int RunningThread = 0;
            public static object LockRunningThread = new object();
        }
        public static class VoucherExchangeVar
        {
            public static object lockDcom = new object();
            public static VoucherExchangeMode VoucherExchangeMode = VoucherExchangeMode.Normal;
            public static int TotalThread = 10;
            public static int RunningThread = 0;
            public static object LockRunningThread = new object();
            private static object lockMVTVoucherInfos = new object();
            public static void VoucherExchanged(MVTVoucherInfo voucher)
            {
                lock (lockMVTVoucherInfos)
                {
                    MVTVoucherInfos.Add(voucher);
                }
            }
        }

        public static class ChangePasswordVar
        {
            public static string NewPassword = string.Empty;
            public static int TotalThread = 10;
            public static int RunningThread = 0;
            public static object LockRunningThread = new object();
        }
    }
    public enum VoucherExchangeMode
    {
        Normal,
        Dcom
    }
}
