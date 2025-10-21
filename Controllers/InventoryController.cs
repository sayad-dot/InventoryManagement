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
                           (user != null && await _context.InventoryAccesses.AnyAsync(ia => ia.InventoryId == id && ia.UserId == user.Id));

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

        [HttpGet]
        public async Task<IActionResult> Settings(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (inventory.CreatorId != user?.Id && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var viewModel = new EditInventoryViewModel
            {
                Id = inventory.Id,
                Title = inventory.Title,
                Description = inventory.Description,
                CategoryId = inventory.CategoryId,
                ImageUrl = inventory.ImageUrl,
                IsPublic = inventory.IsPublic,
                AvailableCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync(),
                FieldOrder = inventory.FieldOrder ?? string.Empty
            };

            // Load custom fields from inventory
            LoadCustomFieldsToViewModel(inventory, viewModel);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBasicSettings(EditInventoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
                return View("Settings", model);
            }

            var inventory = await _context.Inventories.FindAsync(model.Id);
            if (inventory == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (inventory.CreatorId != user?.Id && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                inventory.Title = model.Title;
                inventory.Description = model.Description;
                inventory.CategoryId = model.CategoryId;
                inventory.ImageUrl = model.ImageUrl;
                inventory.IsPublic = model.IsPublic;
                inventory.UpdatedAt = DateTime.UtcNow;

                _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Basic settings updated successfully!";
                return RedirectToAction("Settings", new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory settings for inventory {InventoryId}", model.Id);
                ModelState.AddModelError("", "An error occurred while updating the inventory. Please try again.");
                model.AvailableCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
                return View("Settings", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCustomFields(EditInventoryViewModel model)
        {
            var inventory = await _context.Inventories.FindAsync(model.Id);
            if (inventory == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (inventory.CreatorId != user?.Id && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // Update custom fields from view model
                UpdateCustomFieldsFromViewModel(inventory, model);
                
                inventory.FieldOrder = model.FieldOrder;
                inventory.UpdatedAt = DateTime.UtcNow;

                _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Custom fields updated successfully!";
                return RedirectToAction("Settings", new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating custom fields for inventory {InventoryId}", model.Id);
                ModelState.AddModelError("", "An error occurred while updating custom fields. Please try again.");
                model.AvailableCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
                LoadCustomFieldsToViewModel(inventory, model);
                return View("Settings", model);
            }
        }

        private void LoadCustomFieldsToViewModel(Inventory inventory, EditInventoryViewModel viewModel)
        {
            // Single-line text fields
            for (int i = 1; i <= 3; i++)
            {
                viewModel.CustomFields.Add(new CustomFieldViewModel
                {
                    Type = "string",
                    Index = i,
                    Name = GetCustomStringName(inventory, i) ?? string.Empty,
                    IsActive = GetCustomStringActive(inventory, i)
                });
            }

            // Multi-line text fields
            for (int i = 1; i <= 3; i++)
            {
                viewModel.CustomFields.Add(new CustomFieldViewModel
                {
                    Type = "text",
                    Index = i,
                    Name = GetCustomTextName(inventory, i) ?? string.Empty,
                    IsActive = GetCustomTextActive(inventory, i)
                });
            }

            // Number fields
            for (int i = 1; i <= 3; i++)
            {
                viewModel.CustomFields.Add(new CustomFieldViewModel
                {
                    Type = "number",
                    Index = i,
                    Name = GetCustomNumberName(inventory, i) ?? string.Empty,
                    IsActive = GetCustomNumberActive(inventory, i)
                });
            }

            // Boolean fields
            for (int i = 1; i <= 3; i++)
            {
                viewModel.CustomFields.Add(new CustomFieldViewModel
                {
                    Type = "bool",
                    Index = i,
                    Name = GetCustomBoolName(inventory, i) ?? string.Empty,
                    IsActive = GetCustomBoolActive(inventory, i)
                });
            }

            // File fields
            for (int i = 1; i <= 3; i++)
            {
                viewModel.CustomFields.Add(new CustomFieldViewModel
                {
                    Type = "file",
                    Index = i,
                    Name = GetCustomFileName(inventory, i) ?? string.Empty,
                    IsActive = GetCustomFileActive(inventory, i)
                });
            }
        }

        private void UpdateCustomFieldsFromViewModel(Inventory inventory, EditInventoryViewModel model)
        {
            foreach (var field in model.CustomFields)
            {
                switch (field.Type)
                {
                    case "string":
                        SetCustomStringField(inventory, field.Index, field.Name, field.IsActive);
                        break;
                    case "text":
                        SetCustomTextField(inventory, field.Index, field.Name, field.IsActive);
                        break;
                    case "number":
                        SetCustomNumberField(inventory, field.Index, field.Name, field.IsActive);
                        break;
                    case "bool":
                        SetCustomBoolField(inventory, field.Index, field.Name, field.IsActive);
                        break;
                    case "file":
                        SetCustomFileField(inventory, field.Index, field.Name, field.IsActive);
                        break;
                }
            }
        }

        // Helper methods for getting custom field values
        private string? GetCustomStringName(Inventory inventory, int index) => index switch
        {
            1 => inventory.CustomString1Name,
            2 => inventory.CustomString2Name,
            3 => inventory.CustomString3Name,
            _ => null
        };

        private bool GetCustomStringActive(Inventory inventory, int index) => index switch
        {
            1 => inventory.CustomString1Active,
            2 => inventory.CustomString2Active,
            3 => inventory.CustomString3Active,
            _ => false
        };

        private string? GetCustomTextName(Inventory inventory, int index) => index switch
        {
            1 => inventory.CustomText1Name,
            2 => inventory.CustomText2Name,
            3 => inventory.CustomText3Name,
            _ => null
        };

        private bool GetCustomTextActive(Inventory inventory, int index) => index switch
        {
            1 => inventory.CustomText1Active,
            2 => inventory.CustomText2Active,
            3 => inventory.CustomText3Active,
            _ => false
        };

        private string? GetCustomNumberName(Inventory inventory, int index) => index switch
        {
            1 => inventory.CustomNumber1Name,
            2 => inventory.CustomNumber2Name,
            3 => inventory.CustomNumber3Name,
            _ => null
        };

        private bool GetCustomNumberActive(Inventory inventory, int index) => index switch
        {
            1 => inventory.CustomNumber1Active,
            2 => inventory.CustomNumber2Active,
            3 => inventory.CustomNumber3Active,
            _ => false
        };

        private string? GetCustomBoolName(Inventory inventory, int index) => index switch
        {
            1 => inventory.CustomBool1Name,
            2 => inventory.CustomBool2Name,
            3 => inventory.CustomBool3Name,
            _ => null
        };

        private bool GetCustomBoolActive(Inventory inventory, int index) => index switch
        {
            1 => inventory.CustomBool1Active,
            2 => inventory.CustomBool2Active,
            3 => inventory.CustomBool3Active,
            _ => false
        };

        private string? GetCustomFileName(Inventory inventory, int index) => index switch
        {
            1 => inventory.CustomFile1Name,
            2 => inventory.CustomFile2Name,
            3 => inventory.CustomFile3Name,
            _ => null
        };

        private bool GetCustomFileActive(Inventory inventory, int index) => index switch
        {
            1 => inventory.CustomFile1Active,
            2 => inventory.CustomFile2Active,
            3 => inventory.CustomFile3Active,
            _ => false
        };

        // Helper methods for setting custom field values
        private void SetCustomStringField(Inventory inventory, int index, string name, bool isActive)
        {
            switch (index)
            {
                case 1:
                    inventory.CustomString1Name = name;
                    inventory.CustomString1Active = isActive;
                    break;
                case 2:
                    inventory.CustomString2Name = name;
                    inventory.CustomString2Active = isActive;
                    break;
                case 3:
                    inventory.CustomString3Name = name;
                    inventory.CustomString3Active = isActive;
                    break;
            }
        }

        private void SetCustomTextField(Inventory inventory, int index, string name, bool isActive)
        {
            switch (index)
            {
                case 1:
                    inventory.CustomText1Name = name;
                    inventory.CustomText1Active = isActive;
                    break;
                case 2:
                    inventory.CustomText2Name = name;
                    inventory.CustomText2Active = isActive;
                    break;
                case 3:
                    inventory.CustomText3Name = name;
                    inventory.CustomText3Active = isActive;
                    break;
            }
        }

        private void SetCustomNumberField(Inventory inventory, int index, string name, bool isActive)
        {
            switch (index)
            {
                case 1:
                    inventory.CustomNumber1Name = name;
                    inventory.CustomNumber1Active = isActive;
                    break;
                case 2:
                    inventory.CustomNumber2Name = name;
                    inventory.CustomNumber2Active = isActive;
                    break;
                case 3:
                    inventory.CustomNumber3Name = name;
                    inventory.CustomNumber3Active = isActive;
                    break;
            }
        }

        private void SetCustomBoolField(Inventory inventory, int index, string name, bool isActive)
        {
            switch (index)
            {
                case 1:
                    inventory.CustomBool1Name = name;
                    inventory.CustomBool1Active = isActive;
                    break;
                case 2:
                    inventory.CustomBool2Name = name;
                    inventory.CustomBool2Active = isActive;
                    break;
                case 3:
                    inventory.CustomBool3Name = name;
                    inventory.CustomBool3Active = isActive;
                    break;
            }
        }

        private void SetCustomFileField(Inventory inventory, int index, string name, bool isActive)
        {
            switch (index)
            {
                case 1:
                    inventory.CustomFile1Name = name;
                    inventory.CustomFile1Active = isActive;
                    break;
                case 2:
                    inventory.CustomFile2Name = name;
                    inventory.CustomFile2Active = isActive;
                    break;
                case 3:
                    inventory.CustomFile3Name = name;
                    inventory.CustomFile3Active = isActive;
                    break;
            }
        }
    }
}