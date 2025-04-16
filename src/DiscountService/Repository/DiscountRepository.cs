using System.Security.Claims;
using DiscountService.Data;
using DiscountService.Entities;
using DiscountService.Models;
using DiscountService.Services;

namespace DiscountService.Respository;

public class DiscountRepository : IDiscountRespository
{
    private readonly AppDbContext _context;
    private readonly GrpcGameClient _grpcClient;
    private string UserId;


    public DiscountRepository(AppDbContext context, GrpcGameClient grpcClient,IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _grpcClient = grpcClient;
        UserId=contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task<bool> CreateDiscount(DiscountModel model)
    {
        if(model != null)
        {
            var game=_grpcClient.GetGame(model.GameId,UserId);
               
            if(!string.IsNullOrEmpty(game.GameName))
            {
                Discount discount=new ()
                {
                    CouponCode=model.CouponCode,
                    DiscountAmount=model.DiscountAmount,
                    GameId=model.GameId,
                    UserId=game.UserId
                };

                await _context.Discounts.AddAsync(discount);

                if(await _context.SaveChangesAsync()>0)
                {
                    return true;
                }
            }

        }
        return false;
    }
}