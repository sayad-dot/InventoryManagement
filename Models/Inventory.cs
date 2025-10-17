using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public int CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public User Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // New fields for home page display
        public int ItemCount { get; set; }
        public string? ImageUrl { get; set; }

        // Navigation properties
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}