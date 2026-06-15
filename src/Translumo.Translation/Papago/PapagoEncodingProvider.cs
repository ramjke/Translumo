using System;
using System.Security.Cryptography;
using System.Text;
using Translumo.Infrastructure.Constants;

namespace Translumo.Translation.Papago
{
    public static class PapagoEncodingProvider
    {
        private static readonly Random _randomizer = new Random();

        public static string GetGuid()
        {
            var datetimeMs = GetTimeMs();

            return RegexStorage.GuidGenerationRegex.Replace("xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx", match =>
            {
                double rand = _randomizer.NextDouble();
                var t = (long)((datetimeMs + 16 * rand) % 16);
                datetimeMs = (long)Math.Floor((double)datetimeMs / 16);

                return (match.Value == "x" ? t : 3 & t | 8).ToString("x");
            });
        }

        public static long GetTimestamp()
        {
            return GetTimeMs() - _randomizer.Next(500, 1500);
        }

        public static string GetHmacPostfix(string guid, string timestamp, string authKey)
        {
            var hmakInput = GetHmakInput(guid, timestamp);
            byte[] bytes = Encoding.UTF8.GetBytes(hmakInput);
            using HMACMD5 hMACMD = new HMACMD5(Encoding.UTF8.GetBytes(authKey));
            return Convert.ToBase64String(hMACMD.ComputeHash(bytes));
        }

        private static string GetHmakInput(string guid, string guidTime)
        {
            return guid + "\n" + "https://papago.naver.com/apis/n2mt/translate" + "\n" + guidTime;
        }

        private static long GetTimeMs()
        {
            var time = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1));
            return (long)(time.TotalMilliseconds + 0.5);
        }

    }
}
