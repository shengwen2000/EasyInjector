using System.Collections.Specialized;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HawkNet.AspNetCore
{
    /// <summary>
    /// 參考 https://github.com/pcibraro/hawknet 將Hawk 核心程式碼移轉過來
    /// (主要是原作者沒有繼續在維護的樣子)
    /// Hawk main class. It provides methods for generating a Hawk authorization header on the client side and authenticate it on the
    /// service side.
    /// </summary>
    public static class Hawk
    {
        readonly static string[] RequiredAttributes = ["id", "ts", "mac", "nonce"];
        readonly static string[] OptionalAttributes = ["ext", "hash"];
        readonly static string[] SupportedAttributes;
        readonly static string[] SupportedAlgorithms = ["sha1", "sha256"];

        readonly static string RandomSource = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        static Hawk()
        {
            SupportedAttributes = [.. RequiredAttributes, .. OptionalAttributes];
        }

        /// <summary>
        /// 服務器端驗證 Request
        /// </summary>
        /// <param name="authorization">Authorization header</param>
        /// <param name="method">Request method</param>
        /// <param name="uri">Request Uri</param>
        /// <param name="getCredentialFunc">A method for searching across the available credentials</param>
        /// <param name="timestampSkewSec">Accepted Time skew for timestamp verification</param>
        /// <param name="requestPayload"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        public static async Task<ClaimsPrincipal> AuthenticateAsync(
            string authorization,
            string method,
            Uri uri,
            Func<string, Task<HawkCredential?>> getCredentialFunc,
            int timestampSkewSec = 60,
            Func<Task<string>>? requestPayload = null,
            string? mediaType = null)
        {
            if (string.IsNullOrEmpty(authorization))
                throw new ArgumentException("Authorization parameter can not be null or empty", nameof(authorization));

            var attributes = ParseAttributes(authorization);

            ValidateAttributes(timestampSkewSec, attributes);

            // 取得憑證
            var credential = await getCredentialFunc(attributes["id"]!) ??
                throw new SecurityException($"找不到HAWK證書 {attributes["id"]}");

            // 驗證憑證屬性基本
            ValidateCredentials(credential);

            // 有內容hash的話 必續驗證hash
            if (!string.IsNullOrEmpty(attributes["hash"]))
            {
                if (requestPayload == null)
                    throw new ArgumentException("requestPayload can not be null or empty", nameof(requestPayload));

                if (string.IsNullOrEmpty(mediaType))
                    throw new ArgumentException("MediaType can not be null or empty", nameof(mediaType));

                var hash = CalculatePayloadHash(await requestPayload(), mediaType, credential);

                if (attributes["hash"] != hash)
                    throw new SecurityException("Bad payload hash");
            }

            var mac = CalculateMac(
                method,
                uri,
                attributes["ext"],
                attributes["ts"]!,
                attributes["nonce"]!,
                credential, "header",
                attributes["hash"]);

            if (!IsEqual(mac, attributes["mac"]!)) throw new SecurityException("Bad mac");

             var identity = new ClaimsIdentity(authenticationType: "Hawk", nameType: "userId", roleType: "role");
             identity.AddClaim(new Claim("userId", credential.User));
             foreach(var role in credential.Roles.Distinct())
                identity.AddClaim(new Claim("role", role));
            var principal = new ClaimsPrincipal(identity);
            return principal;
        }


        /// <summary>
        /// 用戶端產生Header
        /// </summary>
        /// <param name="method">Request method</param>
        /// <param name="uri">Request uri</param>
        /// <param name="credential">Credential used to calculate the MAC</param>
        /// <param name="ext">Optional extension attribute</param>
        /// <param name="ts">Timestamp</param>
        /// <param name="nonce">Random Nonce</param>
        /// <param name="payloadHash">Hash of the request payload</param>
        /// <param name="type">Type used as header for the normalized string. Default value is 'header'</param>
        /// <returns>Hawk authorization header</returns>
        public static string GetAuthorizationHeader(
            string method,
            Uri uri,
            HawkCredential credential,
            string? ext = null,
            DateTime? ts = null,
            string? nonce = null,
            string? payloadHash = null,
            string? type = null)
        {

            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("The method can not be null or empty", nameof(method));

            if (credential == null)
                throw new ArgumentNullException(nameof(credential), "The credential can not be null");

            if (string.IsNullOrEmpty(nonce))
                nonce = GetRandomString(6);

            if (string.IsNullOrEmpty(type))
                type = "header";

            var normalizedTs = ((int)Math.Floor(ConvertToUnixTimestamp(ts ?? DateTime.UtcNow))).ToString();

            var mac = CalculateMac(
                method,
                uri,
                ext,
                normalizedTs,
                nonce,
                credential,
                type,
                payloadHash);

            var authorization = string.Format("id=\"{0}\", ts=\"{1}\", nonce=\"{2}\", mac=\"{3}\", ext=\"{4}\"",
                credential.Id, normalizedTs, nonce, mac, ext);

            if (!string.IsNullOrEmpty(payloadHash))
                authorization += string.Format(", hash=\"{0}\"", payloadHash);

            return authorization;
        }

        /// <summary>
        /// Gets a random string of a given size
        /// </summary>
        /// <param name="size">Expected size for the generated string</param>
        /// <returns>Random string</returns>
        public static string GetRandomString(int size)
        {
            var result = new StringBuilder();
            var random = new Random();

            for (var i = 0; i < size; ++i)
                result.Append(RandomSource[random.Next(RandomSource.Length)]);

            return result.ToString();
        }

        /// <summary>
        /// Parse all the attributes present in the Hawk authorization header
        /// </summary>
        /// <param name="authorization">Authorization header</param>
        /// <returns>List of parsed attributes</returns>
        public static NameValueCollection ParseAttributes(string authorization)
        {
            var allAttributes = new NameValueCollection();

            foreach (var attribute in authorization.Split(','))
            {
                var index = attribute.IndexOf('=');
                if (index > 0)
                {
                    var key = attribute[..index].Trim();
                    var value = attribute[(index + 1)..].Trim();

                    if (value.StartsWith("\""))
                        value = value[1..^1];

                    allAttributes.Add(key, value);
                }
            }

            return allAttributes;
        }

        /// <summary>
        /// Computes a mac following the Hawk rules
        /// </summary>
        /// <param name="method">Request method</param>
        /// <param name="uri">Request uri</param>
        /// <param name="ext">Extesion attribute</param>
        /// <param name="ts">Timestamp</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="credential">Credential</param>
        /// <param name="type">Credential</param>///
        /// <param name="payloadHash">Hash of the request payload</param>
        /// <returns>Generated mac</returns>
        public static string CalculateMac(
            string method,
            Uri uri, string? ext,
            string ts,
            string nonce,
            HawkCredential credential,
            string type, string? payloadHash = null)
        {
            var hmac = GetHMAC(credential.Algorithm);

            hmac.Key = Encoding.UTF8.GetBytes(credential.Key);

            var host = uri.Host;
            var sanitizedHost = (host.IndexOf(':') > 0) ?
                host[..host.IndexOf(':')] :
                host;

            var normalized = "hawk.1." + type + "\n" +
                        ts + "\n" +
                        nonce + "\n" +
                        method.ToUpper() + "\n" +
                        uri.PathAndQuery + "\n" +
                        sanitizedHost + "\n" +
                        uri.Port.ToString() + "\n" +
                        ((!string.IsNullOrEmpty(payloadHash)) ? payloadHash : "") + "\n" +
                        ((!string.IsNullOrEmpty(ext)) ? ext : "") + "\n";

            var messageBytes = Encoding.UTF8.GetBytes(normalized);
            var mac = hmac.ComputeHash(messageBytes);
            var encodedMac = Convert.ToBase64String(mac);
            return encodedMac;
        }

        static HMAC GetHMAC(string algorithm)
        {
            if (algorithm.Equals("sha1", StringComparison.InvariantCultureIgnoreCase))
                return new HMACSHA1();
            else if (algorithm.Equals("sha256", StringComparison.InvariantCultureIgnoreCase))
                return new HMACSHA256();
            else
                throw new Exception("Not supported algorithm");

        }

        /// <summary>
        /// Converts a Datatime to an equivalent Unix Timestamp, in seconds
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static double ConvertToUnixTimestamp(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        /// <summary>
        /// Generates a mac hash using the supplied payload and credentials
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="credential"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        public static string CalculatePayloadHash(string payload, string mediaType, HawkCredential credential)
        {
            var normalized = "hawk.1.payload\n" +
                mediaType + "\n" +
                payload + "\n";

            var hmac = GetHMAC(credential.Algorithm);
            var encodedMac = Convert.ToBase64String(hmac
                .ComputeHash(Encoding.UTF8.GetBytes(normalized)));
            return encodedMac;
        }

        private static bool CheckTimestamp(string ts, int timestampSkewSec)
        {
            if (double.TryParse(ts, out double parsedTs))
            {
                var now = ConvertToUnixTimestamp(DateTime.Now);
                var result = Math.Abs(parsedTs - now);

                // Check timestamp staleness
                if (result > timestampSkewSec)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        private static void ValidateAttributes(int timestampSkewSec, NameValueCollection attributes)
        {
            // 必要的屬性檢查
            if (!RequiredAttributes.All(a => attributes.AllKeys.Any(k => k == a)))
                throw new SecurityException("Missing attributes");

            // 有沒有未知的屬性
            if (!attributes.AllKeys.All(a => SupportedAttributes.Any(k => k == a)))
                throw new SecurityException("Unknown attributes");

            // 時間戳記檢查
            if (!CheckTimestamp(attributes["ts"]!, timestampSkewSec))
                throw new SecurityException("Stale timestamp");
        }

        private static void ValidateCredentials(HawkCredential credential)
        {
            if (string.IsNullOrEmpty(credential.Algorithm) ||
                string.IsNullOrEmpty(credential.Key))
                throw new SecurityException("Invalid credentials");

            if (!SupportedAlgorithms.Any(a =>
                string.Equals(a, credential.Algorithm, StringComparison.InvariantCultureIgnoreCase)))
                throw new SecurityException("Unknown algorithm");
        }

        // Fixed time comparision
        private static bool IsEqual(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}
