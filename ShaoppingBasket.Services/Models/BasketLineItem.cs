using System;
namespace ShoppingBasket.Services.Models
{
    /// <summary>
    /// Represens a line item in the basket belonging to a product..
    /// </summary>
    public class BasketLineItem
    {
        public Guid ProductSku { get; set; }

        public int Quantity { get; set; }

        public decimal LineItemPrice { get; set; }

        public decimal LineItemDiscount { get; set; }

        public bool IsGiftVoucherItem { get; set; }
    }
}
