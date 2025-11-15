using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    public class SupportTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Summary { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = "Average"; // High, Average, Low

        public string? InventoryTitle { get; set; }
        
        public string? PageUrl { get; set; }

        public string? OneDriveFileId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Pending"; // Pending, Processed, Failed
    }
}
