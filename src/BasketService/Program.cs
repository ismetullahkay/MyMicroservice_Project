using BasketService.Repository;
using DiscountService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IBasketRepository,BasketRepository>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(opt=>
{
    opt.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("basket",false)); //deneme-deneme kelimeler arası tire yapar
    opt.UsingRabbitMq((context,cfg)=>{
        cfg.Host(builder.Configuration["RabbitMQ:Host"],"/",host =>{
            host.Username(builder.Configuration.GetValue("RabbitMQ:Username","guest"));
            host.Username(builder.Configuration.GetValue("RabbitMQ:Password","guest"));
        });
        cfg.ConfigureEndpoints(context);
    });
    

});

builder.Services.AddScoped<GrpcDiscountClient>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt=>{
    opt.Authority=builder.Configuration["AuthorirtyServiceUrl"];
    opt.RequireHttpsMetadata=false;
    opt.TokenValidationParameters.ValidateAudience=false;
    opt.TokenValidationParameters.NameClaimType="username"; //tokeni olusturan kullanıocın kim olduguna dair mesaj
    

});
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

app.Run();
