using System.ComponentModel.DataAnnotations;
using InventoryManagement.Models;

namespace InventoryManagement.ViewModels
{
    public class CreateItemViewModel
    {
        public int InventoryId { get; set; }
        public string InventoryTitle { get; set; } = string.Empty;
        
        [Display(Name = "Custom ID")]
        public string? CustomId { get; set; }
        
        // Custom field values
        public List<ItemCustomFieldViewModel> CustomFields { get; set; } = new List<ItemCustomFieldViewModel>();
    }

    public class EditItemViewModel
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public string InventoryTitle { get; set; } = string.Empty;
        
        [Display(Name = "Custom ID")]
        public string? CustomId { get; set; }
        
        // Custom field values
        public List<ItemCustomFieldViewModel> CustomFields { get; set; } = new List<ItemCustomFieldViewModel>();
        
        // For optimistic locking
        public int Version { get; set; }
    }

    public class ItemCustomFieldViewModel
    {
        public string Type { get; set; } = string.Empty;
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        
        // Field values
        public string? StringValue { get; set; }
        public string? TextValue { get; set; }
        public decimal? NumberValue { get; set; }
        public bool? BoolValue { get; set; }
        public string? FileValue { get; set; }
        
        // Validation
        public bool IsRequired { get; set; }
        public string? ValidationMessage { get; set; }
    }

    public class ItemViewModel
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public string? CustomId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Version { get; set; }
        
        // Custom field values for display
        public List<ItemCustomFieldViewModel> CustomFields { get; set; } = new List<ItemCustomFieldViewModel>();
        
        // For access control
        public bool CanEdit { get; set; }
        public string CreatorName { get; set; } = string.Empty;
    }

    public class ItemsTableViewModel
    {
        public int InventoryId { get; set; }
        public string InventoryTitle { get; set; } = string.Empty;
        public bool CanEdit { get; set; }
        public List<ItemViewModel> Items { get; set; } = new List<ItemViewModel>();
        public List<string> ColumnHeaders { get; set; } = new List<string>();
    }
}