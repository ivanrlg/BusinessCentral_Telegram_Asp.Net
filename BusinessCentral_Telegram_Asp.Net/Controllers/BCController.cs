using BusinessCentral_Telegram_Asp.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Telegram.Bot.Types;

namespace BusinessCentral_Telegram_Asp.Net.Controllers
{
    [Route("api/[Controller]")]
    public class BCController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Webhook([FromServices] BCServices _BCServices, [FromBody] Update _update)
        {
            Response<string> response = await _BCServices.ConnectToBC(_update);

            if (response.IsSuccess)
                return Ok();
            else
                return BadRequest(response.Message);
        }
    }
}
