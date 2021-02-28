using System;
using System.Linq;
using System.Threading.Tasks;
using ShoppingBasket.Services.Interfaces;
using ShoppingBasket.Services.Models;

namespace ShoppingBasket.Services
{
    public class BasketService : IBasketService
    {
        private static Models.ShoppingBasket _shoppingBasket;
        private readonly IVoucherService _voucherService;

        public BasketService(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        public Models.ShoppingBasket GetCurrentBasket()
        {
            // in a production ready application we create a basket based on the user session
            // If user already created a basket, we persist the items to the storage.
            // And if user is coming back to the application, we initialise the basket using the items from storage.
            // For the test purpose, I am creating the basket instance if it's null otherwise using the same instance.
            return GetBasket();
        }

        public void AddItemToBasket(BasketItem basketItem)
        {
            if (basketItem != null)
            {
                var basket = GetCurrentBasket();
                BasketLineItem existingLineItem = GetLineItemBySku(basketItem.ProductSku);
                if (existingLineItem == null)
                {
                    AddLineItemToBasket(basket, new BasketLineItem()
                    {
                        Quantity = 1,
                        ProductSku = basketItem.ProductSku,
                        LineItemPrice = basketItem.ItemPrice,
                        LineItemDiscount = basketItem.ItemDisount,
                        IsGiftVoucherItem = basketItem.IsGiftVoucherItem
                    });
                }
                else
                {
                    existingLineItem.Quantity = existingLineItem.Quantity + 1;
                    existingLineItem.LineItemPrice = existingLineItem.LineItemPrice + basketItem.ItemPrice;
                    existingLineItem.LineItemDiscount = existingLineItem.LineItemDiscount + basketItem.ItemDisount;
                }
                UpdateBasket(basket);
            }
        }

        public BasketLineItem GetLineItemBySku(Guid productSku)
        {
            var basket = GetCurrentBasket();
            return basket?.LineItems.SingleOrDefault(item => item.ProductSku == productSku);
        }

        public void ResetBasket()
        {
            _shoppingBasket = null;
        }

        public void UpdateQuantity(BasketUpdate updateOperation, Guid productSku, int quantity)
        {
            // TODO: Implement
        }


        public void Delete(Guid productSku)
        {
            // TODO: Implement
        }

        public async Task ApplyVoucherAsync(string voucherCode)
        {
            var basket = GetCurrentBasket();
            if (basket.VoucherApplied == true)
            {
                return;
            }

            var voucher = await _voucherService.GetAsync(voucherCode);
            if (voucher != null &&
                voucher.ExpiryDate >= DateTime.UtcNow.Date)
            {
                var discountToBeApplied = voucher.Discount;
                if (basket.NumberOfItems > 0)
                {
                    var hasNonGiftItems = basket.LineItems.Any(li => li.IsGiftVoucherItem == false);

                    if (hasNonGiftItems)
                    {
                        var giftItemsTotal = basket.LineItems.Where(li => li.IsGiftVoucherItem).Sum(li => li.LineItemPrice);
                        var giftItemDiscount = basket.LineItems.Where(li => li.IsGiftVoucherItem).Sum(li => li.LineItemDiscount);

                        var nonGiftItemsTotal = basket.LineItems.Where(li => li.IsGiftVoucherItem == false).Sum(li => li.LineItemPrice);
                        var nonGiftItemDiscount = basket.LineItems.Where(li => li.IsGiftVoucherItem == false).Sum(li => li.LineItemDiscount);

                        var discountAfterApplyingVoucher = nonGiftItemDiscount + voucher.Discount;

                        var priceAfterApplyinVoucher = nonGiftItemsTotal - voucher.Discount;

                        if(priceAfterApplyinVoucher < 0)
                        {
                            priceAfterApplyinVoucher = 0;
                        }

                        basket.TotalDiscount = giftItemDiscount  + discountAfterApplyingVoucher;
                        basket.TotalPrice = giftItemsTotal + priceAfterApplyinVoucher - giftItemDiscount - nonGiftItemDiscount;

                        if (basket.TotalPrice < 0)
                        {
                            basket.TotalPrice = 0;
                        }

                        basket.VoucherApplied = true;
                        basket.VoucherDiscount = voucher.Discount;
                    }

                    //var notGiftedLineItemsPrice = basket.
                    //    LineItems.
                    //    Where(li => li.IsGiftVoucherItem == false).
                    //    Sum(li => li.LineItemPrice);
                    //var maxVoucherDisount = notGiftedLineItemsPrice - voucher.Discount;

                    //if(maxVoucherDisount > 0)
                    //{
                    //    basket.TotalDiscount = basket.TotalDiscount + voucher.Discount;
                    //    basket.TotalPrice = basket.TotalPrice - discountToBeApplied;
                    //}

                    //if (basket.TotalPrice < 0)
                    //{
                    //    basket.TotalPrice = 0;
                    //}
                    //basket.VoucherApplied = true;
                    //basket.VoucherDiscount = voucher.Discount;
                }
            }
        }

        private static Models.ShoppingBasket GetBasket()
        {
            if (_shoppingBasket == null)
            {
                _shoppingBasket = new Models.ShoppingBasket();
            }
            return _shoppingBasket;
        }

        private void AddLineItemToBasket(Models.ShoppingBasket basket, BasketLineItem basketItem)
        {
            basket.LineItems.Add(basketItem);
        }

        private void UpdateBasket(Models.ShoppingBasket basket)
        {
            if (basket != null &&
                basket.LineItems != null &&
                basket.LineItems.Any())
            {
                basket.ActualPrice = basket.LineItems.Sum(item => item.LineItemPrice);
                basket.TotalDiscount = basket.LineItems.Sum(item => item.LineItemDiscount) + basket.VoucherDiscount;
                basket.TotalPrice = basket.ActualPrice - basket.TotalDiscount;
                if (basket.TotalPrice < 0)
                {
                    basket.TotalPrice = 0;
                }
                basket.NumberOfItems = basket.LineItems.Sum(item => item.Quantity);
            }
        }
    }
}
