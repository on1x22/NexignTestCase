using Microsoft.EntityFrameworkCore;
using RockPaperScissors.DAL.Contexts;
using RockPaperScissors.Repository;

var builder = WebApplication.CreateBuilder(args);

RegisterServices(builder.Services);

var app = builder.Build();

Configure(app);

app.Run();


void RegisterServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddTransient<IGameRepository, GameRepository>();

    services.AddDbContext<GameDbContext>(options => 
        options.UseInMemoryDatabase("TestGameDb"));  
}

void Configure(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();
}