using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MXH.MVT
{
    public class MVTRegisterQueue
    {
        public DateTime QueueTime { get; set; }
        public string PhoneNumber { get; set; }
        public bool Resolved { get; set; }
        public MVTRegisterQueueState QueueState { get; set; }
    }
    public enum MVTRegisterQueueState
    {
        None,
        Processing,
        Failed,
        Succeed,
    }
}
