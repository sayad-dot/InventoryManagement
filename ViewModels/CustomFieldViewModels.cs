using System.ComponentModel.DataAnnotations;
using InventoryManagement.Models;

namespace InventoryManagement.ViewModels
{
    public class CustomFieldViewModel
    {
        public string Type { get; set; } = string.Empty; // "string", "text", "number", "bool", "file"
        public int Index { get; set; } // 1, 2, or 3
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Value { get; set; } // For display in forms
        public decimal? NumberValue { get; set; }
        public bool? BoolValue { get; set; }
        public string? FileValue { get; set; }
    }

    public class InventorySettingsViewModel
    {
        public int InventoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Custom Fields
        public List<CustomFieldViewModel> CustomFields { get; set; } = new List<CustomFieldViewModel>();
        
        // Field order for drag & drop
        public string FieldOrder { get; set; } = string.Empty;
    }

    public class ItemFormViewModel
    {
        public int InventoryId { get; set; }
        public int? ItemId { get; set; }
        public string? CustomId { get; set; }
        
        // Custom field values
        public List<CustomFieldViewModel> CustomFields { get; set; } = new List<CustomFieldViewModel>();
        
        // For optimistic locking
        public int Version { get; set; } = 1;
    }
}