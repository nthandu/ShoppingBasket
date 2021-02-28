using System;
using System.Linq;
using ShoppingBasket.Services.Interfaces;
using ShoppingBasket.Services.Models;

namespace ShoppingBasket.Services
{
    public class BasketService : IBasketService
    {
        private static Models.ShoppingBasket _shoppingBasket;

        public BasketService()
        {
            // Inject any dependencies here such as services for product info, orders, users etc.
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
                    AddLineItemToBasket(basket, new BasketLineItem() {
                        Quantity = 1,
                        ProductSku = basketItem.ProductSku,
                        LineItemPrice = basketItem.ItemPrice,
                        LineItemDiscount = basketItem.ItemDisount
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

        private static Models.ShoppingBasket GetBasket()
        {
            if(_shoppingBasket == null)
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
            if(basket != null &&
                basket.LineItems != null &&
                basket.LineItems.Any())
            {
                basket.ActualPrice = basket.LineItems.Sum(item => item.LineItemPrice);
                basket.TotalDiscount = basket.LineItems.Sum(item => item.LineItemDiscount);
                basket.TotalPrice = basket.ActualPrice - basket.TotalDiscount;
                basket.NumberOfItems = basket.LineItems.Sum(item => item.Quantity);
            }
        }
    }
}
