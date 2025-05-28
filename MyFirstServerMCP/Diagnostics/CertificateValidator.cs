using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace MyFirstServerMCP.Diagnostics
{
    /// <summary>
    /// Helper class to handle SSL certificate validation issues in development environments
    /// </summary>
    public static class CertificateValidator
    {
        /// <summary>
        /// Creates an HttpClientHandler that ignores SSL certificate validation errors
        /// Only use this in development environments, not in production!
        /// </summary>
        public static HttpClientHandler CreateInsecureHttpClientHandler()
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    // Always return true to accept all certificates
                    return true;
                }
            };
        }

        /// <summary>
        /// Gets the default HttpClient that bypasses SSL certificate validation
        /// </summary>
        public static HttpClient GetInsecureHttpClient()
        {
            return new HttpClient(CreateInsecureHttpClientHandler());
        }
    }
}
