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

        /*public async Task CreatePlayer_test(string playerName)
        {
            var player = new Player { Name = playerName };
            await dbContext.Players.AddAsync(player);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<Player>> GetAllPlayers()
        {
            return await dbContext.Players.ToListAsync();
        }

        public async Task<List<Game>> GetAllGames()
        {
            return await dbContext.Games.ToListAsync();
        }

        public async Task<List<Round>> GetAllRounds()
        {
            return await dbContext.Rounds.ToListAsync();
        }*/

        public async Task/*<Player>*/ CreatePlayer(/*string playerName, int? id = null*/ 
                                                    Player player)
        {
            /*var player = await GetPlayer(playerName);
            if (player != null)
                return player;

            player = new Player { Name = playerName };
            if (id != null)
                player.Id = id.Value;*/

            await dbContext.Players.AddAsync(player);
            /*await dbContext.SaveChangesAsync();
            return player;*/

        }

        public async Task/*<Game>*/ CreateGame(/*Player player*/ Game game)
        {
            /*if (player == null)
                return default;

            var game = new Game();
            game.PlayerOneId = player.Id;*/
            await dbContext.Games.AddAsync(game);
            /*await dbContext.SaveChangesAsync();

            await CreateNewRound(game.Id);
            return game;*/
        }



        public async Task<Game> GetGame(int gameId) =>
            await dbContext.Games.FirstOrDefaultAsync(g => g.Id == gameId);

        /*public async Task ConnectSecondPlayerToTheGame(Game game, Player secondPlayer)
        {
            game.PlayerTwoId = secondPlayer.Id;
            game.PlayerTwo = secondPlayer;
            dbContext.Attach(game);
            dbContext.Entry(game).Property(g => g.PlayerTwoId).IsModified = true;
            await dbContext.SaveChangesAsync();
        }*/

        public async Task ConnectSecondPlayerToTheGame(Game game/*, Player secondPlayer*/)
        {
            dbContext.Attach(game);
            dbContext.Entry(game).Property(g => g.PlayerTwoId).IsModified = true;
        }

        public async Task/*<string>*/ MakeTurn(int gameId, int playerId, string turn)
        {
            var round = await GetLastRoundInGame(gameId);

            var isFirstPlayerTurn = GetGame(gameId).Result.PlayerOneId == playerId;
            if (!isFirstPlayerTurn)
            {
                round.PlayerTwoTurn = turn;
                dbContext.Entry(round).Property(r => r.PlayerTwoTurn).IsModified = true;

                var winnerIdInRound = GetWnnerIdOfRound(round);
                //if (winnerIdInRound == ResultOfGame.IncorrectResult)
                //    return default;

                round.WinnerId = (int)winnerIdInRound;
                dbContext.Entry(round).Property(r => r.WinnerId).IsModified = true;

                if (round.WinnerId >= 0)
                {
                    await dbContext.SaveChangesAsync();
                    //return round.WinnerId.ToString();
                }
            }

            round.PlayerOneTurn = turn;
            dbContext.Entry(round).Property(r => r.PlayerOneTurn).IsModified = true;
            await dbContext.SaveChangesAsync();

            //return turn;
        }

        public void WriteTurn(/*string? playerOneTurn = default, string? playerTwoTurn = default, string? winnerId = default*/
                                   Round round)
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

        public async Task<int?> CheckWhoseTurn(int gameId)
        {
            //var roundsInGame = await dbContext.Rounds.Where(r => r.GameId == gameId).ToListAsync();
            var roundsInGame = await GetRoundsInGame(gameId);
            var game = await GetGame(gameId);

            // Если не сыграно ни одного раунда
            if (roundsInGame.Count() == 0)
            {
                Round round = new Round();

                await CreateNewRound(/*gameId*/round);

                return game.PlayerOneId;
            }

            // Если сыграны все раунды
            if (roundsInGame.Count() >= Game.MAX_ROUNDS &&
                roundsInGame[Game.MAX_ROUNDS - 1].WinnerId != null)
                return default;

            // Если игра закончилась досрочно
            var winsOfPlayerOne = roundsInGame.Where(r => r.WinnerId == game.PlayerOneId).Count();
            var winsOfPlayerTwo = roundsInGame.Where(r => r.WinnerId == game.PlayerTwoId).Count();
            if (winsOfPlayerOne == Game.WINS_IN_ROUNDS_TO_WIN_THE_GAME ||
                winsOfPlayerTwo == Game.WINS_IN_ROUNDS_TO_WIN_THE_GAME)
                return default;

            var playerOneTurnInLastRound = roundsInGame.Last().PlayerOneTurn;
            if (playerOneTurnInLastRound == null)
                return game.PlayerOneId;

            var playerTwoTurnInLastRound = roundsInGame.Last().PlayerTwoTurn;
            if (playerTwoTurnInLastRound == null)
                return game.PlayerTwoId;

            Round newRound = new Round();
            await CreateNewRound(/*gameId*/newRound);

            return game.PlayerOneId;
        }

        /*private async Task CreateNewRound(int gameId)
        {
            //List<Round> roundsInGame = await dbContext.Rounds.Where(r => r.GameId == gameId).ToListAsync();
            List<Round> roundsInGame = await GetRoundsInGame(gameId);

            if (roundsInGame.Count() < Game.MAX_ROUNDS)
            {
                Round newRound = new Round()
                {
                    RoundNumber = roundsInGame.Count() + 1,
                    GameId = gameId,
                    PlayerOneTurn = default,
                    PlayerTwoTurn = default
                };

                await dbContext.Rounds.AddAsync(newRound);
                await dbContext.SaveChangesAsync();
            }
        }*/

        public async Task CreateNewRound(Round round) =>
            await dbContext.Rounds.AddAsync(round);



        private ResultOfGame GetWnnerIdOfRound(Round round)
        {
            ResultOfGame result;

            if (round.PlayerOneTurn == null || round.PlayerTwoTurn == null)
                return ResultOfGame.IncorrectResult;
            var turns = (round.PlayerOneTurn, round.PlayerTwoTurn);

            switch (turns)
            {
                case ("камень", "камень"):
                    result = ResultOfGame.Draw;
                    break;
                case ("камень", "ножницы"):
                    result = ResultOfGame.PlayerOneWin;
                    break;
                case ("камень", "бумага"):
                    result = ResultOfGame.PlayerTwoWin;
                    break;
                case ("ножницы", "камень"):
                    result = ResultOfGame.PlayerTwoWin;
                    break;
                case ("ножницы", "ножницы"):
                    result = ResultOfGame.Draw;
                    break;
                case ("ножницы", "бумага"):
                    result = ResultOfGame.PlayerOneWin;
                    break;
                case ("бумага", "камень"):
                    result = ResultOfGame.PlayerOneWin;
                    break;
                case ("бумага", "ножницы"):
                    result = ResultOfGame.PlayerTwoWin;
                    break;
                case ("бумага", "бумага"):
                    result = ResultOfGame.Draw;
                    break;
                default:
                    result = ResultOfGame.IncorrectResult;
                    break;
            }

            return result;
        }

        public async Task<List<Round>> GetRoundsInGame(int gameId) =>
            await dbContext.Rounds.Where(r => r.GameId == gameId).ToListAsync();

        public async Task SaveChanges()
        {
            await dbContext.SaveChangesAsync();
        }

        public enum ResultOfGame
        {
            IncorrectResult = -1,
            Draw = 0,
            PlayerOneWin = 1,
            PlayerTwoWin = 2
        }
    }


}
