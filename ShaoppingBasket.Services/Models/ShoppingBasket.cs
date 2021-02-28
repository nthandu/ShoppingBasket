using System.Collections.Generic;

namespace ShoppingBasket.Services.Models
{
    public class ShoppingBasket
    {
        public ShoppingBasket()
        {
            LineItems = new List<BasketLineItem>();
        }

        /// <summary>
        /// Rettunrs list of line items.
        /// </summary>
        public IList<BasketLineItem> LineItems { get; private set; }

        /// <summary>
        /// Price of the basket after applying the discounts.
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Price of the basket before applying any discount.
        /// </summary>
        public decimal ActualPrice { get; set; }

        /// <summary>
        /// Total discount on the basket.
        /// </summary>
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Total number of items in the basket.
        /// </summary>
        public int NumberOfItems { get; set; }

        public bool VoucherApplied { get; set; }

        public decimal VoucherDiscount { get; set; }
    }
}
