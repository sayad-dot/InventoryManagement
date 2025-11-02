namespace InventoryManagement.Models
{
    /// <summary>
    /// Represents an element in a custom ID format
    /// </summary>
    public class CustomIdElement
    {
        public string Type { get; set; } = string.Empty; // "fixed", "random20", "random32", "random6", "random9", "guid", "datetime", "sequence"
        public string Value { get; set; } = string.Empty; // For fixed text or format string for datetime
        public int Order { get; set; } // Display order
        public string? Options { get; set; } // JSON string for additional options (e.g., leading zeros for sequence)
    }

    /// <summary>
    /// Configuration for Custom ID format
    /// </summary>
    public class CustomIdFormatConfig
    {
        public List<CustomIdElement> Elements { get; set; } = new List<CustomIdElement>();
        public string Separator { get; set; } = "-"; // Default separator between elements
    }

    /// <summary>
    /// Enum for Custom ID Element Types
    /// </summary>
    public enum CustomIdElementType
    {
        FixedText,      // Fixed text string
        Random20Bit,    // 20-bit random number (0-1048575)
        Random32Bit,    // 32-bit random number (0-4294967295)
        Random6Digit,   // 6-digit random number (000000-999999)
        Random9Digit,   // 9-digit random number (000000000-999999999)
        Guid,           // Full GUID
        DateTime,       // Date/time with custom format
        Sequence        // Auto-incrementing sequence per inventory
    }
}
