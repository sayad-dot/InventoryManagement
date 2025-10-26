using Microsoft.AspNetCore.SignalR;
using InventoryManagement.Models;
using InventoryManagement.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Hubs
{
    public class DiscussionHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscussionHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task JoinDiscussionGroup(int inventoryId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"inventory-{inventoryId}");
        }

        public async Task LeaveDiscussionGroup(int inventoryId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"inventory-{inventoryId}");
        }

        public async Task SendMessage(int inventoryId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (Context.User == null)
                return;

            var user = await _userManager.GetUserAsync(Context.User);
            if (user == null)
                return;

            // Create discussion post
            var discussion = new Discussion
            {
                InventoryId = inventoryId,
                UserId = user.Id,
                Message = message.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Discussions.Add(discussion);
            await _context.SaveChangesAsync();

            // Load user details for the response
            var userDetails = await _userManager.FindByIdAsync(user.Id);

            // Notify all clients in the group
            await Clients.Group($"inventory-{inventoryId}").SendAsync("ReceiveMessage", new
            {
                Id = discussion.Id,
                UserId = user.Id,
                UserName = userDetails?.FullName ?? user.UserName,
                Message = discussion.Message,
                CreatedAt = discussion.CreatedAt,
                CreatedAtDisplay = discussion.CreatedAt.ToString("MMM dd, yyyy HH:mm"),
                CanEdit = true // Current user can edit their own messages
            });
        }

        public async Task DeleteMessage(int messageId)
        {
            if (Context.User == null)
                return;

            var user = await _userManager.GetUserAsync(Context.User);
            if (user == null)
                return;

            var message = await _context.Discussions
                .Include(d => d.Inventory)
                .FirstOrDefaultAsync(d => d.Id == messageId);

            if (message == null)
                return;

            // Check if user can delete (owner of message or owner of inventory or admin)
            if (message.UserId != user.Id && message.Inventory.CreatorId != user.Id)
                return;

            _context.Discussions.Remove(message);
            await _context.SaveChangesAsync();

            // Notify all clients in the group
            await Clients.Group($"inventory-{message.InventoryId}").SendAsync("MessageDeleted", messageId);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}