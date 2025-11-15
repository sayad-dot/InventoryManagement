using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InventoryManagement.Services
{
    public interface ISupportTicketService
    {
        Task<SupportTicket> CreateTicketAsync(string userId, string summary, string priority, string? inventoryTitle, string pageUrl);
        Task<string> GenerateTicketJsonAsync(SupportTicket ticket);
    }

    public class SupportTicketService : ISupportTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOneDriveService _oneDriveService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SupportTicketService> _logger;

        public SupportTicketService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IOneDriveService oneDriveService,
            IConfiguration configuration,
            ILogger<SupportTicketService> logger)
        {
            _context = context;
            _userManager = userManager;
            _oneDriveService = oneDriveService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<SupportTicket> CreateTicketAsync(
            string userId, 
            string summary, 
            string priority, 
            string? inventoryTitle, 
            string pageUrl)
        {
            try
            {
                var ticket = new SupportTicket
                {
                    UserId = userId,
                    Summary = summary,
                    Priority = priority,
                    InventoryTitle = inventoryTitle,
                    PageUrl = pageUrl,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pending"
                };

                _context.SupportTickets.Add(ticket);
                await _context.SaveChangesAsync();

                // Generate JSON and upload to OneDrive
                var jsonContent = await GenerateTicketJsonAsync(ticket);
                var fileName = $"ticket_{ticket.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
                
                var fileId = await _oneDriveService.UploadFileAsync(fileName, jsonContent);
                
                // Update ticket with file ID
                ticket.OneDriveFileId = fileId;
                ticket.Status = "Processed";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Support ticket {ticket.Id} created and uploaded successfully");
                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support ticket");
                
                // Update status to Failed if ticket was created
                var ticket = await _context.SupportTickets
                    .FirstOrDefaultAsync(t => t.UserId == userId && t.Summary == summary);
                
                if (ticket != null)
                {
                    ticket.Status = "Failed";
                    await _context.SaveChangesAsync();
                }
                
                throw;
            }
        }

        public async Task<string> GenerateTicketJsonAsync(SupportTicket ticket)
        {
            // Get user details
            var user = await _userManager.FindByIdAsync(ticket.UserId);
            
            // Get admin emails
            var adminEmails = await GetAdminEmailsAsync();

            var ticketData = new
            {
                ticketId = ticket.Id,
                reportedBy = user?.FullName ?? "Unknown User",
                reportedByEmail = user?.Email ?? "unknown@email.com",
                inventory = ticket.InventoryTitle ?? "N/A",
                link = ticket.PageUrl ?? "N/A",
                summary = ticket.Summary,
                priority = ticket.Priority,
                createdAt = ticket.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                adminEmails = adminEmails
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(ticketData, options);
        }

        private async Task<List<string>> GetAdminEmailsAsync()
        {
            try
            {
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                
                if (adminRole == null)
                {
                    _logger.LogWarning("Admin role not found");
                    return new List<string>();
                }

                var adminUserIds = await _context.UserRoles
                    .Where(ur => ur.RoleId == adminRole.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                var adminEmails = await _context.Users
                    .Where(u => adminUserIds.Contains(u.Id))
                    .Select(u => u.Email)
                    .Where(email => email != null)
                    .ToListAsync();

                return adminEmails.Select(e => e!).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin emails");
                return new List<string>();
            }
        }
    }
}
