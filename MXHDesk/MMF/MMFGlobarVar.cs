using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MXH.MMF
{
    public static class MMFGlobarVar
    {
        private static bool _START = false;

        public static List<MMFAccount> Accounts = new List<MMFAccount>();
        private static Thread UpdateInfoHandler = null;
        private static Thread ChangePasswordHandler = null;

        public static void StartHanding()
        {
            if (_START)
                return;
            UpdateInfoHandler = new Thread(new ThreadStart(UpdateInfoHanding));
            ChangePasswordHandler = new Thread(new ThreadStart(ChangePasswordHanding));

            UpdateInfoHandler.Start();
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
                    var account = GlobalVar.GetSequence<MMFAccount>(ObjectSequenceType.MMFUpdateInfo);

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
                    var account = GlobalVar.GetSequence<MMFAccount>(ObjectSequenceType.MVTChangePassword);

                    if (account != null)
                        account.ChangePassword(ChangePasswordVar.NewPassword);

                    lock (ChangePasswordVar.LockRunningThread)
                        ChangePasswordVar.RunningThread--;
                }).Start();
            }
        }


        public static class UpdateInfoVar
        {
            public static int TotalThread = 20;
            public static int RunningThread = 0;
            public static object LockRunningThread = new object();
        }


        public static class ChangePasswordVar
        {
            public static string NewPassword = string.Empty;
            public static int TotalThread = 10;
            public static int RunningThread = 0;
            public static object LockRunningThread = new object();
        }
        public static class RegisterVar
        {
            public static int OTPTimeout = 60;
            public static int TotalThread = 15;
            public static int RunningThread = 0;
            public static string Password { get; set; }

            public static bool Stop = false;

            private static BindingList<MMFRegisterQueue> Queues = new BindingList<MMFRegisterQueue>();

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
                        if (existed.QueueState == MMFRegisterQueueState.Failed)
                        {
                            existed.Resolved = false;
                            existed.QueueState = MMFRegisterQueueState.None;
                        }
                        existed.QueueTime = DateTime.Now;
                    }
                    else
                    {
                        Queues.Add(new MMFRegisterQueue()
                        {
                            PhoneNumber = phoneNumber,
                            QueueState = MMFRegisterQueueState.None,
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
                    && queue.QueueState == MMFRegisterQueueState.None);
                }
            }
            public static MMFRegisterQueue GetQueue()
            {
                lock (Queues)
                {
                    var _queue = Queues.Where(queue => !queue.Resolved
                    && queue.QueueState == MMFRegisterQueueState.None)
                        .OrderBy(queue => queue.QueueTime).FirstOrDefault();
                    if (_queue != null)
                        _queue.QueueState = MMFRegisterQueueState.Processing;
                    return _queue;
                }
            }

        }
    }
}
