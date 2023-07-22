using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBackup.Class
{
    public static class Function
    {
        public static string Between(string data, string kiri, string kanan, int max = 0)
        {
            if (data.Length == 0)
            {
                return string.Empty;
            }
            int num = 0;
            if (kiri == null)
            {
                kiri = string.Empty;
            }
            if (kanan == null)
            {
                kanan = string.Empty;
            }
            string text = data.ToLower();
            kiri = kiri.ToLower();
            kanan = kanan.ToLower();
            if (kiri.IndexOf("{}") >= 0)
            {
                kanan = kiri.Split(new char[]
                {
                    '}'
                })[1];
                kiri = kiri.Split(new char[]
                {
                    '{'
                })[0];
            }
            if (kiri.Length > 0)
            {
                num = text.IndexOf(kiri);
                if (num < 0)
                {
                    return string.Empty;
                }
                num += kiri.Length;
            }
            if (kanan.Length <= 0)
            {
                string text2 = data.Substring(num);
                if (max > 0 && text2.Length > max)
                {
                    text2 = text2.Substring(0, max);
                }
                return text2;
            }
            int num2 = text.IndexOf(kanan, num);
            if (num2 < 0)
            {
                return string.Empty;
            }
            num2 -= num;
            if (max > 0)
            {
                string text3 = data.Substring(num, num2);
                if (text3.Length > max)
                {
                    text3 = text3.Substring(0, max);
                }
                return text3;
            }
            return data.Substring(num, num2);
        }
    }
}
