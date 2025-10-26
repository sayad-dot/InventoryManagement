using InventoryManagement.Data;
using InventoryManagement.Models;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Services
{
    public interface IDiscussionService
    {
        Task<List<DiscussionViewModel>> GetDiscussionsAsync(int inventoryId, string currentUserId);
        Task<DiscussionViewModel> CreateDiscussionAsync(int inventoryId, string message, string userId);
        Task<bool> DeleteDiscussionAsync(int discussionId, string userId);
        Task<bool> CanEditDiscussionAsync(int discussionId, string userId);
    }

    public class DiscussionService : IDiscussionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscussionService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<DiscussionViewModel>> GetDiscussionsAsync(int inventoryId, string currentUserId)
        {
            var discussions = await _context.Discussions
                .Where(d => d.InventoryId == inventoryId)
                .OrderBy(d => d.CreatedAt)
                .Include(d => d.User)
                .Select(d => new DiscussionViewModel
                {
                    Id = d.Id,
                    InventoryId = d.InventoryId,
                    UserId = d.UserId,
                    UserName = d.User.FullName,
                    Message = d.Message,
                    CreatedAt = d.CreatedAt,
                    CanEdit = d.UserId == currentUserId
                })
                .ToListAsync();

            return discussions;
        }

        public async Task<DiscussionViewModel> CreateDiscussionAsync(int inventoryId, string message, string userId)
        {
            var discussion = new Discussion
            {
                InventoryId = inventoryId,
                UserId = userId,
                Message = message.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Discussions.Add(discussion);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            return new DiscussionViewModel
            {
                Id = discussion.Id,
                InventoryId = discussion.InventoryId,
                UserId = discussion.UserId,
                UserName = user?.FullName ?? user?.UserName ?? "Unknown",
                Message = discussion.Message,
                CreatedAt = discussion.CreatedAt,
                CanEdit = true
            };
        }

        public async Task<bool> DeleteDiscussionAsync(int discussionId, string userId)
        {
            var discussion = await _context.Discussions
                .Include(d => d.Inventory)
                .FirstOrDefaultAsync(d => d.Id == discussionId);

            if (discussion == null)
                return false;

            // Check if user can delete (owner of message or owner of inventory)
            if (discussion.UserId != userId && discussion.Inventory.CreatorId != userId)
                return false;

            _context.Discussions.Remove(discussion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanEditDiscussionAsync(int discussionId, string userId)
        {
            var discussion = await _context.Discussions.FindAsync(discussionId);
            return discussion != null && discussion.UserId == userId;
        }
    }
}