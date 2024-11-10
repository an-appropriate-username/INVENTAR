using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Models
{
    public class InventoryLog
    {
        [PrimaryKey, AutoIncrement]
        public int LogID { get; set; }

        // Product ID and Change Type
        public int ProductID { get; set; }
        public string? ChangeType { get; set; }
        public string? ChangeTypeFormatted => string.Join(Environment.NewLine, ChangeType.Split(','));

        // Stock
        public string? StockOldValue { get; set; }
        public string? StockNewValue { get; set; }
        public string? ReductionReason { get; set; }
        // Name
        public string? NameOldValue { get; set; }
        public string? NameNewValue { get; set; }

        public string? BrandNewValue { get; set; }
        public string? BrandOldValue { get; set; }
        // Category
        public string? CategoryOldValue { get; set; }
        public string? CategoryNewValue { get; set; }
        // Customer Price
        public decimal PriceOldValue { get; set; }
        public decimal PriceNewValue { get; set; }
        // Wholesale price
        public decimal WholesalePriceOldValue { get; set; }
        public decimal WholesalePriceNewValue { get; set; }

        // User and Date
        public int? UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
