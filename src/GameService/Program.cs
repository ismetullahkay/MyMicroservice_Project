using GameService.Base;
using GameService.Consumers;
using GameService.Data;
using GameService.Repositories;
using GameService.Repositories.ForCategory;
using GameService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<GameDbContext>(opt => {
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped(typeof(BaseResponseModel));
builder.Services.AddScoped<ICategoryRepository,CategoryRepository>();
builder.Services.AddScoped<IGameRepository,GameRepository>();

builder.Services.AddScoped<IFileService,FileService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(opt=>{
    opt.AddConsumersFromNamespaceContaining<GameCreatedFaultConsumer>();

    opt.AddEntityFrameworkOutbox<GameDbContext>(x=>{
        x.QueryDelay=TimeSpan.FromSeconds(10);

        x.UsePostgres();
        x.UseBusOutbox();
    });

    opt.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("game",false)); //deneme-deneme kelimeler arası tire yapar
    opt.UsingRabbitMq((context,cfg)=>{
        cfg.Host(builder.Configuration["RabbitMQ:Host"],"/",host =>{
            host.Username(builder.Configuration.GetValue("RabbitMQ:Username","guest"));
            host.Username(builder.Configuration.GetValue("RabbitMQ:Password","guest"));
        });
        cfg.ConfigureEndpoints(context);
    });
    

});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt=>{
    opt.Authority=builder.Configuration["AuthorirtyServiceUrl"];
    opt.RequireHttpsMetadata=false;
    opt.TokenValidationParameters.ValidateAudience=false;
    opt.TokenValidationParameters.NameClaimType="username"; //tokeni olusturan kullanıocın kim olduguna dair mesaj  
});

builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<GrpcGameService>();

app.Run();
