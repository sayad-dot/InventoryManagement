using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace InventoryManagement.Services
{
    /// <summary>
    /// Service for generating custom IDs based on configured format
    /// </summary>
    public class CustomIdGeneratorService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomIdGeneratorService> _logger;

        public CustomIdGeneratorService(
            ApplicationDbContext context,
            ILogger<CustomIdGeneratorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Generate a custom ID for an item based on inventory's format configuration
        /// </summary>
        public async Task<string> GenerateCustomIdAsync(int inventoryId)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null)
            {
                throw new ArgumentException($"Inventory {inventoryId} not found");
            }

            // If no custom format is defined, return empty string (manual entry required)
            if (string.IsNullOrWhiteSpace(inventory.CustomIdFormat))
            {
                return string.Empty;
            }

            try
            {
                var config = JsonSerializer.Deserialize<CustomIdFormatConfig>(inventory.CustomIdFormat);
                if (config == null || !config.Elements.Any())
                {
                    return string.Empty;
                }

                var parts = new List<string>();
                foreach (var element in config.Elements.OrderBy(e => e.Order))
                {
                    var part = await GenerateElementValueAsync(element, inventoryId);
                    parts.Add(part);
                }

                return string.Join(config.Separator ?? "-", parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating custom ID for inventory {InventoryId}", inventoryId);
                return string.Empty;
            }
        }

        /// <summary>
        /// Generate value for a single element
        /// </summary>
        private async Task<string> GenerateElementValueAsync(CustomIdElement element, int inventoryId)
        {
            return element.Type.ToLower() switch
            {
                "fixed" => element.Value ?? "",
                "random20" => GenerateRandom20Bit(),
                "random32" => GenerateRandom32Bit(),
                "random6" => GenerateRandom6Digit(),
                "random9" => GenerateRandom9Digit(),
                "guid" => Guid.NewGuid().ToString("N").ToUpper(), // No hyphens, uppercase
                "datetime" => GenerateDateTime(element.Value),
                "sequence" => await GenerateSequenceAsync(inventoryId, element),
                _ => ""
            };
        }

        /// <summary>
        /// Generate 20-bit random number (0-1048575)
        /// </summary>
        private string GenerateRandom20Bit()
        {
            var value = RandomNumberGenerator.GetInt32(0, 1048576); // 2^20
            return value.ToString("X5"); // 5 hex digits
        }

        /// <summary>
        /// Generate 32-bit random number (0-4294967295)
        /// </summary>
        private string GenerateRandom32Bit()
        {
            var bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            var value = BitConverter.ToUInt32(bytes, 0);
            return value.ToString("X8"); // 8 hex digits
        }

        /// <summary>
        /// Generate 6-digit random number (000000-999999)
        /// </summary>
        private string GenerateRandom6Digit()
        {
            var value = RandomNumberGenerator.GetInt32(0, 1000000);
            return value.ToString("D6"); // 6 digits with leading zeros
        }

        /// <summary>
        /// Generate 9-digit random number (000000000-999999999)
        /// </summary>
        private string GenerateRandom9Digit()
        {
            var value = RandomNumberGenerator.GetInt32(0, 1000000000);
            return value.ToString("D9"); // 9 digits with leading zeros
        }

        /// <summary>
        /// Generate date/time string with custom format
        /// </summary>
        private string GenerateDateTime(string? format)
        {
            var now = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(format))
            {
                return now.ToString("yyyyMMdd"); // Default: 20251102
            }

            try
            {
                return now.ToString(format);
            }
            catch
            {
                return now.ToString("yyyyMMdd");
            }
        }

        /// <summary>
        /// Generate auto-incrementing sequence number for the inventory
        /// </summary>
        private async Task<string> GenerateSequenceAsync(int inventoryId, CustomIdElement element)
        {
            // Get the highest sequence number used in this inventory
            var maxCustomId = await _context.Items
                .Where(i => i.InventoryId == inventoryId && i.CustomId != null)
                .OrderByDescending(i => i.Id)
                .Select(i => i.CustomId)
                .FirstOrDefaultAsync();

            int nextSequence = 1;

            // Try to extract existing sequence numbers if possible
            // This is a simple implementation - in production you might want a dedicated sequence table
            var existingItems = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .CountAsync();
            
            nextSequence = existingItems + 1;

            // Parse options for formatting (e.g., leading zeros)
            var options = ParseElementOptions(element.Options);
            var leadingZeros = options.GetValueOrDefault("leadingZeros", 3);

            return nextSequence.ToString($"D{leadingZeros}");
        }

        /// <summary>
        /// Parse element options from JSON string
        /// </summary>
        private Dictionary<string, int> ParseElementOptions(string? optionsJson)
        {
            if (string.IsNullOrWhiteSpace(optionsJson))
            {
                return new Dictionary<string, int>();
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, int>>(optionsJson) 
                    ?? new Dictionary<string, int>();
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Validate if a custom ID is unique within an inventory
        /// </summary>
        public async Task<bool> IsCustomIdUniqueAsync(int inventoryId, string customId, int? excludeItemId = null)
        {
            if (string.IsNullOrWhiteSpace(customId))
            {
                return true; // Empty IDs are allowed
            }

            var query = _context.Items
                .Where(i => i.InventoryId == inventoryId && i.CustomId == customId);

            if (excludeItemId.HasValue)
            {
                query = query.Where(i => i.Id != excludeItemId.Value);
            }

            return !await query.AnyAsync();
        }

        /// <summary>
        /// Generate a preview of the custom ID format
        /// </summary>
        public string GeneratePreview(string formatJson)
        {
            if (string.IsNullOrWhiteSpace(formatJson))
            {
                return "No format configured";
            }

            try
            {
                var config = JsonSerializer.Deserialize<CustomIdFormatConfig>(formatJson);
                if (config == null || !config.Elements.Any())
                {
                    return "No format configured";
                }

                var parts = new List<string>();
                foreach (var element in config.Elements.OrderBy(e => e.Order))
                {
                    var part = GeneratePreviewPart(element);
                    parts.Add(part);
                }

                return string.Join(config.Separator ?? "-", parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating preview");
                return "Invalid format";
            }
        }

        /// <summary>
        /// Generate a preview value for an element (not persisted)
        /// </summary>
        private string GeneratePreviewPart(CustomIdElement element)
        {
            return element.Type.ToLower() switch
            {
                "fixed" => element.Value ?? "",
                "random20" => "A1B2C",
                "random32" => "E74FA329",
                "random6" => "123456",
                "random9" => "987654321",
                "guid" => "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6",
                "datetime" => GenerateDateTime(element.Value),
                "sequence" => "001",
                _ => ""
            };
        }
    }
}
