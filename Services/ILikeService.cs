using InventoryManagement.Data;
using InventoryManagement.Models;
using InventoryManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Services
{
    public interface ILikeService
    {
        Task<LikeViewModel> ToggleLikeAsync(int itemId, string userId);
        Task<LikeViewModel> GetLikeStatusAsync(int itemId, string userId);
        Task<int> GetLikeCountAsync(int itemId);
    }

    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _context;

        public LikeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LikeViewModel> ToggleLikeAsync(int itemId, string userId)
        {
            var existingLike = await _context.ItemLikes
                .FirstOrDefaultAsync(il => il.ItemId == itemId && il.UserId == userId);

            if (existingLike != null)
            {
                // Unlike
                _context.ItemLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Like
                var like = new ItemLike
                {
                    ItemId = itemId,
                    UserId = userId,
                    LikedAt = DateTime.UtcNow
                };
                _context.ItemLikes.Add(like);
                await _context.SaveChangesAsync();
            }

            var likeCount = await GetLikeCountAsync(itemId);
            return new LikeViewModel
            {
                ItemId = itemId,
                IsLiked = existingLike == null, // If we removed, then it's not liked; if we added, then it's liked
                LikeCount = likeCount
            };
        }

        public async Task<LikeViewModel> GetLikeStatusAsync(int itemId, string userId)
        {
            var isLiked = await _context.ItemLikes
                .AnyAsync(il => il.ItemId == itemId && il.UserId == userId);

            var likeCount = await GetLikeCountAsync(itemId);

            return new LikeViewModel
            {
                ItemId = itemId,
                IsLiked = isLiked,
                LikeCount = likeCount
            };
        }

        public async Task<int> GetLikeCountAsync(int itemId)
        {
            return await _context.ItemLikes
                .CountAsync(il => il.ItemId == itemId);
        }
    }
}