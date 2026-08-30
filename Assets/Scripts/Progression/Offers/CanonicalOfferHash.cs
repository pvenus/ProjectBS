using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Progression
{
    internal static class CanonicalOfferHash
    {
        public static byte[] Compute(params string[] fields)
        {
            using SHA256 sha = SHA256.Create();
            return sha.ComputeHash(Encode(fields));
        }

        public static string ComputeHex(IEnumerable<string> fields)
        {
            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encode(fields));
            StringBuilder builder = new(hash.Length * 2);
            foreach (byte value in hash)
            {
                builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static byte[] Encode(IEnumerable<string> fields)
        {
            StringBuilder builder = new();
            foreach (string field in fields)
            {
                string value = field ?? string.Empty;
                int byteCount = Encoding.UTF8.GetByteCount(value);
                builder.Append(byteCount.ToString(CultureInfo.InvariantCulture));
                builder.Append(':');
                builder.Append(value);
                builder.Append('|');
            }

            return Encoding.UTF8.GetBytes(builder.ToString());
        }
    }
}
