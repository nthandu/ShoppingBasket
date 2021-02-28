using System;
namespace ShoppingBasket.Services.Models
{
    /// <summary>
    /// Represents a product to be added to basket.
    /// </summary>
    public class BasketItem
    {
        public Guid ProductSku { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// Item price before applying any discount.
        /// </summary>
        public decimal ItemPrice { get; set; }

        public decimal ItemDisount { get; set; }

        /// <summary>
        /// Will be set to true if it's sale item or a gift voucher item.
        /// </summary>
        public bool IsGiftVoucherItem { get; set; }
    }
}
