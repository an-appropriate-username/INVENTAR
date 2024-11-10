using System;
using System.Threading.Tasks;
using INVApp.Models;

namespace INVApp.Services
{
    public class ChangeLogService
    {
        #region Fields

        private readonly DatabaseService _databaseService;

        #endregion

        #region Constructor

        public ChangeLogService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// This is the logic for the changelog being generated (excluding inital log).
        /// </summary>
        /// <param name="oldProduct">The original state of the product before changes.</param>
        /// <param name="existingProduct">The updated product with changes.</param>
        /// <param name="stockAdjustment">Difference in stock levels (positive or negative).</param>
        public async Task ProductUpdateChangeLogAsync(Product oldProduct, Product existingProduct, int stockAdjustment)
        {
            // Create an InventoryLog entry to track changes
            var log = new InventoryLog
            {
                ProductID = oldProduct.ProductID,
                ChangeType = string.Empty,
                Timestamp = DateTime.Now,

                NameOldValue = oldProduct.ProductName,
                NameNewValue = existingProduct.ProductName,

                CategoryOldValue = oldProduct.Category,
                CategoryNewValue = existingProduct.Category,

                PriceOldValue = oldProduct.Price,
                PriceNewValue = existingProduct.Price,

                WholesalePriceOldValue = oldProduct.WholesalePrice,
                WholesalePriceNewValue = existingProduct.WholesalePrice,

                StockOldValue = oldProduct.CurrentStockLevel.ToString(),
                StockNewValue = existingProduct.CurrentStockLevel.ToString(),
            };

            // Detect and log product changes
            DetectProductChanges(oldProduct, existingProduct, stockAdjustment, log);

            // If changes exist, trim and save the log
            if (!string.IsNullOrEmpty(log.ChangeType))
            {
                log.ChangeType = log.ChangeType.TrimEnd(',', ' ');
                await _databaseService.AddInventoryLogAsync(log);
            }
        }

        /// <summary>
        /// Helper method to detect changes and categorize them.
        /// </summary>
        private void DetectProductChanges(Product oldProduct, Product existingProduct, int stockAdjustment, InventoryLog log)
        {
            if (oldProduct.ProductName != existingProduct.ProductName)
            {
                log.ChangeType += "Name Changed, ";
            }

            if (oldProduct.Category != existingProduct.Category)
            {
                log.ChangeType += "Category Changed, ";
            }

            if (oldProduct.Price != existingProduct.Price)
            {
                log.ChangeType += "Price Changed, ";
            }

            if (oldProduct.WholesalePrice != existingProduct.WholesalePrice)
            {
                log.ChangeType += "Wholesale Price Changed, ";
            }

            // Handle stock changes
            if (oldProduct.CurrentStockLevel != existingProduct.CurrentStockLevel)
            {
                if (stockAdjustment > 0)
                {
                    log.ChangeType += "Stock Increased, ";
                }
                else if (stockAdjustment < 0)
                {
                    log.ChangeType += "Stock Decreased, ";
                }
                else
                {
                    log.ChangeType += "Stock Changed, ";
                }
            }
        }

        #endregion
    }
}
