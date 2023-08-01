using Microsoft.EntityFrameworkCore;
using RockPaperScissors.DAL.ContextModels;
using RockPaperScissors.DAL.Contexts;

namespace RockPaperScissors.Repository
{
    public class GameRepository : IGameRepository
    {
        private readonly GameDbContext dbContext;

        public GameRepository(GameDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
         
        public async Task CreatePlayer_test(string playerName)
        {
            var player = new Player { Name = playerName };
            await dbContext.Players.AddAsync(player);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<Player>> GetAllPlayers()
        {
            //var players = await dbContext.Players.ToListAsync();
            return await dbContext.Players.ToListAsync();
        }





        public async Task<Player> CreatePlayer(string playerName)
        {
            var player = new Player { Name = playerName };
            await dbContext.Players.AddAsync(player);
            await dbContext.SaveChangesAsync();
            return player;
        }

        public async Task<Game> CreateGame(Player player)
        {
            if (player == null)
                return default;

            var game = new Game();
            game.PlayerOneId = player.Id;
            game.PlayerOne = player;
            await dbContext.Games.AddAsync(game);
            await dbContext.SaveChangesAsync();
            return game;
        }
    }
}
