using MassTransit;
using SearchService.Consumer;
using SearchService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());



builder.Services.AddMassTransit(opt=>{
    opt.AddConsumersFromNamespaceContaining<GameCreatedConsumer>();


    opt.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search",false)); //deneme-deneme kelimeler arası tire yapar
    opt.UsingRabbitMq((context,cfg)=>{
        cfg.Host(builder.Configuration["RabbitMQ:Host"],"/",host =>{ //host sonra alıcı oldugu için receive
            host.Username(builder.Configuration.GetValue("RabbitMQ:Username","guest"));
            host.Username(builder.Configuration.GetValue("RabbitMQ:Password","guest"));
        });
        cfg.ReceiveEndpoint("search-game-created",e=>{
            e.UseMessageRetry(r=>r.Interval(5,5));
            e.ConfigureConsumer<GameCreatedConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });

});

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

app.Lifetime.ApplicationStarted.Register(async() =>{  //? error function
    try{
        await DbInitializer.InitializeDb(app);

    }
    catch(System.Exception){
        throw;

    }
})  ;
 
app.Run();
