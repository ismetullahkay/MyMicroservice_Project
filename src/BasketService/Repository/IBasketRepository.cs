using BasketService.Base;
using BasketService.Model;

namespace BasketService.Repository;

public interface IBasketRepository{
    Task<ResponseModel<bool>> AddBasket(BasketModel model);

    Task<ResponseModel<BasketModel>> GetBasketItem(long index); //rediste genel olarak long index alır

    Task<ResponseModel<List<BasketModel>>> GetBasketItems();

    Task<ResponseModel<bool>> RemoveBasketItem(long index);

    Task<ResponseModel<bool>> Checkout();   
    Task<ResponseModel<bool>> ImplementCoupon(long index,string couponCode);
    
}