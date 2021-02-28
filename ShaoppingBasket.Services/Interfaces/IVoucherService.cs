using System;
using System.Threading.Tasks;
using ShoppingBasket.Services.Models;

namespace ShoppingBasket.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<Voucher> GetAsync(string voucherCode);

        Task<bool> IsVadlidAsync(string voucherCode);
    }
}
