using System.Security.Claims;
using AutoMapper;
using Contracts;
using GameService.Base;
using GameService.Data;
using GameService.DTOs;
using GameService.Entities;
using GameService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace GameService.Repositories;

public class GameRepository : IGameRepository
{
    private readonly GameDbContext _context;
    private IMapper _mapper;
    private BaseResponseModel _response;
    private IFileService _service;
    private IPublishEndpoint _publishEndpoint;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string UserId;

    public GameRepository(GameDbContext context, IMapper mapper, BaseResponseModel response, IFileService service, IPublishEndpoint publishEndpoint, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _response = response;
        _service = service;
        _publishEndpoint = publishEndpoint;
        _httpContextAccessor = httpContextAccessor;
        UserId=_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
    }

    public async Task<BaseResponseModel> CreateGame(GameDTO game)
    {
      if(game.File.Length>0){
        
        string videoUrl=await _service.UploadVideo(game.File);

        var objDto=_mapper.Map<Game>(game);

        objDto.VideoUrl=videoUrl;
        objDto.UserId=UserId;

        await _context.Games.AddAsync(objDto);
        await _publishEndpoint.Publish(_mapper.Map<GameCreated>(objDto));

        if(await _context.SaveChangesAsync()>0){
            _response.IsSuccess=true;
            _response.Message="Created Game is Successfully";
            _response.Data=objDto;
            
            return _response;
        }   
      }
        _response.IsSuccess=false;
        return _response;
    }

    public async Task<BaseResponseModel> GetAllGames()
    {
        List<Game> games= await _context.Games.Include(c=>c.Category).Include(g=>g.GameImages).ToListAsync(); //include ile ilişkili çektik

        if(games is not null){
          _response.Data=games;
          _response.IsSuccess=true;
          return _response;
        }
        _response.IsSuccess=false;
        return _response;
    }

    public async Task<BaseResponseModel> GetGamesByCategory(Guid categoryId)
    {
        List<Game> games= await _context.Games.Where(c=>c.CategoryId==categoryId).ToListAsync(); 
        //gamestaki categoryid'ye denk gelen id verilirse ilgili categoriye ait oyunları listeler

        if(games is not null){
          _response.Data=games;
          _response.IsSuccess=true;
          return _response;
        }
        _response.IsSuccess=false;
        return _response;

    }

    public async Task<BaseResponseModel> RemoveGame(Guid gameId)
    {
        var removeGame= await _context.Games.FindAsync(gameId); //find pk'ye göre arar

        if(removeGame is not null)
        {
          _context.Games.Remove(removeGame);
          await _publishEndpoint.Publish<GameDeleted>(new {Id=gameId.ToString()});
         
          if(await _context.SaveChangesAsync() > 0 )
          {
            _response.IsSuccess=true;
            _response.Data=removeGame;
            return _response;          
          }    
        }

        _response.IsSuccess=false;
        return _response;      
    }

    public async Task<BaseResponseModel> UpdateGame(UpdateGameDTO game, Guid gameId)
    {
        var updateGame= await _context.Games.FindAsync(gameId);
        

        if(updateGame is not null){

         updateGame.Price=game.Price;
         updateGame.GameDescription=game.GameDescription;
         updateGame.GameAuthor=game.GameAuthor;
         updateGame.GameName=game.GameName;
         updateGame.MinimumSystemRequirement=game.MinimumSystemRequirement;
         updateGame.RecommendedSystemRequirement=game.MinimumSystemRequirement;

         await _publishEndpoint.Publish(_mapper.Map<GameUpdated>(updateGame));

        if(await _context.SaveChangesAsync() > 0 )
          {
            _response.IsSuccess=true;
            _response.Data=updateGame;
            return _response;          
          }    
        }
          _response.IsSuccess=false;
        return _response;     
    }
}