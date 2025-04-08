using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumer;

public class GameUpdatedConsumer : IConsumer<GameUpdated>
{
    private readonly IMapper _mapper;
    public GameUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<GameUpdated> context)
    {
        Console.WriteLine("=>>> Game Updated Consuming"+context.Message.Id);
        
        var objDto=_mapper.Map<GameItem>(context.Message);

        var result=await DB.Update<GameItem>()
                            .Match(a=>a.ID==context.Message.Id)
                            .ModifyOnly(x=> new {
                                x.CategoryId,
                                x.RecommendedSystemRequirement,
                                x.MinimumSystemRequirement,
                                x.GameDescription,
                                x.GameAuthor,
                                x.GameName,
                                x.Price
                            },objDto)
                            .ExecuteAsync();

        if(!result.IsAcknowledged)
        {
            Console.WriteLine("ooops something went wrong");
        }                    
    }
}