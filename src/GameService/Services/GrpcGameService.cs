using GameService.Data;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace GameService.Services;

public class GrpcGameService : GrpcGame.GrpcGameBase
{
    private readonly GameDbContext _context;
    public GrpcGameService(GameDbContext context)
    {
        _context = context;
    }

    public override async Task<GrpcGameResponse> GetGame(GetGameRequest request, ServerCallContext context)
    {
        Console.WriteLine("====> Grpc Received call service started");

        var game= await _context.Games.FirstOrDefaultAsync(x=>x.Id==Guid.Parse(request.Id) && x.UserId==request.UserId);

        if(game==null)
        {
            // Console.WriteLine($"Game not found with Id: {request.Id} and UserId: {request.UserId}");
            // throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));

        }
        
        var response=new GrpcGameResponse
        {
            Game=new GrpcGameModel
            {
                GameName=game.GameName,
                Price=Convert.ToDouble(game.Price),
                VideoUrl=game.VideoUrl,
                GameDescription=game.GameDescription,
                MinimumSystemRequirement=game.MinimumSystemRequirement,
                RecommendedSystemRequirement=game.RecommendedSystemRequirement,
                UserId=game.UserId,
                CategoryId=game.CategoryId.ToString(),

            }
        };

        return response;
    }
}