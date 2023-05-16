using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MXH
{
    public static class Utils
    {
        public static string MD5Encrypt(string data)
        {
            string str;
            using (MD5 md5 = MD5.Create())
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte num in md5.ComputeHash(Encoding.UTF8.GetBytes(data)))
                    stringBuilder.Append(num.ToString("x2").ToLower());
                str = stringBuilder.ToString();
            }
            return str;
        }
    }
}
