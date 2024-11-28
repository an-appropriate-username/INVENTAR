using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.DTO
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
        public string Weight { get; set; }
        public int CategoryId { get; set; }
        public CategoryDto Category { get; set; }
        public int CurrentStockLevel { get; set; }
        public int MinimumStockLevel { get; set; }
        public decimal Price { get; set; }
        public decimal WholesalePrice { get; set; }
        public string EaN13Barcode { get; set; }
        public string? ImagePath { get; set; }
    }
}
