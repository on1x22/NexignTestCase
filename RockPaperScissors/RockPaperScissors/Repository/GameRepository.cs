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

        public async Task<List<Game>> GetAllGames()
        {
            return await dbContext.Games.ToListAsync();
        }

        public async Task<List<Round>> GetAllRounds()
        {
            return await dbContext.Rounds.ToListAsync();
        }



        public async Task<Player> CreatePlayer(string playerName)
        {
            var player = await GetPlayer(playerName);
            if (player != null) 
                return player;
            
            player = new Player { Name = playerName };
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
            //game.PlayerOne = player;
            await dbContext.Games.AddAsync(game);
            await dbContext.SaveChangesAsync();
            return game;
        }
        public async Task<Game> FindGame(int gameId) => 
            await dbContext.Games.FirstOrDefaultAsync(g => g.Id == gameId);
        
        public async Task ConnectSecondPlayerToTheGame(Game game, Player secondPlayer)
        {
            game.PlayerTwoId = secondPlayer.Id;
            game.PlayerTwo = secondPlayer;
            dbContext.Attach(game);
            dbContext.Entry(game).Property(g => g.PlayerTwoId).IsModified=true;
            //dbContext.Entry(game).Property(g =>g.PlayerTwo).IsModified=true;
            await dbContext.SaveChangesAsync();
        }

        private async Task<Player> GetPlayer(string playerName) => 
            await dbContext.Players.FirstOrDefaultAsync(p => p.Name == playerName);

        
    }
}
