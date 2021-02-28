using System;
using NUnit.Framework;
using ShoppingBasket.Services.Interfaces;
using Moq;
using System.Threading.Tasks;

namespace ShoppingBasket.Services.Tests
{
    [TestFixture]
    public class BaskeServiceTests
    {
        private BasketService _sut;
        private static Guid _productSku = Guid.Parse("33d399f6-6366-476b-8353-9dca5a8a3f7a");
        private Mock<IVoucherService> _mockVoucherService;

        [SetUp]
        public void SetUp()
        {
            _mockVoucherService = new Mock<IVoucherService>();
            _mockVoucherService.
                Setup(s => s.GetAsync(Models.Constants.VoucherCode5)).
                ReturnsAsync(new Models.Voucher()
                {
                    Code = Models.Constants.VoucherCode5,
                    Discount = 5.0m,
                    ExpiryDate = DateTime.UtcNow.AddDays(360)
                });
            _mockVoucherService.
                Setup(s => s.GetAsync(Models.Constants.VoucherCode1)).
                ReturnsAsync(new Models.Voucher()
                {
                    Code = Models.Constants.VoucherCode5,
                    Discount = 1.0m,
                    ExpiryDate = DateTime.UtcNow.AddDays(360)
                });
            _mockVoucherService.
                Setup(s => s.GetAsync(Models.Constants.VoucherCode50)).
                ReturnsAsync(new Models.Voucher()
                {
                    Code = Models.Constants.VoucherCode50,
                    Discount = 50.0m,
                    ExpiryDate = DateTime.UtcNow.AddDays(360)
                });
            _sut = new BasketService(_mockVoucherService.Object);
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
            var basketItem = GetBasketItem(_productSku);
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
            var basketItem = GetBasketItem(_productSku);
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
            var basketItem = GetBasketItem(_productSku);
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
            var basketItem = GetBasketItem(_productSku, 3.0m, 1.0m);
            _sut.AddItemToBasket(basketItem);
            var basket = _sut.GetCurrentBasket();
            Assert.AreEqual(3.0m, basket.ActualPrice);
            Assert.AreEqual(2.0m, basket.TotalPrice);
            Assert.AreEqual(1.0m, basket.TotalDiscount);
        }

        [Test]
        public async Task ApplyVoucherAsync_Calls_GetAsync()
        {
            await _sut.ApplyVoucherAsync(Models.Constants.VoucherCode5);
            _mockVoucherService.Verify(s => s.GetAsync(Models.Constants.VoucherCode5), Times.Once);
        }

        [Test]
        public async Task ApplyVoucherAsync_Updates_BasketTotalPrice_WhenVoucherIsValid()
        {
            // Arrange
            var basketItem = GetBasketItem(Guid.NewGuid(), 8.0m, 1.0m);
            _sut.AddItemToBasket(basketItem);

            // Act
            await _sut.ApplyVoucherAsync(Models.Constants.VoucherCode5);

            var basket = _sut.GetCurrentBasket();

            // Assert
            Assert.AreEqual(8.0m, basket.ActualPrice);
            Assert.AreEqual(2.0m, basket.TotalPrice);
            Assert.AreEqual(6.0m, basket.TotalDiscount);
        }

        [Test]
        public async Task ApplyVoucherAsync_DoesNotApplyVoucher_When_VoucherAlreadyApplied()
        {
            // Arrange
            var basketItem = GetBasketItem(Guid.NewGuid(), 8.0m, 1.0m);
            _sut.AddItemToBasket(basketItem);

            // Act
            await _sut.ApplyVoucherAsync(Models.Constants.VoucherCode5);
            await _sut.ApplyVoucherAsync(Models.Constants.VoucherCode1);
            var basket = _sut.GetCurrentBasket();

            // Assert
            Assert.AreEqual(8.0m, basket.ActualPrice);
            Assert.AreEqual(2.0m, basket.TotalPrice);
            Assert.AreEqual(6.0m, basket.TotalDiscount);
        }

        [Test]
        public async Task ApplyVoucherAsync_SetBasketTotalToZero_When_VoucherDicountGreaterThanBasketTotal()
        {
            // Arrange
            var basketItem = GetBasketItem(Guid.NewGuid(), 3.0m, 0.0m);
            _sut.AddItemToBasket(basketItem);

            // Act
            await _sut.ApplyVoucherAsync(Models.Constants.VoucherCode5);
            var basket = _sut.GetCurrentBasket();

            // Assert
            Assert.AreEqual(3.0m, basket.ActualPrice);
            Assert.AreEqual(0.0m, basket.TotalPrice);
            Assert.AreEqual(5.0m, basket.TotalDiscount);
        }


        [Test]
        public async Task ApplyVoucherAsync_NotAppliesDiscount_When_ItemsAreGiftVoucherItems()
        {
            // Arrange
            var basketItem = GetBasketItem(Guid.NewGuid(), 8.0m, 0.0m, true);
            _sut.AddItemToBasket(basketItem);

            // Act
            await _sut.ApplyVoucherAsync(Models.Constants.VoucherCode5);
            var basket = _sut.GetCurrentBasket();

            // Assert
            Assert.AreEqual(8.0m, basket.ActualPrice);
            Assert.AreEqual(8.0m, basket.TotalPrice);
            Assert.AreEqual(0.0m, basket.TotalDiscount);
        }


        #region Task Tests

        [Test]
        public void BasketTestOne()
        {
            // Arrange
            var itemOne = GetBasketItem(Guid.NewGuid(), 54.65m, 0, false);
            _sut.AddItemToBasket(itemOne);
            _sut.AddItemToBasket(GetBasketItem(Guid.NewGuid(), 3.5m, 0, false));
            var basket = _sut.GetCurrentBasket();
            Assert.True(58.15m == basket.TotalPrice);
            Assert.True(0m == basket.TotalDiscount);
        }


        [Test]
        public async Task BasketTestTwo()
        {
            // Arrange
            var itemOne = GetBasketItem(Guid.NewGuid(), 10.5m, 0, false);
            _sut.AddItemToBasket(itemOne);
            _sut.AddItemToBasket(GetBasketItem(Guid.NewGuid(), 54.65m, 0, false));
            await _sut.ApplyVoucherAsync(Models.Constants.VoucherCode5);
            var basket = _sut.GetCurrentBasket();
            Assert.True(60.15m == basket.TotalPrice);
            Assert.True(5m == basket.TotalDiscount);
        }

        [Test]
        public async Task BasketTestThree()
        {
            // Arrange
            var itemOne = GetBasketItem(Guid.NewGuid(), 10.5m, 0, false);
            _sut.AddItemToBasket(itemOne);
            await _sut.ApplyVoucherAsync(Models.Constants.VoucherCode50);
            var basket = _sut.GetCurrentBasket();
            Assert.True(0 == basket.TotalPrice);
            Assert.True(50m == basket.TotalDiscount);
        }

        [Test]
        public async Task BasketTestFour()
        {
            // Arrange
            var itemOne = GetBasketItem(Guid.NewGuid(), 25.0m, 0, false);
            _sut.AddItemToBasket(itemOne);
            _sut.AddItemToBasket(GetBasketItem(Guid.NewGuid(), 10.0m, 0, true));

            // Act
            await _sut.ApplyVoucherAsync(Models.Constants.VoucherCode50);
            var basket = _sut.GetCurrentBasket();

            // Assert
            Assert.AreEqual(10.0m, basket.TotalPrice);
            Assert.AreEqual(50m, basket.TotalDiscount);
        }

        #endregion



        private static Models.BasketItem GetBasketItem(Guid productSku,
            decimal itemPrice = 2.5m,
            decimal discount = 0.0m,
            bool isGiftItem = false)
        {
            return new Models.BasketItem()
            {
                ProductSku = productSku,
                ItemPrice = itemPrice,
                ItemDisount = discount,
                IsGiftVoucherItem = isGiftItem
            };
        }
    }
}
