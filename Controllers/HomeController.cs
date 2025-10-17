using InventoryManagement.ViewModels;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InventoryManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
                        Creator = new User { Id = 1, Username = "admin", FullName = "System Admin" }
                    },
                    new Inventory { 
                        Id = 2, 
                        Title = "Library Books", 
                        Description = "Technical books and programming references",
                        ItemCount = 128,
                        CreatedAt = DateTime.Now.AddDays(-2),
                        Creator = new User { Id = 2, Username = "librarian", FullName = "Sarah Johnson" }
                    },
                    new Inventory { 
                        Id = 3, 
                        Title = "Furniture Inventory", 
                        Description = "Office chairs, desks, and conference room furniture",
                        ItemCount = 89,
                        CreatedAt = DateTime.Now.AddDays(-3),
                        Creator = new User { Id = 3, Username = "facilities", FullName = "Mike Wilson" }
                    },
                    new Inventory { 
                        Id = 4, 
                        Title = "IT Equipment", 
                        Description = "Servers, network gear, and IT infrastructure",
                        ItemCount = 34,
                        CreatedAt = DateTime.Now.AddDays(-4),
                        Creator = new User { Id = 1, Username = "admin", FullName = "System Admin" }
                    },
                    new Inventory { 
                        Id = 5, 
                        Title = "Company Vehicles", 
                        Description = "Fleet of company cars and service vehicles",
                        ItemCount = 12,
                        CreatedAt = DateTime.Now.AddDays(-5),
                        Creator = new User { Id = 4, Username = "fleet_mgr", FullName = "Robert Brown" }
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
                        Creator = new User { Id = 1, Username = "admin", FullName = "System Admin" }
                    },
                    new Inventory { 
                        Id = 6, 
                        Title = "Electronics Lab", 
                        Description = "Test equipment and electronic components",
                        ItemCount = 156,
                        CreatedAt = DateTime.Now.AddDays(-10),
                        Creator = new User { Id = 5, Username = "lab_tech", FullName = "Emily Chen" }
                    },
                    new Inventory { 
                        Id = 2, 
                        Title = "Library Books", 
                        Description = "Technical books and programming references",
                        ItemCount = 128,
                        CreatedAt = DateTime.Now.AddDays(-2),
                        Creator = new User { Id = 2, Username = "librarian", FullName = "Sarah Johnson" }
                    },
                    new Inventory { 
                        Id = 7, 
                        Title = "Tools & Equipment", 
                        Description = "Maintenance tools and workshop equipment",
                        ItemCount = 203,
                        CreatedAt = DateTime.Now.AddDays(-15),
                        Creator = new User { Id = 6, Username = "technician", FullName = "David Lee" }
                    },
                    new Inventory { 
                        Id = 3, 
                        Title = "Furniture Inventory", 
                        Description = "Office chairs, desks, and conference room furniture",
                        ItemCount = 89,
                        CreatedAt = DateTime.Now.AddDays(-3),
                        Creator = new User { Id = 3, Username = "facilities", FullName = "Mike Wilson" }
                    }
                },
                Tags = new List<string> 
                { 
                    "equipment", "books", "furniture", "electronics", 
                    "vehicles", "tools", "machinery", "supplies", 
                    "computers", "hardware", "software", "office",
                    "lab", "research", "development", "production"
                }
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
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
