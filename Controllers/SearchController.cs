using InventoryManagement.Services;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SearchViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SearchViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var results = await _searchService.SearchAsync(
                    model.Query, 
                    model.SearchType, 
                    model.Page, 
                    model.PageSize
                );

                return View(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during search for query: {Query}", model.Query);
                ModelState.AddModelError("", "An error occurred during search. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> QuickSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new { success = false, message = "Query must be at least 2 characters long." });
            }

            try
            {
                var results = await _searchService.SearchAsync(query, "all", 1, 5);
                return Json(new { success = true, results = results.Results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during quick search for query: {Query}", query);
                return Json(new { success = false, message = "Search failed." });
            }
        }
    }
}