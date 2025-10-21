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
        public string CreatorId { get; set; } = string.Empty;

        [ForeignKey("CreatorId")]
        public ApplicationUser Creator { get; set; } = null!;

        // Category system (from predefined list)
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        // Image/illustration (cloud URL)
        public string? ImageUrl { get; set; }

        // Access control
        public bool IsPublic { get; set; } = false;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Statistics
        public int ItemCount { get; set; } = 0;
        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;

        // Custom ID Format
        public string? CustomIdFormat { get; set; }

        // Custom Fields - Single-line text (up to 3)
        public string? CustomString1Name { get; set; }
        public string? CustomString2Name { get; set; }
        public string? CustomString3Name { get; set; }
        public bool CustomString1Active { get; set; }
        public bool CustomString2Active { get; set; }
        public bool CustomString3Active { get; set; }

        // Custom Fields - Multi-line text (up to 3)
        public string? CustomText1Name { get; set; }
        public string? CustomText2Name { get; set; }
        public string? CustomText3Name { get; set; }
        public bool CustomText1Active { get; set; }
        public bool CustomText2Active { get; set; }
        public bool CustomText3Active { get; set; }

        // Custom Fields - Numbers (up to 3)
        public string? CustomNumber1Name { get; set; }
        public string? CustomNumber2Name { get; set; }
        public string? CustomNumber3Name { get; set; }
        public bool CustomNumber1Active { get; set; }
        public bool CustomNumber2Active { get; set; }
        public bool CustomNumber3Active { get; set; }

        // Custom Fields - Boolean (up to 3)
        public string? CustomBool1Name { get; set; }
        public string? CustomBool2Name { get; set; }
        public string? CustomBool3Name { get; set; }
        public bool CustomBool1Active { get; set; }
        public bool CustomBool2Active { get; set; }
        public bool CustomBool3Active { get; set; }

        // Custom Fields - File/Image URLs (up to 3)
        public string? CustomFile1Name { get; set; }
        public string? CustomFile2Name { get; set; }
        public string? CustomFile3Name { get; set; }
        public bool CustomFile1Active { get; set; }
        public bool CustomFile2Active { get; set; }
        public bool CustomFile3Active { get; set; }

        // Field display order (comma-separated field identifiers)
        public string? FieldOrder { get; set; }

        // Navigation properties
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<InventoryAccess> AccessUsers { get; set; } = new List<InventoryAccess>();
        public ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();
    }

    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }

    public class InventoryTag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InventoryId { get; set; }

        [Required]
        public int TagId { get; set; }

        // Navigation
        [ForeignKey("InventoryId")]
        public Inventory Inventory { get; set; } = null!;

        [ForeignKey("TagId")]
        public Tag Tag { get; set; } = null!;
    }

    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();
    }
}