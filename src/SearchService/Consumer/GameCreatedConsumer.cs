using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumer;

public class GameCreatedConsumer : IConsumer<GameCreated>
{
    private readonly IMapper _mapper;
    public GameCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<GameCreated> context)
    {
        Console.WriteLine("Consuming starting"+context.Message.GameName);

        var gameItem=_mapper.Map<GameItem>(context.Message); 
        //datalar dbdeki outboxda body içerisinde messagede tutuluyor   

        await gameItem.SaveAsync();
    }
}