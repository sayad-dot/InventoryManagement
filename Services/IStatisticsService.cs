using InventoryManagement.Data;
using InventoryManagement.Models;
using InventoryManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Services
{
    public interface IStatisticsService
    {
        Task<StatisticsViewModel> GetInventoryStatisticsAsync(int inventoryId);
        Task<List<FieldStatViewModel>> GetFieldStatisticsAsync(int inventoryId);
        Task<List<PopularItemViewModel>> GetPopularItemsAsync(int inventoryId, int count = 5);
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StatisticsViewModel> GetInventoryStatisticsAsync(int inventoryId)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                return new StatisticsViewModel();

            var statistics = new StatisticsViewModel
            {
                InventoryId = inventory.Id,
                InventoryTitle = inventory.Title,
                TotalItems = inventory.ItemCount,
                TotalLikes = await _context.ItemLikes.CountAsync(il => il.Item.InventoryId == inventoryId),
                TotalDiscussions = await _context.Discussions.CountAsync(d => d.InventoryId == inventoryId),
                UniqueContributors = await GetUniqueContributorsAsync(inventoryId),
                LastItemAdded = await GetLastItemAddedAsync(inventoryId),
                LastDiscussion = await GetLastDiscussionAsync(inventoryId),
                FieldStats = await GetFieldStatisticsAsync(inventoryId),
                PopularItems = await GetPopularItemsAsync(inventoryId, 5)
            };

            return statistics;
        }

        public async Task<List<FieldStatViewModel>> GetFieldStatisticsAsync(int inventoryId)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null) return new List<FieldStatViewModel>();

            var fieldStats = new List<FieldStatViewModel>();
            var items = await _context.Items.Where(i => i.InventoryId == inventoryId).ToListAsync();

            // Analyze string fields
            for (int i = 1; i <= 3; i++)
            {
                if (GetCustomStringActive(inventory, i))
                {
                    var fieldName = GetCustomStringName(inventory, i);
                    var values = items.Select(item => GetCustomStringValue(item, i))
                                    .Where(v => !string.IsNullOrEmpty(v))
                                    .Cast<string>()
                                    .ToList();

                    fieldStats.Add(new FieldStatViewModel
                    {
                        FieldName = fieldName ?? $"String Field {i}",
                        FieldType = "string",
                        DataType = "Text",
                        CommonValues = GetCommonValues(values, 5),
                        UniqueValues = values.Distinct().Count(),
                        EmptyCount = items.Count - values.Count
                    });
                }
            }

            // Analyze text fields
            for (int i = 1; i <= 3; i++)
            {
                if (GetCustomTextActive(inventory, i))
                {
                    var fieldName = GetCustomTextName(inventory, i);
                    var values = items.Select(item => GetCustomTextValue(item, i))
                                    .Where(v => !string.IsNullOrEmpty(v))
                                    .Cast<string>()
                                    .ToList();

                    fieldStats.Add(new FieldStatViewModel
                    {
                        FieldName = fieldName ?? $"Text Field {i}",
                        FieldType = "text",
                        DataType = "Long Text",
                        CommonValues = GetCommonValues(values, 3), // Show fewer for long text
                        UniqueValues = values.Distinct().Count(),
                        EmptyCount = items.Count - values.Count
                    });
                }
            }

            // Analyze number fields
            for (int i = 1; i <= 3; i++)
            {
                if (GetCustomNumberActive(inventory, i))
                {
                    var fieldName = GetCustomNumberName(inventory, i);
                    var values = items.Select(item => GetCustomNumberValue(item, i))
                                    .Where(v => v.HasValue)
                                    .Select(v => v!.Value)
                                    .ToList();

                    if (values.Any())
                    {
                        fieldStats.Add(new FieldStatViewModel
                        {
                            FieldName = fieldName ?? $"Number Field {i}",
                            FieldType = "number",
                            DataType = "Numeric",
                            MinValue = values.Min(),
                            MaxValue = values.Max(),
                            AverageValue = values.Average(),
                            SumValue = values.Sum(),
                            EmptyCount = items.Count - values.Count
                        });
                    }
                }
            }

            // Analyze boolean fields
            for (int i = 1; i <= 3; i++)
            {
                if (GetCustomBoolActive(inventory, i))
                {
                    var fieldName = GetCustomBoolName(inventory, i);
                    var values = items.Select(item => GetCustomBoolValue(item, i))
                                    .Where(v => v.HasValue)
                                    .Select(v => v!.Value)
                                    .ToList();

                    if (values.Any())
                    {
                        fieldStats.Add(new FieldStatViewModel
                        {
                            FieldName = fieldName ?? $"Boolean Field {i}",
                            FieldType = "bool",
                            DataType = "True/False",
                            TrueCount = values.Count(v => v),
                            FalseCount = values.Count(v => !v),
                            EmptyCount = items.Count - values.Count
                        });
                    }
                }
            }

            return fieldStats;
        }

        public async Task<List<PopularItemViewModel>> GetPopularItemsAsync(int inventoryId, int count = 5)
        {
            var popularItems = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .Select(i => new
                {
                    i.Id,
                    i.CustomId,
                    LikeCount = i.Likes.Count,
                    DisplayValue = i.CustomString1Value ?? i.CustomId ?? $"Item {i.Id}"
                })
                .OrderByDescending(i => i.LikeCount)
                .ThenByDescending(i => i.Id)
                .Take(count)
                .Select(i => new PopularItemViewModel
                {
                    ItemId = i.Id,
                    CustomId = i.CustomId ?? string.Empty,
                    LikeCount = i.LikeCount,
                    DisplayValue = i.DisplayValue
                })
                .ToListAsync();

            return popularItems;
        }

        private async Task<int> GetUniqueContributorsAsync(int inventoryId)
        {
            var itemContributors = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .Select(i => i.Inventory.CreatorId)
                .Distinct()
                .CountAsync();

            var discussionContributors = await _context.Discussions
                .Where(d => d.InventoryId == inventoryId)
                .Select(d => d.UserId)
                .Distinct()
                .CountAsync();

            var likeContributors = await _context.ItemLikes
                .Where(il => il.Item.InventoryId == inventoryId)
                .Select(il => il.UserId)
                .Distinct()
                .CountAsync();

            // Combine and get unique contributors
            var allContributors = new HashSet<string>();
            
            // Add item creators
            var itemCreators = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .Select(i => i.Inventory.CreatorId)
                .Distinct()
                .ToListAsync();
            itemCreators.ForEach(id => allContributors.Add(id));

            // Add discussion contributors
            var discussionUsers = await _context.Discussions
                .Where(d => d.InventoryId == inventoryId)
                .Select(d => d.UserId)
                .Distinct()
                .ToListAsync();
            discussionUsers.ForEach(id => allContributors.Add(id));

            // Add like contributors
            var likeUsers = await _context.ItemLikes
                .Where(il => il.Item.InventoryId == inventoryId)
                .Select(il => il.UserId)
                .Distinct()
                .ToListAsync();
            likeUsers.ForEach(id => allContributors.Add(id));

            return allContributors.Count;
        }

        private async Task<DateTime> GetLastItemAddedAsync(int inventoryId)
        {
            var lastItem = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();

            return lastItem?.CreatedAt ?? DateTime.MinValue;
        }

        private async Task<DateTime> GetLastDiscussionAsync(int inventoryId)
        {
            var lastDiscussion = await _context.Discussions
                .Where(d => d.InventoryId == inventoryId)
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefaultAsync();

            return lastDiscussion?.CreatedAt ?? DateTime.MinValue;
        }

        private List<ValueCountViewModel> GetCommonValues(List<string> values, int topCount)
        {
            return values.GroupBy(v => v)
                        .OrderByDescending(g => g.Count())
                        .Take(topCount)
                        .Select(g => new ValueCountViewModel
                        {
                            Value = g.Key.Length > 50 ? g.Key.Substring(0, 50) + "..." : g.Key,
                            Count = g.Count(),
                            Percentage = (double)g.Count() / values.Count * 100
                        })
                        .ToList();
        }

        // Helper methods for accessing custom fields (same as in InventoryController)
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

        // Helper methods for getting Item custom field values
        private string? GetCustomStringValue(Item item, int index) => index switch
        {
            1 => item.CustomString1Value,
            2 => item.CustomString2Value,
            3 => item.CustomString3Value,
            _ => null
        };

        private string? GetCustomTextValue(Item item, int index) => index switch
        {
            1 => item.CustomText1Value,
            2 => item.CustomText2Value,
            3 => item.CustomText3Value,
            _ => null
        };

        private decimal? GetCustomNumberValue(Item item, int index) => index switch
        {
            1 => item.CustomNumber1Value,
            2 => item.CustomNumber2Value,
            3 => item.CustomNumber3Value,
            _ => null
        };

        private bool? GetCustomBoolValue(Item item, int index) => index switch
        {
            1 => item.CustomBool1Value,
            2 => item.CustomBool2Value,
            3 => item.CustomBool3Value,
            _ => null
        };
    }
}