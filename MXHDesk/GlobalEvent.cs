using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MXH
{
    public static class GlobalEvent
    {
        public static Action<string> OnGlobalMessaging = (message) => { };
        public static Action<string> ONATCommandResponse = (response) => { };
    }
}
