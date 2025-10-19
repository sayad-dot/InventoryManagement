using InventoryManagement.Data;
using InventoryManagement.Models;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<InventoryController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> MyInventories()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new MyInventoriesViewModel();

            // Get owned inventories
            var ownedInventories = await _context.Inventories
                .Where(i => i.CreatorId == user.Id)
                .Include(i => i.Category)
                .Include(i => i.InventoryTags)
                    .ThenInclude(it => it.Tag)
                .OrderByDescending(i => i.UpdatedAt)
                .ToListAsync();

            viewModel.OwnedInventories = ownedInventories.Select(i => new InventoryViewModel
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                CreatorName = user.FullName,
                CreatorId = user.Id,
                CategoryName = i.Category?.Name,
                ImageUrl = i.ImageUrl,
                IsPublic = i.IsPublic,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                ItemCount = i.ItemCount,
                Tags = i.InventoryTags.Select(it => it.Tag.Name).ToList(),
                IsOwner = true
            }).ToList();

            // Get accessible inventories (where user has access but doesn't own)
            var accessibleInventories = await _context.InventoryAccesses
                .Where(ia => ia.UserId == user.Id)
                .Include(ia => ia.Inventory)
                    .ThenInclude(i => i.Category)
                .Include(ia => ia.Inventory)
                    .ThenInclude(i => i.InventoryTags)
                    .ThenInclude(it => it.Tag)
                .Include(ia => ia.Inventory)
                    .ThenInclude(i => i.Creator)
                .Select(ia => ia.Inventory)
                .OrderByDescending(i => i.UpdatedAt)
                .ToListAsync();

            viewModel.AccessibleInventories = accessibleInventories.Select(i => new InventoryViewModel
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                CreatorName = i.Creator.FullName,
                CreatorId = i.CreatorId,
                CategoryName = i.Category?.Name,
                ImageUrl = i.ImageUrl,
                IsPublic = i.IsPublic,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                ItemCount = i.ItemCount,
                Tags = i.InventoryTags.Select(it => it.Tag.Name).ToList(),
                IsOwner = false
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            var tags = await _context.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToListAsync();

            var viewModel = new CreateInventoryViewModel
            {
                AvailableCategories = categories,
                AvailableTags = tags
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInventoryViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Create the inventory
                    var inventory = new Inventory
                    {
                        Title = model.Title,
                        Description = model.Description,
                        CreatorId = user.Id,
                        CategoryId = model.CategoryId > 0 ? model.CategoryId : null,
                        ImageUrl = model.ImageUrl,
                        IsPublic = model.IsPublic,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Inventories.Add(inventory);
                    await _context.SaveChangesAsync();

                    // Process tags
                    if (!string.IsNullOrWhiteSpace(model.TagsInput))
                    {
                        var tagNames = model.TagsInput.Split(',')
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                            .Distinct()
                            .ToList();

                        foreach (var tagName in tagNames)
                        {
                            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
                            if (tag == null)
                            {
                                tag = new Tag { Name = tagName.ToLower() };
                                _context.Tags.Add(tag);
                                await _context.SaveChangesAsync();
                            }

                            var inventoryTag = new InventoryTag
                            {
                                InventoryId = inventory.Id,
                                TagId = tag.Id
                            };
                            _context.InventoryTags.Add(inventoryTag);
                        }

                        await _context.SaveChangesAsync();
                    }

                    _logger.LogInformation("User {UserId} created inventory {InventoryId}", user.Id, inventory.Id);
                    TempData["SuccessMessage"] = $"Inventory '{inventory.Title}' created successfully!";
                    return RedirectToAction("MyInventories");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating inventory for user {UserId}", user.Id);
                    ModelState.AddModelError("", "An error occurred while creating the inventory. Please try again.");
                }
            }

            // If we got this far, something failed; redisplay form
            model.AvailableCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            model.AvailableTags = await _context.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Creator)
                .Include(i => i.Category)
                .Include(i => i.InventoryTags)
                    .ThenInclude(it => it.Tag)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isOwner = inventory.CreatorId == user?.Id;
            var hasAccess = isOwner || inventory.IsPublic || 
                           await _context.InventoryAccesses.AnyAsync(ia => ia.InventoryId == id && ia.UserId == user.Id);

            if (!hasAccess)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var viewModel = new InventoryViewModel
            {
                Id = inventory.Id,
                Title = inventory.Title,
                Description = inventory.Description,
                CreatorName = inventory.Creator.FullName,
                CreatorId = inventory.CreatorId,
                CategoryName = inventory.Category?.Name,
                ImageUrl = inventory.ImageUrl,
                IsPublic = inventory.IsPublic,
                CreatedAt = inventory.CreatedAt,
                UpdatedAt = inventory.UpdatedAt,
                ItemCount = inventory.ItemCount,
                Tags = inventory.InventoryTags.Select(it => it.Tag.Name).ToList(),
                IsOwner = isOwner
            };

            return View(viewModel);
        }
    }
}