using InventoryManagement.Data;
using InventoryManagement.Models;
using InventoryManagement.ViewModels;
using InventoryManagement.Services;
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
        private readonly IAccessControlService _accessControlService;

        public InventoryController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<InventoryController> logger,
            IAccessControlService accessControlService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _accessControlService = accessControlService;
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

        // ITEM MANAGEMENT METHODS

        [HttpGet]
        public async Task<IActionResult> Items(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Creator)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var hasAccess = await HasWriteAccess(inventory, user);
            
            var items = await _context.Items
                .Where(i => i.InventoryId == id)
                .OrderBy(i => i.CustomId)
                .ToListAsync();

            var viewModel = new ItemsTableViewModel
            {
                InventoryId = inventory.Id,
                InventoryTitle = inventory.Title,
                CanEdit = hasAccess,
                Items = items.Select(item => MapItemToViewModel(item, inventory, hasAccess)).ToList()
            };

            // Get column headers from active custom fields
            viewModel.ColumnHeaders = GetColumnHeaders(inventory);
            
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateItem(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Creator)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var hasAccess = await HasWriteAccess(inventory, user);
            if (!hasAccess)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var viewModel = new CreateItemViewModel
            {
                InventoryId = inventory.Id,
                InventoryTitle = inventory.Title,
                CustomFields = GetActiveCustomFieldsForForm(inventory)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(CreateItemViewModel model)
        {
            _logger.LogInformation("=== CreateItem POST STARTED ===");
            _logger.LogInformation("Model received - InventoryId: {InventoryId}, CustomId: {CustomId}", model.InventoryId, model.CustomId ?? "NULL");
            _logger.LogInformation("CustomFields received: {Count} items", model.CustomFields?.Count ?? 0);
            
            if (model.CustomFields != null)
            {
                for (int i = 0; i < model.CustomFields.Count; i++)
                {
                    var field = model.CustomFields[i];
                    _logger.LogInformation("  CustomField[{Index}]: Type={Type}, Name={Name}, StringValue={StringValue}, NumberValue={NumberValue}", 
                        i, field.Type, field.Name, field.StringValue, field.NumberValue);
                }
            }
            
            var inventory = await _context.Inventories.FindAsync(model.InventoryId);
            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found: {InventoryId}", model.InventoryId);
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var hasAccess = await HasWriteAccess(inventory, user);
            if (!hasAccess)
            {
                _logger.LogWarning("User {UserId} does not have write access to inventory {InventoryId}", user?.Id, model.InventoryId);
                return RedirectToAction("AccessDenied", "Account");
            }

            // Log ModelState validation errors
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid:");
                foreach (var modelError in ModelState)
                {
                    foreach (var error in modelError.Value.Errors)
                    {
                        _logger.LogWarning("  {Key}: {Error}", modelError.Key, error.ErrorMessage);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate CustomId within this inventory
                    if (!string.IsNullOrEmpty(model.CustomId))
                    {
                        var existingItem = await _context.Items
                            .FirstOrDefaultAsync(i => i.InventoryId == model.InventoryId && i.CustomId == model.CustomId);
                        
                        if (existingItem != null)
                        {
                            ModelState.AddModelError("CustomId", "An item with this Custom ID already exists in this inventory.");
                            model.CustomFields = GetActiveCustomFieldsForForm(inventory);
                            return View(model);
                        }
                    }

                    var item = new Item
                    {
                        InventoryId = model.InventoryId,
                        CustomId = model.CustomId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Version = 1
                    };

                    _logger.LogInformation("Created item object: InventoryId={InventoryId}, CustomId={CustomId}", item.InventoryId, item.CustomId);

                    // Set custom field values
                    if (model.CustomFields != null)
                    {
                        _logger.LogInformation("Setting custom field values for {Count} fields", model.CustomFields.Count);
                        SetItemCustomFieldValues(item, model.CustomFields);
                    }

                    _context.Items.Add(item);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Item saved to database with ID: {ItemId}", item.Id);

                    // Update inventory item count
                    await UpdateInventoryItemCount(inventory.Id);
                    
                    _logger.LogInformation("Updated inventory item count for inventory {InventoryId}", inventory.Id);

                    TempData["SuccessMessage"] = "Item created successfully!";
                    return RedirectToAction("Items", new { id = model.InventoryId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating item for inventory {InventoryId}", model.InventoryId);
                    ModelState.AddModelError("", "An error occurred while creating the item. Please try again.");
                }
            }

            // If we got here, something failed
            _logger.LogWarning("CreateItem failed, returning to view. ModelState.IsValid: {IsValid}", ModelState.IsValid);
            model.CustomFields = GetActiveCustomFieldsForForm(inventory);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditItem(int id)
        {
            var item = await _context.Items
                .Include(i => i.Inventory)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var hasAccess = await HasWriteAccess(item.Inventory, user);
            if (!hasAccess)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var viewModel = new EditItemViewModel
            {
                Id = item.Id,
                InventoryId = item.InventoryId,
                InventoryTitle = item.Inventory.Title,
                CustomId = item.CustomId,
                Version = item.Version,
                CustomFields = GetCustomFieldsForEdit(item, item.Inventory)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(EditItemViewModel model)
        {
            var item = await _context.Items
                .Include(i => i.Inventory)
                .FirstOrDefaultAsync(i => i.Id == model.Id);

            if (item == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var hasAccess = await HasWriteAccess(item.Inventory, user);
            if (!hasAccess)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Optimistic locking check
            if (item.Version != model.Version)
            {
                ModelState.AddModelError("", "This item has been modified by another user. Please refresh and try again.");
                model.CustomFields = GetCustomFieldsForEdit(item, item.Inventory);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate CustomId (excluding current item)
                    if (!string.IsNullOrEmpty(model.CustomId) && model.CustomId != item.CustomId)
                    {
                        var existingItem = await _context.Items
                            .FirstOrDefaultAsync(i => i.InventoryId == model.InventoryId && 
                                                i.CustomId == model.CustomId && 
                                                i.Id != model.Id);
                        
                        if (existingItem != null)
                        {
                            ModelState.AddModelError("CustomId", "An item with this Custom ID already exists in this inventory.");
                            model.CustomFields = GetCustomFieldsForEdit(item, item.Inventory);
                            return View(model);
                        }
                    }

                    item.CustomId = model.CustomId;
                    item.UpdatedAt = DateTime.UtcNow;
                    item.Version++;

                    // Update custom field values
                    SetItemCustomFieldValues(item, model.CustomFields);

                    _context.Items.Update(item);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Item updated successfully!";
                    return RedirectToAction("Items", new { id = model.InventoryId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "This item has been modified by another user. Please refresh and try again.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating item {ItemId}", model.Id);
                    ModelState.AddModelError("", "An error occurred while updating the item. Please try again.");
                }
            }

            // If we got here, something failed
            model.CustomFields = GetCustomFieldsForEdit(item, item.Inventory);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items
                .Include(i => i.Inventory)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var hasAccess = await HasWriteAccess(item.Inventory, user);
            if (!hasAccess)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();

                // Update inventory item count
                await UpdateInventoryItemCount(item.InventoryId);

                TempData["SuccessMessage"] = "Item deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item {ItemId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the item. Please try again.";
            }

            return RedirectToAction("Items", new { id = item.InventoryId });
        }

        // ACCESS CONTROL METHODS

        [HttpGet]
        public async Task<IActionResult> AccessControl(int id)
        {
            var viewModel = await _accessControlService.GetAccessControlViewModelAsync(id);
            if (viewModel == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            if (inventory.CreatorId != user.Id && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccessControl(AccessControlViewModel model)
        {
            var inventory = await _context.Inventories.FindAsync(model.InventoryId);
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
                inventory.IsPublic = model.IsPublic;
                inventory.UpdatedAt = DateTime.UtcNow;

                _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Access control settings updated successfully!";
                return RedirectToAction("AccessControl", new { id = model.InventoryId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating access control for inventory {InventoryId}", model.InventoryId);
                ModelState.AddModelError("", "An error occurred while updating access control. Please try again.");
                return View("AccessControl", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantAccess(int inventoryId, string userEmail)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _accessControlService.GrantUserAccessAsync(inventoryId, userEmail, user.Id);

            if (result)
            {
                TempData["SuccessMessage"] = $"Access granted to {userEmail}";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to grant access to {userEmail}. User not found or you don't have permission.";
            }

            return RedirectToAction("AccessControl", new { id = inventoryId });
        }

        [HttpPost]
        public async Task<IActionResult> RevokeAccess(int accessId, int inventoryId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _accessControlService.RevokeUserAccessAsync(accessId, user.Id);

            if (result)
            {
                TempData["SuccessMessage"] = "Access revoked successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to revoke access";
            }

            return RedirectToAction("AccessControl", new { id = inventoryId });
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var results = await _accessControlService.SearchUsersAsync(term);
            return Json(results);
        }

        // HELPER METHODS

        private async Task<bool> HasWriteAccess(Inventory inventory, ApplicationUser? user)
        {
            if (user == null) return false;
            
            // Creator has full access
            if (inventory.CreatorId == user.Id) return true;
            
            // Admins have full access
            if (User.IsInRole("Admin")) return true;
            
            // Public inventories allow all authenticated users to write
            if (inventory.IsPublic) return true;
            
            // Check explicit access
            return await _context.InventoryAccesses
                .AnyAsync(ia => ia.InventoryId == inventory.Id && ia.UserId == user.Id);
        }

        private List<ItemCustomFieldViewModel> GetActiveCustomFieldsForForm(Inventory inventory)
        {
            var fields = new List<ItemCustomFieldViewModel>();

            // Single-line text fields
            for (int i = 1; i <= 3; i++)
            {
                if (GetCustomStringActive(inventory, i) && !string.IsNullOrEmpty(GetCustomStringName(inventory, i)))
                {
                    fields.Add(new ItemCustomFieldViewModel
                    {
                        Type = "string",
                        Index = i,
                        Name = GetCustomStringName(inventory, i)!,
                        IsActive = true
                    });
                }
            }

            // Multi-line text fields
            for (int i = 1; i <= 3; i++)
            {
                if (GetCustomTextActive(inventory, i) && !string.IsNullOrEmpty(GetCustomTextName(inventory, i)))
                {
                    fields.Add(new ItemCustomFieldViewModel
                    {
                        Type = "text",
                        Index = i,
                        Name = GetCustomTextName(inventory, i)!,
                        IsActive = true
                    });
                }
            }

            // Number fields
            for (int i = 1; i <= 3; i++)
            {
                if (GetCustomNumberActive(inventory, i) && !string.IsNullOrEmpty(GetCustomNumberName(inventory, i)))
                {
                    fields.Add(new ItemCustomFieldViewModel
                    {
                        Type = "number",
                        Index = i,
                        Name = GetCustomNumberName(inventory, i)!,
                        IsActive = true
                    });
                }
            }

            // Boolean fields
            for (int i = 1; i <= 3; i++)
            {
                if (GetCustomBoolActive(inventory, i) && !string.IsNullOrEmpty(GetCustomBoolName(inventory, i)))
                {
                    fields.Add(new ItemCustomFieldViewModel
                    {
                        Type = "bool",
                        Index = i,
                        Name = GetCustomBoolName(inventory, i)!,
                        IsActive = true
                    });
                }
            }

            // File fields
            for (int i = 1; i <= 3; i++)
            {
                if (GetCustomFileActive(inventory, i) && !string.IsNullOrEmpty(GetCustomFileName(inventory, i)))
                {
                    fields.Add(new ItemCustomFieldViewModel
                    {
                        Type = "file",
                        Index = i,
                        Name = GetCustomFileName(inventory, i)!,
                        IsActive = true
                    });
                }
            }

            return fields;
        }

        private List<ItemCustomFieldViewModel> GetCustomFieldsForEdit(Item item, Inventory inventory)
        {
            var fields = GetActiveCustomFieldsForForm(inventory);
            
            foreach (var field in fields)
            {
                switch (field.Type)
                {
                    case "string":
                        field.StringValue = GetCustomStringValue(item, field.Index);
                        break;
                    case "text":
                        field.TextValue = GetCustomTextValue(item, field.Index);
                        break;
                    case "number":
                        field.NumberValue = GetCustomNumberValue(item, field.Index);
                        break;
                    case "bool":
                        field.BoolValue = GetCustomBoolValue(item, field.Index);
                        break;
                    case "file":
                        field.FileValue = GetCustomFileValue(item, field.Index);
                        break;
                }
            }
            
            return fields;
        }

        private void SetItemCustomFieldValues(Item item, List<ItemCustomFieldViewModel> customFields)
        {
            foreach (var field in customFields)
            {
                switch (field.Type)
                {
                    case "string":
                        SetCustomStringValue(item, field.Index, field.StringValue);
                        break;
                    case "text":
                        SetCustomTextValue(item, field.Index, field.TextValue);
                        break;
                    case "number":
                        SetCustomNumberValue(item, field.Index, field.NumberValue);
                        break;
                    case "bool":
                        SetCustomBoolValue(item, field.Index, field.BoolValue);
                        break;
                    case "file":
                        SetCustomFileValue(item, field.Index, field.FileValue);
                        break;
                }
            }
        }

        private ItemViewModel MapItemToViewModel(Item item, Inventory inventory, bool canEdit)
        {
            var viewModel = new ItemViewModel
            {
                Id = item.Id,
                InventoryId = item.InventoryId,
                CustomId = item.CustomId,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                Version = item.Version,
                CanEdit = canEdit,
                CreatorName = inventory.Creator.FullName
            };

            // Map custom field values
            var activeFields = GetActiveCustomFieldsForForm(inventory);
            foreach (var field in activeFields)
            {
                var fieldWithValue = new ItemCustomFieldViewModel
                {
                    Type = field.Type,
                    Index = field.Index,
                    Name = field.Name,
                    IsActive = true
                };

                switch (field.Type)
                {
                    case "string":
                        fieldWithValue.StringValue = GetCustomStringValue(item, field.Index);
                        break;
                    case "text":
                        fieldWithValue.TextValue = GetCustomTextValue(item, field.Index);
                        break;
                    case "number":
                        fieldWithValue.NumberValue = GetCustomNumberValue(item, field.Index);
                        break;
                    case "bool":
                        fieldWithValue.BoolValue = GetCustomBoolValue(item, field.Index);
                        break;
                    case "file":
                        fieldWithValue.FileValue = GetCustomFileValue(item, field.Index);
                        break;
                }

                viewModel.CustomFields.Add(fieldWithValue);
            }

            return viewModel;
        }

        private List<string> GetColumnHeaders(Inventory inventory)
        {
            var headers = new List<string> { "Custom ID" };
            
            var activeFields = GetActiveCustomFieldsForForm(inventory);
            headers.AddRange(activeFields.Select(f => f.Name));
            
            headers.Add("Actions");
            return headers;
        }

        private async Task UpdateInventoryItemCount(int inventoryId)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory != null)
            {
                inventory.ItemCount = await _context.Items.CountAsync(i => i.InventoryId == inventoryId);
                inventory.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // Helper methods for getting custom field values (Inventory)
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

        // Helper methods for setting custom field values (Inventory)
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

        // Helper methods for getting and setting Item custom field values
        private string? GetCustomStringValue(Item item, int index) => index switch
        {
            1 => item.CustomString1Value,
            2 => item.CustomString2Value,
            3 => item.CustomString3Value,
            _ => null
        };

        private void SetCustomStringValue(Item item, int index, string? value)
        {
            switch (index)
            {
                case 1:
                    item.CustomString1Value = value;
                    break;
                case 2:
                    item.CustomString2Value = value;
                    break;
                case 3:
                    item.CustomString3Value = value;
                    break;
            }
        }

        private string? GetCustomTextValue(Item item, int index) => index switch
        {
            1 => item.CustomText1Value,
            2 => item.CustomText2Value,
            3 => item.CustomText3Value,
            _ => null
        };

        private void SetCustomTextValue(Item item, int index, string? value)
        {
            switch (index)
            {
                case 1:
                    item.CustomText1Value = value;
                    break;
                case 2:
                    item.CustomText2Value = value;
                    break;
                case 3:
                    item.CustomText3Value = value;
                    break;
            }
        }

        private decimal? GetCustomNumberValue(Item item, int index) => index switch
        {
            1 => item.CustomNumber1Value,
            2 => item.CustomNumber2Value,
            3 => item.CustomNumber3Value,
            _ => null
        };

        private void SetCustomNumberValue(Item item, int index, decimal? value)
        {
            switch (index)
            {
                case 1:
                    item.CustomNumber1Value = value;
                    break;
                case 2:
                    item.CustomNumber2Value = value;
                    break;
                case 3:
                    item.CustomNumber3Value = value;
                    break;
            }
        }

        private bool? GetCustomBoolValue(Item item, int index) => index switch
        {
            1 => item.CustomBool1Value,
            2 => item.CustomBool2Value,
            3 => item.CustomBool3Value,
            _ => null
        };

        private void SetCustomBoolValue(Item item, int index, bool? value)
        {
            switch (index)
            {
                case 1:
                    item.CustomBool1Value = value;
                    break;
                case 2:
                    item.CustomBool2Value = value;
                    break;
                case 3:
                    item.CustomBool3Value = value;
                    break;
            }
        }

        private string? GetCustomFileValue(Item item, int index) => index switch
        {
            1 => item.CustomFile1Value,
            2 => item.CustomFile2Value,
            3 => item.CustomFile3Value,
            _ => null
        };

        private void SetCustomFileValue(Item item, int index, string? value)
        {
            switch (index)
            {
                case 1:
                    item.CustomFile1Value = value;
                    break;
                case 2:
                    item.CustomFile2Value = value;
                    break;
                case 3:
                    item.CustomFile3Value = value;
                    break;
            }
        }
    }
}