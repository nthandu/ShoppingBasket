using System;
namespace ShoppingBasket.Services.Models
{
    public class Voucher
    {
        public string Code { get; set; }

        public decimal Discount { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}
