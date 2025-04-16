using DiscountService.Models;

namespace DiscountService.Respository;

public interface IDiscountRespository
{
    Task<bool>CreateDiscount(DiscountModel model);
}