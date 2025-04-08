using AutoMapper;
using Contracts;
using Elastic.Clients.Elasticsearch;
using FilterService.Models;
using MassTransit;

namespace FilterService.Consumer;

public class GameCreatedFilterConsumer : IConsumer<GameCreated>
{
    private readonly IMapper _mapper;
    private readonly ElasticsearchClient _elasticClient;

    private string indexName;

    public GameCreatedFilterConsumer(IMapper mapper, ElasticsearchClient elasticClient,IConfiguration configuration)
    {
        _mapper = mapper;
        _elasticClient = elasticClient;
        indexName=configuration.GetValue<string>("indexName");
    }

    public async Task Consume(ConsumeContext<GameCreated> context)
    {
        Console.WriteLine("Consuming Filter Service or Created Game --->"+ context.Message.GameName);

        var objDto=_mapper.Map<GameFilterItem>(context.Message);
        objDto.GameId=context.Message.Id.ToString();

        var elasticsearch=await _elasticClient.IndexAsync(objDto,x=>x.Index(indexName));

        if(!elasticsearch.IsValidResponse){
            Console.WriteLine("Consuming proccess is not valid");
        }
    }
}
