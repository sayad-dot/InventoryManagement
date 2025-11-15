using InventoryManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly ISupportTicketService _supportTicketService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SupportController> _logger;

        public SupportController(
            ISupportTicketService supportTicketService,
            UserManager<ApplicationUser> userManager,
            ILogger<SupportController> logger)
        {
            _supportTicketService = supportTicketService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket(
            [FromForm] string summary,
            [FromForm] string priority,
            [FromForm] string? inventoryTitle,
            [FromForm] string pageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(summary))
                {
                    return Json(new { success = false, message = "Summary is required" });
                }

                if (string.IsNullOrWhiteSpace(priority) || 
                    !new[] { "High", "Average", "Low" }.Contains(priority))
                {
                    return Json(new { success = false, message = "Valid priority is required (High, Average, or Low)" });
                }

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var ticket = await _supportTicketService.CreateTicketAsync(
                    userId, 
                    summary, 
                    priority, 
                    inventoryTitle, 
                    pageUrl);

                _logger.LogInformation($"Support ticket {ticket.Id} created successfully by user {userId}");

                return Json(new 
                { 
                    success = true, 
                    message = "Support ticket created successfully! Our team will review it shortly.",
                    ticketId = ticket.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support ticket");
                return Json(new 
                { 
                    success = false, 
                    message = "An error occurred while creating the support ticket. Please try again." 
                });
            }
        }
    }
}
