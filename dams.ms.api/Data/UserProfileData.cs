using Dams.ms.auth.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Dams.ms.auth.Data
{
    public class UserProfileData
    {
        #region Private Varibles
        public readonly IConfigurationRoot ObjConfiguration;
        private const string OAuthVersion = "1.0";
        private const string OAuthSignatureMethod = "HMAC-SHA1";
        #endregion

        #region Public Variables
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "given_name")]
        public string GivenName { get; set; }

        [JsonProperty(PropertyName = "family_name")]
        public string FamilyName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        #endregion

        #region Constructors
        public UserProfileData()
        {
        }

        public UserProfileData(IConfigurationRoot Configuration)
        {
            ObjConfiguration = Configuration;
        }
        #endregion

        #region Web Request
        public UserProfileData WebRequest(string Url, string token, string header)
        {
            var webRequest = System.Net.WebRequest.Create(Url);
            webRequest.Method = "GET";
            webRequest.Timeout = 20000;
            webRequest.ContentType = "application/json";
            if (header != null) webRequest.Headers.Add(header, token);
            using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                {
                    JsonSerializerSettings serSettings = new JsonSerializerSettings();
                    serSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    return JsonConvert.DeserializeObject<UserProfileData>(sr.ReadToEnd(), serSettings);
                }
            }
        }
        #endregion

        #region Twitter Web Request
        public UserProfileData TwitterWebRequest(string Url, string token, string secret, string header)
        {
            SortedDictionary<string, string> requestParameters = new SortedDictionary<string, string>();
            TwitterParams twitterParams = new TwitterParams();
            twitterParams = getTwitterParameters(Url, token, secret);
            requestParameters.Add("oauth_consumer_key", twitterParams.OAuthConsumerKey);
            requestParameters.Add("oauth_nonce", twitterParams.OAuthNonce);
            requestParameters.Add("oauth_signature_method", OAuthSignatureMethod);
            requestParameters.Add("oauth_token", token);
            requestParameters.Add("oauth_timestamp", twitterParams.OAuthTimestamp);
            requestParameters.Add("oauth_version", OAuthVersion);
            requestParameters.Add("oauth_signature", twitterParams.OAuthSignature);
            Url = Url + "?" + requestParameters.ToWebString();
            var webRequest = System.Net.WebRequest.Create(Url);
            webRequest.Method = "GET";
            webRequest.Timeout = 20000;
            webRequest.ContentType = "application/json";
            if (header != null) webRequest.Headers.Add(header, token);
            try
            {
                var response = webRequest.GetResponse();
                using (System.IO.Stream s = response.GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        JsonSerializerSettings serSettings = new JsonSerializerSettings();
                        serSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                        return JsonConvert.DeserializeObject<UserProfileData>(sr.ReadToEnd(), serSettings);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error is {0} ", ex.Message);
                return null;
            }
        }
        #endregion

        #region Twitter Web Request Helper Methods

        private static string CreateOauthNonce()
        {
            return Convert.ToBase64String(new System.Text.ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        }

        private static string CreateOAuthTimestamp()
        {

            var nowUtc = DateTime.UtcNow;
            var timeSpan = nowUtc - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

            return timestamp;
        }

        private string CreateOauthSignature(string resourceUrl, Method method, string oauthNonce, string oauthTimestamp, string AccessToken, string AccessTokenSecret)
        {
            string ConsumerKey = ObjConfiguration.GetSection("Twitter:ConsumerKey").Value;
            string ConsumerKeySecret = ObjConfiguration.GetSection("Twitter:ConsumerKeySecret").Value;
            //firstly we need to add the standard oauth parameters to the sorted list
            SortedDictionary<string, string> requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("oauth_consumer_key", ConsumerKey);
            requestParameters.Add("oauth_nonce", oauthNonce);
            requestParameters.Add("oauth_signature_method", OAuthSignatureMethod);
            requestParameters.Add("oauth_timestamp", oauthTimestamp);
            requestParameters.Add("oauth_token", AccessToken);
            requestParameters.Add("oauth_version", OAuthVersion);

            var sigBaseString = requestParameters.ToWebString();

            var signatureBaseString = string.Concat
            (method.ToString(), "&", Uri.EscapeDataString(resourceUrl), "&",
                                Uri.EscapeDataString(sigBaseString.ToString()));

            //Using this base string, we then encrypt the data using a composite of the 
            //secret keys and the HMAC-SHA1 algorithm.
            var compositeKey = string.Concat(Uri.EscapeDataString(ConsumerKeySecret), "&",
                                             Uri.EscapeDataString(AccessTokenSecret));

            string oauthSignature;
            using (var hasher = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(compositeKey)))
            {
                oauthSignature = Convert.ToBase64String(
                    hasher.ComputeHash(System.Text.Encoding.ASCII.GetBytes(signatureBaseString)));
            }

            return oauthSignature;
        }

        private TwitterParams getTwitterParameters(string url, string token, string secret)
        {
            TwitterParams twitterParams = new TwitterParams();
            twitterParams.OAuthNonce = CreateOauthNonce();
            twitterParams.OAuthTimestamp = CreateOAuthTimestamp();
            twitterParams.OAuthToken = token;
            twitterParams.OAuthConsumerKey = ObjConfiguration.GetSection("Twitter:ConsumerKey").Value;
            twitterParams.OAuthSignature = CreateOauthSignature(url, Method.GET, twitterParams.OAuthNonce, twitterParams.OAuthTimestamp, token, secret);
            return twitterParams;
        }

        #endregion

        #region Method Enum
        public enum Method
        {
            POST,
            GET
        }
        #endregion
    }
}
