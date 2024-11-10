using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace INVApp.Models
{
    public class Product
    {
        [PrimaryKey, AutoIncrement]
        public int ProductID { get; set; }

        public string ProductName { get; set; }
        public string BrandName {  get; set; }
        public string ProductWeight { get; set; }
        public string Category { get; set; }

        public int CurrentStockLevel { get; set; }
        public int MinimumStockLevel { get; set; }
        public decimal Price { get; set; }
        public decimal WholesalePrice { get; set; }


        public string EAN13Barcode { get; set; }

    }
}
