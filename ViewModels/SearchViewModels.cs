using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.ViewModels
{
    public class SearchViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Search term must be between 2 and 100 characters.", MinimumLength = 2)]
        public string Query { get; set; } = string.Empty;
        
        public string SearchType { get; set; } = "all"; // all, inventories, items
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalResults { get; set; }
        public List<SearchResultViewModel> Results { get; set; } = new List<SearchResultViewModel>();
    }

    public class SearchResultViewModel
    {
        public string Type { get; set; } = string.Empty; // "inventory" or "item"
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Highlight { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public double Relevance { get; set; }
        
        // For items
        public string InventoryTitle { get; set; } = string.Empty;
        public int InventoryId { get; set; }
        public string CustomId { get; set; } = string.Empty;
    }

    public class StatisticsViewModel
    {
        public int InventoryId { get; set; }
        public string InventoryTitle { get; set; } = string.Empty;
        
        // Basic counts
        public int TotalItems { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDiscussions { get; set; }
        public int UniqueContributors { get; set; }
        
        // Field statistics
        public List<FieldStatViewModel> FieldStats { get; set; } = new List<FieldStatViewModel>();
        
        // Recent activity
        public DateTime LastItemAdded { get; set; }
        public DateTime LastDiscussion { get; set; }
        
        // Popular items
        public List<PopularItemViewModel> PopularItems { get; set; } = new List<PopularItemViewModel>();
    }

    public class FieldStatViewModel
    {
        public string FieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty; // For display
        
        // For numeric fields
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal? AverageValue { get; set; }
        public decimal? SumValue { get; set; }
        
        // For text fields
        public List<ValueCountViewModel> CommonValues { get; set; } = new List<ValueCountViewModel>();
        public int UniqueValues { get; set; }
        public int EmptyCount { get; set; }
        
        // For boolean fields
        public int TrueCount { get; set; }
        public int FalseCount { get; set; }
    }

    public class ValueCountViewModel
    {
        public string Value { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class PopularItemViewModel
    {
        public int ItemId { get; set; }
        public string CustomId { get; set; } = string.Empty;
        public int LikeCount { get; set; }
        public string DisplayValue { get; set; } = string.Empty;
    }
}
