using BusinessCentral_Telegram_Asp.Services;
using Shared.Models;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Added IvanSinglenton.devs
var botConfig = builder.Configuration.GetSection("ConfigurationsValues").Get<ConfigurationsValues>();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHostedService<ConfigureWebhook>();
builder.Services.AddHttpClient("tgwebhook").AddTypedClient<ITelegramBotClient>
    (httpClient => new TelegramBotClient(botConfig.TelegramToken, httpClient));
builder.Services.AddScoped<BCServices>();
builder.Services.AddCors();
//Added IvanSinglenton.dev

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();
//app.MapControllers();

//Added IL
app.UseRouting();
app.UseCors();

app.UseEndpoints(endpoints =>
{
    var token = botConfig.TelegramToken;
    endpoints.MapControllerRoute(name: "tgwebhook",
                                 pattern: $"bot/{token}",
                                 new { controller = "api/BC", action = "Post" });
    endpoints.MapControllers();
});
//Added IL

app.Run();
