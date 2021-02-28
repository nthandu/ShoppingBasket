using System;
using NUnit.Framework;

namespace ShoppingBasket.Services.Tests
{
    [TestFixture]
    public class BaskeServiceTests
    {
        private BasketService _sut;
        private static Guid _productSku = Guid.Parse("33d399f6-6366-476b-8353-9dca5a8a3f7a");

        [SetUp]
        public void SetUp()
        {
            _sut = new BasketService();
        }

        [TearDown]
        public void TearDown()
        {
            _sut.ResetBasket();
            _sut = null;
        }

        [Test]
        public void GetCurrentBasket_ReturnsBasket()
        {
            var shoppingBasket = _sut.GetCurrentBasket();
            Assert.NotNull(shoppingBasket);
        }

        [Test]
        public void AddItemToBasket_UpdatesBasket_WhenItemIsNotNull()
        {
            var basketItem = GetBasketItem();
            var shoppingBasket = _sut.GetCurrentBasket();
            var numberOfItems = shoppingBasket.LineItems.Count;
            _sut.AddItemToBasket(basketItem);
            Assert.AreEqual(numberOfItems + 1, shoppingBasket.LineItems.Count);
        }

        [Test]
        public void AddItemToBasket_ShouldNotUpdateBasket_WhenItemIsNull()
        {
            Models.BasketItem basketItem = null;
            var shoppingBasket = _sut.GetCurrentBasket();
            var numberOfItems = shoppingBasket.LineItems.Count;
            _sut.AddItemToBasket(basketItem);
            Assert.AreEqual(numberOfItems, shoppingBasket.LineItems.Count);
        }

        [Test]
        public void AddItemToBasket_UpdatesLineItemInTheBasket_WhenItemWithSameSkuAlreadyExisting()
        {
            // Arrange
            var basketItem = GetBasketItem();
            var shoppingBasket = _sut.GetCurrentBasket();
            _sut.AddItemToBasket(basketItem);
            var numberOfItems = shoppingBasket.LineItems.Count;

            // Act
            _sut.AddItemToBasket(basketItem);

            // Assert
            Assert.AreEqual(numberOfItems, shoppingBasket.LineItems.Count);
        }

        [Test]
        public void GetLineItemBySku_ReturnsNull_WhenProductSkuNotExists()
        {
            var basketLineItem = _sut.GetLineItemBySku(Guid.NewGuid());
            Assert.Null(basketLineItem);
        }

        [Test]
        public void GetLineItemBySku_ReturnsBasketItem_WhenProductSkuExists()
        {
            // Arrange
            var basketItem = GetBasketItem();
            _sut.AddItemToBasket(basketItem);

            // Act
            var basketLineItem = _sut.GetLineItemBySku(_productSku);

            // Assert
            Assert.NotNull(basketLineItem);
            Assert.AreEqual(_productSku, basketLineItem.ProductSku);
        }

        [Test]
        public void AddItemToBasket_Updates_BasketPrice()
        {
            var basketItem = GetBasketItem(3.0m, 1.0m);
            _sut.AddItemToBasket(basketItem);
            var basket = _sut.GetCurrentBasket();
            Assert.AreEqual(3.0m, basket.ActualPrice);
            Assert.AreEqual(2.0m, basket.TotalPrice);
            Assert.AreEqual(1.0m, basket.TotalDiscount);
        }

        private static Models.BasketItem GetBasketItem(decimal itemPrice = 2.5m, decimal discount = 0.0m)
        {
            return new Models.BasketItem()
            {
                ProductSku = _productSku,
                ItemPrice = itemPrice,
                ItemDisount = discount
            };
        }
    }
}
