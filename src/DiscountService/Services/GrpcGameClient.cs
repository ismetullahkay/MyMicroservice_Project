using DiscountService.Models;
using Grpc.Net.Client;
using GameService;


namespace DiscountService.Services;

public class GrpcGameClient //Bu sınıfın amacı, dışarıdaki bir gRPC Game servisinden oyun bilgilerini almak
{
    private readonly ILogger<GrpcGameClient> _logger;
    private readonly IConfiguration _configuration;
    public GrpcGameClient(ILogger<GrpcGameClient> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Game GetGame(string gameId,string userId) //grpc'nin getGame çağrısı
    {
        _logger.LogWarning("Calling grpc protobuf serviec");

        var channel=GrpcChannel.ForAddress(_configuration["GrpcGame"]); //localhost.com/9999
        var client=new GrpcGame.GrpcGameClient(channel);

        var request=new GetGameRequest
        {
            Id=gameId,
            UserId=userId
        };
        try
        {
            var response=client.GetGame(request);

            Game game=new Game
            {
                GameName=response.Game.GameName,
                Price=Convert.ToDecimal(response.Game.Price),
                VideoUrl=response.Game.VideoUrl,
                GameDescription=response.Game.GameDescription,
                MinimumSystemRequirement=response.Game.MinimumSystemRequirement,
                RecommendedSystemRequirement=response.Game.RecommendedSystemRequirement,
                UserId=response.Game.UserId,
                CategoryId=Guid.Parse(response.Game.CategoryId)
            };
            Console.WriteLine(response);
            return game;
        }
        catch(System.Exception ex)
        {
            _logger.LogError(ex.Message);
            throw ex;
        }
    }
}