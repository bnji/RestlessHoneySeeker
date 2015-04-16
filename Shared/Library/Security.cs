using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Library
{
    public class Security
    {
        public static string SHA1(string str)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return Encoding.UTF8.GetString(sha1.ComputeHash(Encoding.UTF8.GetBytes(str)));
            }
        }

        public static string mt_rand_str(int amount, string chars = "abcdefghijklmnopqrstuvwxyz1234567890")
        {
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, amount)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }

    }
}
