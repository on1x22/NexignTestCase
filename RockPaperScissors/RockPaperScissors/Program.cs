using Microsoft.EntityFrameworkCore;
using RockPaperScissors.DAL.Contexts;
using RockPaperScissors.Repository;

var builder = WebApplication.CreateBuilder(args);

RegisterServices(builder.Services);

var app = builder.Build();

Configure(app);

/*// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();*/

app.Run();


void RegisterServices(IServiceCollection services)
{
    // Add services to the container.

    services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddTransient<IGameRepository, GameRepository>();
    //services.AddLogging(builder => builder.AddConsole());


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
        //db.Database.EnsureCreated();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    //app.UseHttpsRedirection();
    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();
}