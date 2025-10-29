using InventoryManagement.Data;
using InventoryManagement.Models;
using InventoryManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Services
{
    public interface ISearchService
    {
        Task<SearchViewModel> SearchAsync(string query, string searchType = "all", int page = 1, int pageSize = 10);
        Task<List<SearchResultViewModel>> SearchInventoriesAsync(string query, int page = 1, int pageSize = 10);
        Task<List<SearchResultViewModel>> SearchItemsAsync(string query, int page = 1, int pageSize = 10);
    }

    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SearchService> _logger;

        public SearchService(ApplicationDbContext context, ILogger<SearchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SearchViewModel> SearchAsync(string query, string searchType = "all", int page = 1, int pageSize = 10)
        {
            var viewModel = new SearchViewModel
            {
                Query = query,
                SearchType = searchType,
                Page = page,
                PageSize = pageSize
            };

            try
            {
                List<SearchResultViewModel> results = new List<SearchResultViewModel>();

                if (searchType == "all" || searchType == "inventories")
                {
                    var inventoryResults = await SearchInventoriesAsync(query, page, pageSize);
                    results.AddRange(inventoryResults);
                }

                if (searchType == "all" || searchType == "items")
                {
                    var itemResults = await SearchItemsAsync(query, page, pageSize);
                    results.AddRange(itemResults);
                }

                // Sort by relevance and apply pagination
                viewModel.Results = results
                    .OrderByDescending(r => r.Relevance)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                viewModel.TotalResults = results.Count;

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search for query: {Query}", query);
                return viewModel;
            }
        }

        public async Task<List<SearchResultViewModel>> SearchInventoriesAsync(string query, int page = 1, int pageSize = 10)
        {
            var searchTerms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var inventories = await _context.Inventories
                .Include(i => i.Creator)
                .Include(i => i.InventoryTags)
                .ThenInclude(it => it.Tag)
                .Where(i => i.IsPublic || _context.InventoryAccesses.Any(ia => ia.InventoryId == i.Id)) // Only accessible inventories
                .ToListAsync();

            var results = new List<SearchResultViewModel>();

            foreach (var inventory in inventories)
            {
                var score = CalculateRelevance(inventory, searchTerms);
                
                if (score > 0)
                {
                    var highlight = BuildHighlight(inventory, searchTerms);
                    
                    results.Add(new SearchResultViewModel
                    {
                        Type = "inventory",
                        Id = inventory.Id,
                        Title = inventory.Title,
                        Description = inventory.Description,
                        Highlight = highlight,
                        Url = $"/Inventory/Details/{inventory.Id}",
                        CreatedAt = inventory.CreatedAt,
                        CreatorName = inventory.Creator.FullName,
                        Relevance = score
                    });
                }
            }

            return results.OrderByDescending(r => r.Relevance).ToList();
        }

        public async Task<List<SearchResultViewModel>> SearchItemsAsync(string query, int page = 1, int pageSize = 10)
        {
            var searchTerms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var items = await _context.Items
                .Include(i => i.Inventory)
                .ThenInclude(inv => inv.Creator)
                .Where(i => i.Inventory.IsPublic || _context.InventoryAccesses.Any(ia => ia.InventoryId == i.InventoryId))
                .ToListAsync();

            var results = new List<SearchResultViewModel>();

            foreach (var item in items)
            {
                var score = CalculateRelevance(item, searchTerms);
                
                if (score > 0)
                {
                    var highlight = BuildHighlight(item, searchTerms);
                    
                    results.Add(new SearchResultViewModel
                    {
                        Type = "item",
                        Id = item.Id,
                        Title = $"Item: {item.CustomId ?? "No ID"}",
                        Description = $"From {item.Inventory.Title}",
                        Highlight = highlight,
                        Url = $"/Inventory/Items/{item.InventoryId}",
                        CreatedAt = item.CreatedAt,
                        CreatorName = item.Inventory.Creator.FullName,
                        InventoryTitle = item.Inventory.Title,
                        InventoryId = item.InventoryId,
                        CustomId = item.CustomId ?? string.Empty,
                        Relevance = score
                    });
                }
            }

            return results.OrderByDescending(r => r.Relevance).ToList();
        }

        private double CalculateRelevance(Inventory inventory, string[] searchTerms)
        {
            double score = 0;
            var textToSearch = $"{inventory.Title} {inventory.Description} {string.Join(" ", inventory.InventoryTags.Select(it => it.Tag.Name))}".ToLower();

            foreach (var term in searchTerms)
            {
                if (inventory.Title.ToLower().Contains(term))
                    score += 3;
                if (inventory.Description.ToLower().Contains(term))
                    score += 2;
                if (textToSearch.Contains(term))
                    score += 1;
            }

            return score;
        }

        private double CalculateRelevance(Item item, string[] searchTerms)
        {
            double score = 0;
            
            // Combine all searchable text from the item
            var textToSearch = $"{item.CustomId} {item.CustomString1Value} {item.CustomString2Value} {item.CustomString3Value} {item.CustomText1Value} {item.CustomText2Value} {item.CustomText3Value}".ToLower();

            foreach (var term in searchTerms)
            {
                if (item.CustomId?.ToLower().Contains(term) == true)
                    score += 3;
                if (textToSearch.Contains(term))
                    score += 1;
            }

            return score;
        }

        private string BuildHighlight(Inventory inventory, string[] searchTerms)
        {
            var highlights = new List<string>();
            var textToSearch = $"{inventory.Title} {inventory.Description}".ToLower();

            foreach (var term in searchTerms)
            {
                if (inventory.Title.ToLower().Contains(term))
                {
                    highlights.Add($"Title: {HighlightTerm(inventory.Title, term)}");
                }
                else if (inventory.Description.ToLower().Contains(term))
                {
                    var excerpt = GetExcerpt(inventory.Description, term);
                    highlights.Add($"Description: {excerpt}");
                }
            }

            return string.Join(" | ", highlights.Take(3));
        }

        private string BuildHighlight(Item item, string[] searchTerms)
        {
            var highlights = new List<string>();

            foreach (var term in searchTerms)
            {
                // Check custom ID
                if (item.CustomId?.ToLower().Contains(term) == true)
                {
                    highlights.Add($"ID: {HighlightTerm(item.CustomId, term)}");
                }

                // Check custom string fields
                CheckFieldForTerm(item.CustomString1Value, "Field 1", term, highlights);
                CheckFieldForTerm(item.CustomString2Value, "Field 2", term, highlights);
                CheckFieldForTerm(item.CustomString3Value, "Field 3", term, highlights);
                CheckFieldForTerm(item.CustomText1Value, "Text Field 1", term, highlights);
                CheckFieldForTerm(item.CustomText2Value, "Text Field 2", term, highlights);
                CheckFieldForTerm(item.CustomText3Value, "Text Field 3", term, highlights);
            }

            return string.Join(" | ", highlights.Take(2));
        }

        private void CheckFieldForTerm(string? fieldValue, string fieldName, string term, List<string> highlights)
        {
            if (!string.IsNullOrEmpty(fieldValue) && fieldValue.ToLower().Contains(term))
            {
                var excerpt = GetExcerpt(fieldValue, term);
                highlights.Add($"{fieldName}: {excerpt}");
            }
        }

        private string HighlightTerm(string text, string term)
        {
            return text.Replace(term, $"<mark>{term}</mark>", StringComparison.OrdinalIgnoreCase);
        }

        private string GetExcerpt(string text, string term, int contextLength = 50)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var index = text.ToLower().IndexOf(term.ToLower());
            if (index == -1) return text.Length > contextLength ? text.Substring(0, contextLength) + "..." : text;

            var start = Math.Max(0, index - contextLength / 2);
            var end = Math.Min(text.Length, index + term.Length + contextLength / 2);

            var excerpt = text.Substring(start, end - start);
            if (start > 0) excerpt = "..." + excerpt;
            if (end < text.Length) excerpt = excerpt + "...";

            return HighlightTerm(excerpt, term);
        }
    }
}