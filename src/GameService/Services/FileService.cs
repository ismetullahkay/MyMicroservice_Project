
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GameService.DTOs;

namespace GameService.Services;

public class FileService : IFileService
{
    private Cloudinary cloudinary;
    private Account account;
    private IConfiguration _configuration;

    public FileService(IConfiguration configuration)
    {
        _configuration = configuration;
        account=new Account(
            configuration.GetValue<string>("Cloudinary:cloudName"),
            configuration.GetValue<string>("Cloudinary:apiKey"),
            configuration.GetValue<string>("Cloudinary:apiSecret"));

         cloudinary=new Cloudinary(account); //account her seferinde login oluyo
         cloudinary.Api.Client.Timeout=TimeSpan.FromMinutes(30);  
    }

    public Task UploadImage()
    {
        throw new NotImplementedException();
    }

    public async Task<string> UploadVideo(IFormFile File)
    {
        var uploadResult=new VideoUploadResult();
        if(File.Length>0) //form üzerinden video yükleme seçildiyse
        {
            using var stream =File.OpenReadStream();
            var uploadParams= new VideoUploadParams
            {
                File= new FileDescription(File.FileName,stream),
                Folder="g-steam_microservices"
            };
            uploadResult=await cloudinary.UploadAsync(uploadParams);
            string videoUrl=cloudinary.Api.UrlVideoUp.BuildUrl(uploadResult.PublicId);
            
            return videoUrl;

        }
        return "";
        
    }
}
