using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MXH
{
    public class ObjectSequence
    {
        public object @Object { get; set; }
        public DateTime SeqTime { get; set; }
        public ObjectSequenceState ObjectSequenceState { get; set; }
        public ObjectSequenceType ObjectSequenceType { get; set; }
    }
    public enum ObjectSequenceState
    {
        New, Processed
    }
    public enum ObjectSequenceType
    {
        MVTUpdateInfo,
        MVTChangePassword,
        MVTExchangeVoucher,

        MMFUpdateInfo,
        MMFChangePassword,

    }
}
