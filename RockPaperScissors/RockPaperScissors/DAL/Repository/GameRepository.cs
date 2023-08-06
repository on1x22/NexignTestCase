using Microsoft.EntityFrameworkCore;
using RockPaperScissors.DAL.ContextModels;
using RockPaperScissors.DAL.Contexts;

namespace RockPaperScissors.DAL.Repository
{
    public class GameRepository : IGameRepository
    {
        private readonly GameDbContext dbContext;

        public GameRepository(GameDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task CreatePlayer(Player player)
        {
            await dbContext.Players.AddAsync(player);
        }

        public async Task CreateGame( Game game)
        {            
            await dbContext.Games.AddAsync(game);            
        }

        public async Task<Game> GetGame(int gameId) =>
            await dbContext.Games.FirstOrDefaultAsync(g => g.Id == gameId);

        public async Task ConnectSecondPlayerToTheGame(Game game)
        {
            dbContext.Attach(game);
            dbContext.Entry(game).Property(g => g.PlayerTwoId).IsModified = true;
        }

        public void WriteTurn(Round round)
        {
            dbContext.Entry(round).Property(r => r.PlayerOneTurn).IsModified = true;
            dbContext.Entry(round).Property(r => r.PlayerTwoTurn).IsModified = true;
            dbContext.Entry(round).Property(r => r.WinnerId).IsModified = true;
        }

        public async Task<Round> GetLastRoundInGame(int gameId) =>
            await dbContext.Rounds.Where(r => r.GameId == gameId)
                                  .OrderBy(r => r.RoundNumber)
                                  .LastAsync();

        public async Task<bool> CheckPlayerInGame(int gameId, int playerId) => 
            await dbContext.Games.Where(g => g.Id == gameId &&
                                       (g.PlayerOneId == playerId ||
                                        g.PlayerTwoId == playerId)).CountAsync() > 0;

        public async Task<Player> GetPlayer(string playerName) =>
            await dbContext.Players.FirstOrDefaultAsync(p => p.Name == playerName);

        public async Task CreateNewRound(Round round) =>
            await dbContext.Rounds.AddAsync(round);

        public async Task<List<Round>> GetRoundsInGame(int gameId) =>
            await dbContext.Rounds.Where(r => r.GameId == gameId).ToListAsync();

        public async Task SaveChanges()
        {
            await dbContext.SaveChangesAsync();
        }
    }


}
