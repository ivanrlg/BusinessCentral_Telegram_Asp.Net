namespace Shared.Models
{
    public class BotConfiguration
    {
        public string TelegramToken { get; init; } = default!;
        public string HostAddress { get; init; } = default!;
        public string Clientid { get; set; }
        public string Tenantid { get; set; }
        public string ClientSecret { get; set; }
        public string CompanyID { get; set; }
        public string EnvironmentName { get; set; }
    }
}
