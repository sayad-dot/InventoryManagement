using InventoryManagement.ViewModels;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Create realistic dummy data
            var viewModel = new HomeViewModel
            {
                LatestInventories = new List<Inventory>
                {
                    new Inventory { 
                        Id = 1, 
                        Title = "Office Equipment", 
                        Description = "Computers, monitors, printers and other office equipment",
                        ItemCount = 47,
                        CreatedAt = DateTime.Now.AddDays(-1),
                        Creator = new ApplicationUser { UserName = "admin", FullName = "System Admin" }
                    },
                    new Inventory { 
                        Id = 2, 
                        Title = "Library Books", 
                        Description = "Technical books and programming references",
                        ItemCount = 128,
                        CreatedAt = DateTime.Now.AddDays(-2),
                        Creator = new ApplicationUser { UserName = "librarian", FullName = "Sarah Johnson" }
                    },
                    new Inventory { 
                        Id = 3, 
                        Title = "Furniture Inventory", 
                        Description = "Office chairs, desks, and conference room furniture",
                        ItemCount = 89,
                        CreatedAt = DateTime.Now.AddDays(-3),
                        Creator = new ApplicationUser { UserName = "facilities", FullName = "Mike Wilson" }
                    }
                },
                PopularInventories = new List<Inventory>
                {
                    new Inventory { 
                        Id = 1, 
                        Title = "Office Equipment", 
                        Description = "Computers, monitors, printers and other office equipment",
                        ItemCount = 47,
                        CreatedAt = DateTime.Now.AddDays(-1),
                        Creator = new ApplicationUser { UserName = "admin", FullName = "System Admin" }
                    },
                    new Inventory { 
                        Id = 6, 
                        Title = "Electronics Lab", 
                        Description = "Test equipment and electronic components",
                        ItemCount = 156,
                        CreatedAt = DateTime.Now.AddDays(-10),
                        Creator = new ApplicationUser { UserName = "lab_tech", FullName = "Emily Chen" }
                    }
                },
                Tags = new List<string> 
                { 
                    "equipment", "books", "furniture", "electronics", 
                    "vehicles", "tools", "machinery", "supplies"
                }
            };

            // Add user-specific welcome message if authenticated
            if (User.Identity.IsAuthenticated)
            {
                ViewData["WelcomeMessage"] = $"Welcome back, {User.Identity.Name}!";
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
        public IActionResult Admin()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
