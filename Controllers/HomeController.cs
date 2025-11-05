using InventoryManagement.ViewModels;
using InventoryManagement.Models;
using InventoryManagement.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch latest inventories from database (ordered by creation date, newest first)
            var latestInventories = await _context.Inventories
                .Include(i => i.Creator)
                .Include(i => i.Items)
                .OrderByDescending(i => i.CreatedAt)
                .Take(10)
                .Select(i => new Inventory
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    ImageUrl = i.ImageUrl,
                    CreatedAt = i.CreatedAt,
                    Creator = i.Creator,
                    ItemCount = i.Items.Count
                })
                .ToListAsync();

            // Fetch top 5 most popular inventories (based on number of items)
            var popularInventories = await _context.Inventories
                .Include(i => i.Creator)
                .Include(i => i.Items)
                .OrderByDescending(i => i.Items.Count)
                .Take(5)
                .Select(i => new Inventory
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    ImageUrl = i.ImageUrl,
                    CreatedAt = i.CreatedAt,
                    Creator = i.Creator,
                    ItemCount = i.Items.Count
                })
                .ToListAsync();

            // Fetch all unique tags from the database
            var tags = await _context.Set<Tag>()
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToListAsync();

            // Calculate statistics
            var totalInventories = await _context.Inventories.CountAsync();
            var totalItems = await _context.Items.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalCategories = await _context.Set<Category>()
                .CountAsync();

            var viewModel = new HomeViewModel
            {
                LatestInventories = latestInventories,
                PopularInventories = popularInventories,
                Tags = tags,
                TotalInventories = totalInventories,
                TotalItems = totalItems,
                TotalUsers = totalUsers,
                TotalCategories = totalCategories
            };

            // Add user-specific welcome message if authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                ViewData["WelcomeMessage"] = $"Welcome back, {User.Identity.Name ?? "User"}!";
            }
            else
            {
                ViewData["WelcomeMessage"] = "Welcome to Inventory Manager";
            }

            return View(viewModel);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get basic admin stats
            var totalUsers = await _userManager.Users.CountAsync();
            var totalInventories = await _context.Inventories.CountAsync();
            var totalItems = await _context.Items.CountAsync();
            var totalDiscussions = await _context.Discussions.CountAsync();
            
            ViewData["TotalUsers"] = totalUsers;
            ViewData["TotalInventories"] = totalInventories;
            ViewData["TotalItems"] = totalItems;
            ViewData["TotalDiscussions"] = totalDiscussions;
            ViewData["AdminName"] = user.FullName;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
