using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    public class UserAccess
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InventoryId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("InventoryId")]
        public Inventory Inventory { get; set; } = null!;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}