using System;
using System.Collections.Generic;
using System.Text;

namespace Dams.ms.auth.Models
{
    public class TwitterParams
    {
        public string OAuthConsumerKey { get; set; } = "";
        public string OAuthNonce { get; set; } = "";
        public string OAuthToken { get; set; } = "";
        public string OAuthTimestamp { get; set; } = "";
        public string OAuthSignature { get; set; } = "";
        public string OAuthVersion { get; set; } = "1.0";
        public string OAuthSignatureMethod { get; set; } = "HMAC-SHA1";
    }
}
