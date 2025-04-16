using AutoMapper;
using Contracts;
using MassTransit;
using OrderService.Data;
using OrderService.Entities;

namespace OrderService.Consumers;

public class CheckoutBasketConsumer : IConsumer<CheckoutBasketModel>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;
    public CheckoutBasketConsumer(IMapper mapper, ApplicationDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task Consume(ConsumeContext<CheckoutBasketModel> context) // mesaj kuyrugundan checkoutbasketmodel mesajı geldiğinde tetiklenecek
    {
        Console.WriteLine("Checkout basket consuming with order");

        var item=_mapper.Map<Order>(context.Message); //ctx.msg kuyruktan gelen mesaj verisidir.(ssepet bilgileri). ordera dönüştürür.
        await _context.Orders.AddAsync(item); //orderi db'e ekler 
        await _context.SaveChangesAsync();
        
    }
}