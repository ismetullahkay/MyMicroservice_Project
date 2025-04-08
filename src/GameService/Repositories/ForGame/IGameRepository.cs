using GameService.Base;
using GameService.DTOs;

namespace GameService.Repositories;
public interface IGameRepository
{
    public Task<BaseResponseModel> CreateGame(GameDTO game);
    public Task<BaseResponseModel> UpdateGame(UpdateGameDTO game, Guid gameId);
    
    public Task<BaseResponseModel> RemoveGame(Guid gameId);
    public Task<BaseResponseModel> GetAllGames();
    public Task<BaseResponseModel> GetGamesByCategory(Guid categoryId);
}