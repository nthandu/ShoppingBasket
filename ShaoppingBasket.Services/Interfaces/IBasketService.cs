using System;
using System.Threading.Tasks;
using ShoppingBasket.Services.Models;

namespace ShoppingBasket.Services.Interfaces
{
    public interface IBasketService
    {
        Models.ShoppingBasket GetCurrentBasket();
        void AddItemToBasket(BasketItem basketItem);
        BasketLineItem GetLineItemBySku(Guid productSku);
        void ResetBasket();
        Task ApplyVoucherAsync(string voucherCode);

        void UpdateQuantity(BasketUpdate updateOperation, Guid productSku, int quantity);
        void Delete(Guid productSku);
    }
}
