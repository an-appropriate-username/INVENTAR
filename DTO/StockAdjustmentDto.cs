using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.DTO
{
    public class StockAdjustmentDto
    {
        public int? Id { get; set; }
        public string Ean13Barcode { get; set; } = string.Empty;
        public int StockAdjustment { get; set; }  // Remove internal
        public string? ReductionReason { get; set; }
        public string? Name { get; set; }  // Match API property names
        public string? BrandName { get; set; }
        public string? Weight { get; set; }
        public string? CategoryName { get; set; }
        public decimal? WholesalePrice { get; set; }
        public decimal? Price { get; set; }
    }
}
