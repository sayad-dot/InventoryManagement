using InventoryManagement.Models;

namespace InventoryManagement.ViewModels
{
    public class HomeViewModel
    {
        public List<Inventory> LatestInventories { get; set; } = new();
        public List<Inventory> PopularInventories { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }
}