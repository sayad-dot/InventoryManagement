using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    public class ItemLike
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ItemId")]
        public Item Item { get; set; } = null!;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }

    public class LikeViewModel
    {
        public int ItemId { get; set; }
        public bool IsLiked { get; set; }
        public int LikeCount { get; set; }
    }
}