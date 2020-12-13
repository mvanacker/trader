using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using ServiceStack.Text;
using MathNet.Numerics.Distributions;
using System.Globalization;

namespace Trader
{
    public static class Utils
    {
        #region Time
        public static DateTime Epoch => new DateTime(1970, 1, 1);
        public static long SinceEpoch(DateTime dt)
        {
            return (long)Math.Round((dt - Epoch).TotalSeconds);
        }
        public static long UtcTime => SinceEpoch(DateTime.UtcNow);
        public static long SinceEpochMs(DateTime dt)
        {
            return (long)Math.Round((dt - Epoch).TotalMilliseconds);
        }
        public static long UtcTimeMs => SinceEpochMs(DateTime.UtcNow);
        private const long _maxNonce = 9007199254740992;
        public static long Nonce
        {
            get
            {
                var rawNonce = DateTime.UtcNow.Ticks - new DateTime(1990, 1, 1).Ticks;
                return rawNonce % _maxNonce;
            }
        }
        #endregion

        #region Encryption
        public static string Hmacsha256(byte[] key, byte[] message)
        {
            using (var hash = new HMACSHA256(key))
            {
                return hash.ComputeHash(message).HexDigest();
            }
        }
        public static string Hmacsha256(string key, string message)
        {
            return Hmacsha256(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(message));
        }
        public static string HexDigest(this byte[] bytes)
        {
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.Append($"{b:x2}");
            return hex.ToString();
        }
        #endregion

        #region Encoding
        public static string BuildQuery(IDictionary<string, object> param)
        {
            if (param == null) return string.Empty;
            var pairs = new List<string>();
            foreach (var item in param)
            {
                pairs.Add($"{item.Key}={WebUtility.UrlEncode(item.Value.ToString())}");
            }
            return string.Join("&", pairs);
        }
        public static string BuildJSON(IDictionary<string, object> param)
        {
            if (param == null) return "";
            var entries = new List<string>();
            foreach (var item in param)
            {
                switch (item.Value)
                {
                    case string s:
                        if (string.IsNullOrEmpty(s))
                        {
                            entries.Add($"\"{item.Key}\"");
                        }
                        else
                        {
                            entries.Add($"\"{item.Key}\":\"{item.Value}\"");
                        }
                        break;
                    case Double d:
                        entries.Add($"\"{item.Key}\":{d}");
                        break;
                    case DateTime t:
                        entries.Add($"\"{item.Key}\":{t.ToString(Const.DATETIME_FORMAT)}");
                        break;
                    case null:
                        entries.Add($"\"{item.Key}\"");
                        break;
                    default:
                        entries.Add($"\"{item.Key}\":\"{item.Value}\"");
                        break;
                }
            }
            return $"{{{string.Join(",", entries)}}}";
        }
        public static Json ToJson(this string s)
        {
            return JsonSerializer.DeserializeFromString<Json>(s);
        }
        public static T ToJson<T>(this string s)
        {
            return JsonSerializer.DeserializeFromString<T>(s);
        }
        public static List<Json> ToJsonList(this string s)
        {
            return JsonSerializer.DeserializeFromString<List<Json>>(s);
        }
        public static List<T> ToJsonList<T>(this string s)
        {
            return JsonSerializer.DeserializeFromString<List<T>>(s);
        }
        public static string EncodeData(this string s, string table, string action = "insert")
        {
            return $"{{\"table\":\"{table}\",\"action\":\"{action}\",\"data\":{s}}}";
        }
        #endregion

        public static void DumpProperties(this object obj, bool dumpInput = false)
        {
            if (obj == null) throw new ArgumentNullException();
            if (dumpInput) Console.WriteLine(obj.ToString());
            var props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                var value = prop.GetValue(obj);
                if (value == null) continue;
                if (value is string s && string.IsNullOrEmpty(s)) continue;
                Console.WriteLine($"{prop.Name}: {value}");
            }
        }
        public static void DumpProperties<T>(this IEnumerable<T> collection, bool dumpInput = false)
        {
            foreach (var item in collection) item.DumpProperties(dumpInput);
        }

        public static double RoundToPip(this double d, double pip)
        {
            return Math.Round(d / pip) * pip;
        }

        public static string ToTitleCase(this string s)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
        }
        public static string Capitalize(this string s)
        {
            return $"{s[0].ToString().ToUpper()}{s.Substring(1)}";
        }

        public static string ToSatoshi(this double d)
        {
            return d.ToString("#,##0");
        }
        public static string ToSatoshi(this long l) => ((double)l).ToSatoshi();
    }
}
