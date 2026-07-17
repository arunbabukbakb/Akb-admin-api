using Microsoft.AspNetCore.Mvc;
using Data.Repository.IRepository;
using System;
using System.Threading.Tasks;

namespace adminapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public TestController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("error")]
        public IActionResult GetError()
        {
            throw new Exception("This is a simulated unhandled exception for testing Serilog and middleware.");
        }

        [HttpPost("send-to-roles")]
        public async Task<IActionResult> SendToRoles([FromQuery] string roles, [FromQuery] string title, [FromQuery] string body, [FromQuery] string? url = null, [FromQuery] string? type = null)
        {
            try
            {
                await _notificationService.SendNotificationToRolesAsync(roles, title, body, url, type);
                return Ok(new { success = true, message = $"Notification sent to roles: {roles}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser([FromQuery] int userId, [FromQuery] string title, [FromQuery] string body, [FromQuery] string? url = null, [FromQuery] string? type = null)
        {
            try
            {
                await _notificationService.SendNotificationToUserAsync(userId, title, body, url, type);
                return Ok(new { success = true, message = $"Notification sent to user ID: {userId}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }
}
