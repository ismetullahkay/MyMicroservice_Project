using DiscountService.Data;
using DiscountService.Respository;
using DiscountService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddDbContext<AppDbContext>(opt=>opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDiscountRespository,DiscountRepository>();
builder.Services.AddScoped<GrpcGameClient>(); //gRPC

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt=>{
    opt.Authority=builder.Configuration["AuthorirtyServiceUrl"];
    opt.RequireHttpsMetadata=false;
    opt.TokenValidationParameters.ValidateAudience=false;
    opt.TokenValidationParameters.NameClaimType="username"; //tokeni olusturan kullanıocın kim olduguna dair mesaj  
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddGrpc();
    
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcDiscountService>();
app.Run();
