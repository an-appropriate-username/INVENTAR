using INVApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Services
{
    public class StockService
    {
        private readonly DatabaseService _databaseService;

        public StockService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task DecrementStock(ObservableCollection<CartItem> cart)
        {
            foreach (var item in cart)
            {
                var product = item.Product;
                if (product != null)
                {
                    product.CurrentStockLevel -= item.Quantity;
                    if (product.CurrentStockLevel < 0) { product.CurrentStockLevel = 0; }
                    await _databaseService.DecrementProductAsync(product);
                }
            }
        }
    }
}
