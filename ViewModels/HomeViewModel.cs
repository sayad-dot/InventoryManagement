using InventoryManagement.Models;

namespace InventoryManagement.ViewModels
{
    public class HomeViewModel
    {
        public List<Inventory> LatestInventories { get; set; } = new();
        public List<Inventory> PopularInventories { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        
        // Statistics
        public int TotalInventories { get; set; }
        public int TotalItems { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
    }
}