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
        public string CreatorId { get; set; } = string.Empty; // Changed to string for Identity

        [ForeignKey("CreatorId")]
        public ApplicationUser Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // New fields for home page display
        public int ItemCount { get; set; }
        public string? ImageUrl { get; set; }

        // Access control
        public bool IsPublic { get; set; } = false;

        // Navigation properties
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<InventoryAccess> AccessUsers { get; set; } = new List<InventoryAccess>();
    }
}