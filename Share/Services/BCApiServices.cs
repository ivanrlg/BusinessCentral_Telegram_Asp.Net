using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Share.Models;
using Shared.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;


namespace Shared.Services
{
    public class BCApiServices
    {
        private static AuthenticationResult AuthResult = null;
        
        readonly ConfigurationsValues mConfigurationsValues;

        public BCApiServices(ConfigurationsValues configurationsValues)
        {
            this.mConfigurationsValues = configurationsValues;
        }

        public async Task<string> GetAccessToken()
        {
            string result = string.Empty;
            if ((AuthResult == null) || (AuthResult.ExpiresOn < DateTime.Now))
            {
                AuthResult = await GetAccessToken(mConfigurationsValues.Tenantid);
            }

            return AuthResult.AccessToken;
        }

        private async Task<AuthenticationResult> GetAccessToken(string aadTenantId)
        {
            Uri uri = new(mConfigurationsValues.Authority.Replace("{AadTenantId}", aadTenantId));
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(mConfigurationsValues.Clientid)
                .WithClientSecret(mConfigurationsValues.ClientSecret)
                .WithAuthority(uri)
                .Build();
            string[] scopes = new string[] { @"https://api.businesscentral.dynamics.com/.default" };
            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token acquired");
                Console.ResetColor();
            }
            catch (MsalServiceException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error occurred while retrieving access token");
                Console.WriteLine($"{ex.ErrorCode} {ex.Message}");
                Console.ResetColor();
            }
            return result;
        }

        public async Task<Response<object>> InsertDataJson(string model, string BCUrl)
        {
            string result = string.Empty;

            if ((AuthResult == null) || (AuthResult.ExpiresOn < DateTime.Now))
            {
                AuthResult = await GetAccessToken(mConfigurationsValues.Tenantid);
            }

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthResult.AccessToken);
                Uri uri = new(BCUrl);

                string request = model;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(request);
                Console.ResetColor();

                StringContent content = new(request, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(uri, content);

                result = await response.Content.ReadAsStringAsync();

                if ((response.StatusCode == HttpStatusCode.OK) || (response.StatusCode == HttpStatusCode.Created))
                {
                    return new Response<object>
                    {
                        IsSuccess = true,
                        Message = result
                    };
                }

                Console.ForegroundColor = ConsoleColor.Red;
                result = $"Call to Business Central API failed: {response.StatusCode} {response.ReasonPhrase}";
                Console.WriteLine(result = $"Call to Business Central API failed: {response.StatusCode} {response.ReasonPhrase}");
                Console.ResetColor();

            }

            return new Response<object>
            {
                IsSuccess = false,
                Message = result
            };
        }

        public async Task<Response<object>> GetDataFromBC(string BCUrl, RequestBC requestBC)
        {
            string result = string.Empty;
            if ((AuthResult == null) || (AuthResult.ExpiresOn < DateTime.Now))
            {
                AuthResult = await GetAccessToken(mConfigurationsValues.Tenantid);
            }

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthResult.AccessToken);

                Uri uri = new(BCUrl);

                string request = JsonConvert.SerializeObject(requestBC);

                StringContent content = new(request, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(uri, content);

                result = await response.Content.ReadAsStringAsync();

                if ((response.StatusCode == HttpStatusCode.OK) || (response.StatusCode == HttpStatusCode.Created))
                {
                    return new Response<object>
                    {
                        IsSuccess = true,
                        Message = result
                    };
                }

                Console.ForegroundColor = ConsoleColor.Red;
                result = $"Call to Business Central API failed: {response.StatusCode} {response.ReasonPhrase}";
                Console.WriteLine(result = $"Call to Business Central API failed: {response.StatusCode} {response.ReasonPhrase}");
                Console.ResetColor();
            }

            return new Response<object>
            {
                IsSuccess = false,
                Message = result
            };
        }
    }
}
