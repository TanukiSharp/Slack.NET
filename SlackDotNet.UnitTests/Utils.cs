using System;
using System.Collections.Generic;
using System.Text;

namespace SlackDotNet.UnitTests
{
    public static class Utils
    {
        public static bool GetValueInUrlEncode(string urlEncode, string key, out string value)
        {
            if (string.IsNullOrWhiteSpace(urlEncode))
                throw new ArgumentException($"Invalid '{nameof(urlEncode)}' argument.", nameof(urlEncode));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"Invalid '{nameof(key)}' argument.", nameof(key));

            int start = urlEncode.IndexOf(key);
            if (start < 0)
            {
                value = null;
                return false;
            }

            start = urlEncode.IndexOf('=', start + key.Length);
            if (start < 0)
            {
                value = null;
                return true;
            }

            start++;

            int end = urlEncode.IndexOf('&', start);

            if (end < 0)
                end = urlEncode.Length;

            value = urlEncode.Substring(start, end - start);

            return true;
        }
    }
}
