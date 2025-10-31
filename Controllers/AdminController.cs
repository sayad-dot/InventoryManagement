using InventoryManagement.Data;
using InventoryManagement.Models;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Use raw SQL to avoid Entity Framework conflicts
            var sql = @"
                SELECT 
                    u.""Id"",
                    COALESCE(u.""Email"", '') as ""Email"",
                    u.""FullName"",
                    CASE 
                        WHEN u.""LockoutEnd"" IS NOT NULL AND u.""LockoutEnd"" > NOW() THEN true 
                        ELSE false 
                    END as ""IsBlocked"",
                    CASE 
                        WHEN r.""Name"" = 'Admin' THEN true 
                        ELSE false 
                    END as ""IsAdmin""
                FROM ""AspNetUsers"" u
                LEFT JOIN ""AspNetUserRoles"" ur ON u.""Id"" = ur.""UserId""
                LEFT JOIN ""AspNetRoles"" r ON ur.""RoleId"" = r.""Id""
                ORDER BY u.""Email""";

            var users = new List<UserManagementViewModel>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                await _context.Database.OpenConnectionAsync();
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new UserManagementViewModel
                        {
                            Id = reader["Id"].ToString() ?? "",
                            Email = reader["Email"].ToString() ?? "",
                            FullName = reader["FullName"].ToString() ?? "",
                            IsBlocked = Convert.ToBoolean(reader["IsBlocked"]),
                            IsAdmin = Convert.ToBoolean(reader["IsAdmin"])
                        });
                    }
                }
            }

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBlock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.LockoutEnd == null || user.LockoutEnd < DateTime.Now)
            {
                // Block the user
                user.LockoutEnd = DateTime.Now.AddYears(100); // Effectively permanent block
                user.LockoutEnabled = true;
            }
            else
            {
                // Unblock the user
                user.LockoutEnd = null;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to toggle block for user {UserId}", userId);
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                // Remove admin role
                var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to remove admin role for user {UserId}", userId);
                    return BadRequest();
                }
            }
            else
            {
                // Add admin role
                var result = await _userManager.AddToRoleAsync(user, "Admin");
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to add admin role for user {UserId}", userId);
                    return BadRequest();
                }
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == userId)
            {
                return BadRequest("You cannot delete yourself.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to delete user {UserId}", userId);
                return BadRequest();
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> SystemStats()
        {
            var stats = new SystemStatisticsViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalInventories = await _context.Inventories.CountAsync(),
                TotalItems = await _context.Items.CountAsync(),
                TotalDiscussions = await _context.Discussions.CountAsync(),
                RecentUsers = await _userManager.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10)
                    .Select(u => new UserViewModel 
                    { 
                        Id = u.Id, 
                        Email = u.Email ?? "", 
                        FullName = u.FullName,
                        CreatedAt = u.CreatedAt 
                    })
                    .ToListAsync()
            };

            return View(stats);
        }
    }
}