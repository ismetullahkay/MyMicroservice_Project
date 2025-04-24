using System.Security.Claims;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AutoMapper;
using BasketService.Base;
using BasketService.Model;
using Contracts;
using DiscountService;
using DiscountService.Services;
using MassTransit;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BasketService.Repository;

public class BasketRepository : IBasketRepository
{
    private readonly IDatabase _db;
    public string connectionString;
    private readonly IHttpContextAccessor _contextAccessor;
    public string UserId;
    public IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;
    private readonly GrpcDiscountClient _discountClient;
    public BasketRepository(IConfiguration configuration, IHttpContextAccessor contextAccessor, IPublishEndpoint publishEndpoint, IMapper mapper, GrpcDiscountClient discountClient) //cstr tetiklendiğinde bağlantı oluşur
    {// bu sınıf ilk nesne oluşturulduğunda çalışır ve bağlantı yapılır
        connectionString = configuration.GetValue<string>("RedisDatabase");
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
        _db = redis.GetDatabase();
        _contextAccessor = contextAccessor;
        UserId = contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
        _discountClient = discountClient;
    } 

    public async Task<ResponseModel<bool>> AddBasket(BasketModel model)
    {
        ResponseModel<bool>responseModel=new ResponseModel<bool>();

        if(model is not null){
            var convertType=JsonConvert.SerializeObject(model); //stringi jsona çevirmeliyiz redis için
            await _db.ListRightPushAsync(UserId,convertType); //sepete eklenen her oyun bir sonraki indise eklenir
            responseModel.isSuccess=true; //user id giriş yapmış kullanıcıdan alınır.ctxAccesor ile 
            return responseModel;
        }
        return responseModel;
    }

    public async Task<ResponseModel<BasketModel>> GetBasketItem(long index) //index sırayı alır
    {
        ResponseModel<BasketModel> responseModel=new ResponseModel<BasketModel>();
        var response=await _db.ListGetByIndexAsync(UserId,index); //belirtilen indexteki nesneyi alır 
        var objResult=JsonConvert.DeserializeObject<BasketModel>(response); //jsonu basketmodel nesnesine dönüştürür
        responseModel.isSuccess=true;
        responseModel.Data=objResult;
        responseModel.index=index;
        
        return responseModel;

    }

    public async Task<ResponseModel<List<BasketModel>>> GetBasketItems()
    {
       ResponseModel<List<BasketModel>> responseModel= new ResponseModel<List<BasketModel>>();
       if(!string.IsNullOrEmpty(UserId))
       {
        var response=await _db.ListRangeAsync(UserId); //redisten sepetteki tüm oyunları listeler(json)
        List<BasketModel> basketModel=new List<BasketModel>();

        foreach(var item in response){
            var objResult=JsonConvert.DeserializeObject<BasketModel>(item); //her oyunu jsondan basketmodel nesnesine dönüştürür
            basketModel.Add(objResult); //her oyunu listeye ekle
        }
       
        if(basketModel.Count>0) //eğer listeye eklenen oyun varsa 
        {
            responseModel.Data=basketModel;
            responseModel.isSuccess=true;
            return responseModel;
        }
        responseModel.isSuccess=false;
        return responseModel;
       }
       responseModel.isSuccess=false;
       responseModel.Message="Please before login your account";
       return responseModel;
    }

    public async Task<ResponseModel<bool>> RemoveBasketItem(long index)
    {
        ResponseModel<bool>responseModel=new ResponseModel<bool>();
        var willDeleteItem=await _db.ListGetByIndexAsync(UserId,index); //belirtilen indexe göre oyun getirilir
        await _db.ListRemoveAsync(UserId,willDeleteItem); //ilgili oyun redisten silinir
        responseModel.isSuccess=true;
        return responseModel;

    }

    public async Task<ResponseModel<bool>> Checkout()
    {
        List<Checkout> checkouts=new List<Checkout>();

        ResponseModel<bool> responseModel=new ResponseModel<bool>();

        var response=await _db.ListRangeAsync(UserId); 

        foreach(var item in response)
        {
            Checkout _checkout=new Checkout();

            var objResult=JsonConvert.DeserializeObject<BasketModel>(item); 

            _checkout.GameName=objResult.GameName;
            _checkout.GameAuthor=objResult.GameAuthor;
            _checkout.GameId=objResult.GameId;
            _checkout.UserId=Guid.Parse(UserId);
            _checkout.Price=objResult.Price;
            _checkout.GameDescription=objResult.GameDescription;

            checkouts.Add(_checkout);
        }

        if(checkouts.Count>0)
        {
           responseModel.isSuccess=true;
           foreach(var item in checkouts)
           {
            await _publishEndpoint.Publish(_mapper.Map<CheckoutBasketModel>(item));   
           } 
           return responseModel;
        }
        responseModel.isSuccess=false;
        return responseModel;
    }

    public async Task<ResponseModel<bool>> ImplementCoupon(long index,string couponCode)
    {
        ResponseModel<bool>responseModel=new ResponseModel<bool>();
        var discount=_discountClient.GetDiscount(couponCode); //gRPC

        if(discount !=null)
        {
            var response=await _db.ListGetByIndexAsync(UserId,index);
            var deserializeObj=JsonConvert.DeserializeObject<BasketModel>(response); //basketmodele çevirdik
            deserializeObj.Price=deserializeObj.Price-(deserializeObj.Price*discount.DiscountAmount)/100;  //KUPON İNDİRİMİ
            var SerializeObject=JsonConvert.SerializeObject(deserializeObj);
            await _db.ListSetByIndexAsync(UserId,index,SerializeObject); //key userid,valu serializeobj
            responseModel.isSuccess=true;
            return responseModel;
        }
        responseModel.isSuccess=false;
        return responseModel;
    }
}