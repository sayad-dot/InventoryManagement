using System.ComponentModel.DataAnnotations;
using InventoryManagement.Models;

namespace InventoryManagement.ViewModels
{
    public class CreateInventoryViewModel
    {
        [Required]
        [StringLength(200, ErrorMessage = "Title must be between 1 and 200 characters.")]
        [Display(Name = "Inventory Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Tags (separate with commas)")]
        public string TagsInput { get; set; } = string.Empty;

        [Display(Name = "Make this inventory public")]
        public bool IsPublic { get; set; } = false;

        [Url]
        [Display(Name = "Image URL (optional)")]
        public string? ImageUrl { get; set; }

        // For dropdown population
        public List<Category> AvailableCategories { get; set; } = new List<Category>();
        public List<string> AvailableTags { get; set; } = new List<string>();
    }

    public class InventoryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public string CreatorId { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ItemCount { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public bool IsOwner { get; set; }
    }

    public class MyInventoriesViewModel
    {
        public List<InventoryViewModel> OwnedInventories { get; set; } = new List<InventoryViewModel>();
        public List<InventoryViewModel> AccessibleInventories { get; set; } = new List<InventoryViewModel>();
    }
        public class EditInventoryViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Title must be between 1 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        public int? CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }

        // Custom Fields
        public List<CustomFieldViewModel> CustomFields { get; set; } = new List<CustomFieldViewModel>();
        public string FieldOrder { get; set; } = string.Empty;

        // For dropdown population
        public List<Category> AvailableCategories { get; set; } = new List<Category>();
    }
}