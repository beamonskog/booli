using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public static class Hash
    {
        public static string Get(string callerId, long unixTime, string privateKey, string unique)
        {
            var stringToHash = callerId + unixTime.ToString() + privateKey + unique;
            var stringToHashInBytes = Encoding.ASCII.GetBytes(stringToHash);
            var hashedString = GetBytes(stringToHashInBytes);

            return hashedString;
        }

        private static string GetBytes(byte[] temp)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hashBytes = sha1.ComputeHash(temp);
                var hexString = BitConverter.ToString(hashBytes);
                hexString = hexString.Replace("-", "");
                return hexString;
            }
        }
    }
}
