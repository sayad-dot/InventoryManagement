using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InventoryId { get; set; }

        [ForeignKey("InventoryId")]
        public Inventory Inventory { get; set; } = null!;

        // Custom ID for this item (within the inventory)
        public string? CustomId { get; set; }

        // Custom Fields - Single-line text values
        public string? CustomString1Value { get; set; }
        public string? CustomString2Value { get; set; }
        public string? CustomString3Value { get; set; }

        // Custom Fields - Multi-line text values
        public string? CustomText1Value { get; set; }
        public string? CustomText2Value { get; set; }
        public string? CustomText3Value { get; set; }

        // Custom Fields - Number values
        public decimal? CustomNumber1Value { get; set; }
        public decimal? CustomNumber2Value { get; set; }
        public decimal? CustomNumber3Value { get; set; }

        // Custom Fields - Boolean values
        public bool? CustomBool1Value { get; set; }
        public bool? CustomBool2Value { get; set; }
        public bool? CustomBool3Value { get; set; }

        // Custom Fields - File/Image URL values
        public string? CustomFile1Value { get; set; }
        public string? CustomFile2Value { get; set; }
        public string? CustomFile3Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Version for optimistic locking
        public int Version { get; set; } = 1;

        // Navigation properties
        public ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();
    }
}