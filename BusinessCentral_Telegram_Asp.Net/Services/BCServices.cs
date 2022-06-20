using Newtonsoft.Json;
using Share.Models;
using Shared.Models;
using Shared.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBridgeAF.Models;

namespace BusinessCentral_Telegram_Asp.Services
{
    public class BCServices
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<BCServices> _logger;
        private readonly ConfigurationsValues _botConfig;
        
        public BCServices(
            ITelegramBotClient botClient, 
            ILogger<BCServices> logger,        
            IConfiguration configuration)
        {
            _botClient = botClient;
            _logger = logger;
            _botConfig = configuration.GetSection("ConfigurationsValues").Get<ConfigurationsValues>();
        }

        public async Task<Response<string>> ConnectToBC(Update update)
        {
            _logger.LogInformation("ConnectToBC: Receive message type: {MessageType}", update.Type);            
            
            if (_botConfig.TelegramToken == null)
            {
                return new Response<string>()
                {
                    IsSuccess = false,
                    Message = "Please set the TelegramToken."
                };
            }

            if (update.Type == UpdateType.Message)
            {
                if (update.Message.Entities != null)
                {
                    if (update.Message.Entities[0].Type == MessageEntityType.BotCommand)
                    {
                        string[] Parameters = update.Message.Text.Split(" ");

                        TelegamCommand request;

                        if (Parameters.Length == 1)
                        {
                            request = new()
                            {
                                CommandName = Parameters[0],
                                Parameter = null
                            };

                            await _botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Command: {request.CommandName} \n"
                                                                                    + $"Parameter: Empty");
                        }
                        else if (Parameters.Length >= 2)
                        {
                            request = new()
                            {
                                CommandName = Parameters[0],
                                Parameter = Parameters[1]
                            };

                            BCApiServices apiServices = new(_botConfig);

                            switch (request.CommandName)
                            {
                                case
                                  "/getitem":
                                    return await GetItem(_botConfig, _botClient, update, request, apiServices);
                                case
                                  "/getcustomer":
                                    break;
                                case
                                   "/salesorders":
                                    break;
                                default:
                                    request.CommandName = "Unknow";
                                    break;
                            }

                            await _botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Command: {request.CommandName} \n"
                                                                                    + $"Parameter: {request.Parameter}");
                        }

                        return new Response<string>()
                        {
                            IsSuccess = true,
                            Message = "Command executed successfully."
                        };
                    }
                }


                await _botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Echo Message: {update.Message.Text}");
            }

            return new Response<string>()
            {
                IsSuccess = true,
                Message = "Command executed successfully."
            };
        }

        private async Task<Response<string>> GetItem(
        ConfigurationsValues configValues,
        ITelegramBotClient _botClient,
        Update Rec,
        TelegamCommand request,
        BCApiServices apiServices)
        {
            _logger.LogInformation("GetItem: Receive message type: {MessageType}", Rec.Type);            
            
            try
            {
                RequestBC requestBC = CreateItemRequest(request);

                Response<object> Response = await apiServices.GetDataFromBC(configValues.GetItemsToJson, requestBC);

                if (Response.IsSuccess)
                {
                    var ResultBC = JsonConvert.DeserializeObject<Ouput>(Response.Message);

                    var ItemBC = JsonConvert.DeserializeObject<Item>(ResultBC.value);

                    if (string.IsNullOrEmpty(ItemBC.No))
                    {
                        var ErrorMessage = JsonConvert.DeserializeObject<ErrorModel>(ResultBC.value);

                        await _botClient.SendTextMessageAsync(Rec.Message.Chat.Id, $"Error: {ErrorMessage.Error}");
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(Rec.Message.Chat.Id,
                                                                $"Item No: {ItemBC.No} \n" +
                                                                $"Description: {ItemBC.Description} \n" +
                                                                $"UnitPrice: {ItemBC.UnitPrice} \n" +
                                                                $"UnitOfMeasure: {ItemBC.UnitOfMeasure}");
                    }
                }
                else
                {
                    await _botClient.SendTextMessageAsync(Rec.Message.Chat.Id, $"Error getting the data {Response.Message}");
                }

                return new Response<string>()
                {
                    IsSuccess = true,
                    Message = "Command executed successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogInformation("GetItem: Exception message type: {Message}", ex.Message);
                
                await _botClient.SendTextMessageAsync(Rec.Message.Chat.Id, $"Error getting the data {ex.Message}");

                return new Response<string>()
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        private RequestBC CreateItemRequest(TelegamCommand request)
        {
            _logger.LogInformation("CreateItemRequest: ItemNo: {Message}", request.Parameter);
            
            Item ItemRequest = new()
            {
                No = request.Parameter
            };

            string body = JsonConvert.SerializeObject(ItemRequest);

            RequestBC requestBC = new()
            {
                jsontext = body
            };

            return requestBC;
        }
    }
}
