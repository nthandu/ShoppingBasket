using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ShoppingBasket.Services.Tests
{
    [TestFixture]
    public class VoucherServiceTests
    {
        private VoucherService _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new VoucherService();
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public async Task GetAsync_Returns_Null_WhenCodeIsNullOrEmpty(string voucherCode)
        {
            var voucher = await _sut.GetAsync(voucherCode);
            Assert.Null(voucher);
        }

        [TestCase(Models.Constants.VoucherCode25)]
        [TestCase(Models.Constants.VoucherCode5)]
        [TestCase(Models.Constants.VoucherCode50)]
        public async Task GetAsync_Returns_Voucher_WhenValidCodeProvided(string voucherCode)
        {
            var voucher = await _sut.GetAsync(voucherCode);
            Assert.NotNull(voucher);
        }

        [TestCase(null)]
        [TestCase(Models.Constants.VoucherCodeNotExists)]
        public async Task IsVadlidAsync_Returns_False_WhenVoucherNotExists(string voucherCode)
        {
            bool isValidVoucher = await _sut.IsVadlidAsync(voucherCode);
            Assert.False(isValidVoucher);
        }

        [Test]
        public async Task IsVadlidAsync_Returns_False_WhenVoucherCodeExpired()
        {
            bool isValidVoucher = await _sut.IsVadlidAsync(Models.Constants.VoucherCode25);
            Assert.False(isValidVoucher);
        }

        [TestCase(Models.Constants.VoucherCode10)]
        [TestCase(Models.Constants.VoucherCode5)]
        [TestCase(Models.Constants.VoucherCode50)]
        public async Task IsVadlidAsync_Returns_TrueWhenVoucherExistsAndNotExpired(string voucherCode)
        {
            bool isValidVoucher = await _sut.IsVadlidAsync(voucherCode);
            Assert.True(isValidVoucher);
        }
    }
}
