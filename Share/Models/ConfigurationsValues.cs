namespace Shared.Models
{
    public class ConfigurationsValues
    {
        public string ClientSecret { get; set; } = default!;
        public string HostAddress { get; init; } = default!;
        public string Clientid { get; set; }
        public string Tenantid { get; set; }
        public string CompanyID { get; set; }
        public string TelegramToken { get; set; }
        public string EnvironmentName { get; set; }

        public string Authority => $"https://login.microsoftonline.com/{Tenantid}/oauth2/v2.0/token";

        public string UrlBCSandbox => $"https://api.businesscentral.dynamics.com/v2.0/{Tenantid}/{EnvironmentName}/ODataV4/";
        
        public string GetItemsToJson => GetBCAPIUrl(UrlBCSandbox, "WebServices_GetItemsToJson", CompanyID);
        
        public string Ping => GetBCAPIUrl(UrlBCSandbox, "WebServices_Ping", CompanyID);

        private static string GetBCAPIUrl(string BCBaseUrl, string Action, string bcCompanyId)
        {
            return BCBaseUrl + $"{Action}?company=({bcCompanyId})";
        }
    }
}
