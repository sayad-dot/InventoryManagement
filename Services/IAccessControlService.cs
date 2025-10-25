using InventoryManagement.Data;
using InventoryManagement.Models;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Services
{
    public interface IAccessControlService
    {
        Task<AccessControlViewModel?> GetAccessControlViewModelAsync(int inventoryId);
        Task<bool> GrantUserAccessAsync(int inventoryId, string userEmail, string currentUserId);
        Task<bool> RevokeUserAccessAsync(int accessId, string currentUserId);
        Task<List<UserSearchResultViewModel>> SearchUsersAsync(string searchTerm);
        Task<bool> HasWriteAccessAsync(int inventoryId, string userId);
    }

    public class AccessControlService : IAccessControlService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccessControlService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AccessControlViewModel?> GetAccessControlViewModelAsync(int inventoryId)
        {
            var inventory = await _context.Inventories
                .Include(i => i.AccessUsers)
                .ThenInclude(au => au.User)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null) return null;

            return new AccessControlViewModel
            {
                InventoryId = inventory.Id,
                InventoryTitle = inventory.Title,
                IsPublic = inventory.IsPublic,
                GrantedUsers = inventory.AccessUsers.Select(au => new UserAccessViewModel
                {
                    Id = au.Id,
                    UserId = au.UserId,
                    UserEmail = au.User.Email ?? string.Empty,
                    UserName = au.User.FullName,
                    GrantedAt = au.GrantedAt
                }).ToList()
            };
        }

        public async Task<bool> GrantUserAccessAsync(int inventoryId, string userEmail, string currentUserId)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null || inventory.CreatorId != currentUserId) 
                return false;

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return false;

            // Check if access already exists
            var existingAccess = await _context.InventoryAccesses
                .FirstOrDefaultAsync(ia => ia.InventoryId == inventoryId && ia.UserId == user.Id);
            
            if (existingAccess != null) return true; // Already has access

            var access = new InventoryAccess
            {
                InventoryId = inventoryId,
                UserId = user.Id
            };

            _context.InventoryAccesses.Add(access);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokeUserAccessAsync(int accessId, string currentUserId)
        {
            var access = await _context.InventoryAccesses
                .Include(ia => ia.Inventory)
                .FirstOrDefaultAsync(ia => ia.Id == accessId);

            if (access == null || access.Inventory.CreatorId != currentUserId) 
                return false;

            _context.InventoryAccesses.Remove(access);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserSearchResultViewModel>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                return new List<UserSearchResultViewModel>();

            var users = await _userManager.Users
                .Where(u => (u.Email != null && u.Email.Contains(searchTerm)) || 
                           (u.FullName != null && u.FullName.Contains(searchTerm)))
                .Take(10)
                .Select(u => new UserSearchResultViewModel
                {
                    UserId = u.Id,
                    Email = u.Email ?? string.Empty,
                    FullName = u.FullName
                })
                .ToListAsync();

            return users;
        }

        public async Task<bool> HasWriteAccessAsync(int inventoryId, string userId)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null) return false;

            // Creator has full access
            if (inventory.CreatorId == userId) return true;

            // Public inventories allow all authenticated users to write
            if (inventory.IsPublic) return true;

            // Check explicit access
            return await _context.InventoryAccesses
                .AnyAsync(ia => ia.InventoryId == inventoryId && ia.UserId == userId);
        }
    }
}