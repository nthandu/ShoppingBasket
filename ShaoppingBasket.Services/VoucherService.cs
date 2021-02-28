using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShoppingBasket.Services.Interfaces;
using ShoppingBasket.Services.Models;

namespace ShoppingBasket.Services
{
    public class VoucherService : IVoucherService
    {
        public async Task<Voucher> GetAsync(string voucherCode)
        {
            if (string.IsNullOrWhiteSpace(voucherCode))
                return null;
            var vouchers = await GetVouchersAsync();
            return vouchers?.
                FirstOrDefault(voucher => voucher.Code != null &&
                                voucher.Code.Equals(voucherCode, StringComparison.CurrentCultureIgnoreCase));
        }

        public async Task<bool> IsVadlidAsync(string voucherCode)
        {
            if (string.IsNullOrWhiteSpace(voucherCode))
                return false;

            var voucher = await GetAsync(voucherCode);

            if(voucher != null &&
                voucher.ExpiryDate >= DateTime.Today.Date)
            {
                // I have simple validation here.
                // We can have any other validation such as user type or any specifc business logics.
                return true;
            }

            return false;
        }

        private async Task<IList<Voucher>> GetVouchersAsync()
        {
            // In live application, this will be replaced by a call to repo.
            // We can implement cache to avoid calls to repo once we have the data.
            return await Task.FromResult(new List<Voucher>()
            {
                new Voucher()
                {
                    Code = Constants.VoucherCode10,
                    Discount = 10.0m,
                    ExpiryDate = DateTime.UtcNow.Date.AddDays(360)
                },
                new Voucher()
                {
                    Code = Constants.VoucherCode5,
                    Discount = 5.0m,
                    ExpiryDate = DateTime.UtcNow.Date.AddDays(360)
                },
                new Voucher()
                {
                    Code = Constants.VoucherCode50,
                    Discount = 50.0m,
                    ExpiryDate = DateTime.UtcNow.Date.AddDays(360)
                },
                new Voucher()
                {
                    Code = Constants.VoucherCode25,
                    Discount = 25.0m,
                    ExpiryDate = DateTime.UtcNow.Date.AddDays(-10)
                }
            });
        }
    }
}
