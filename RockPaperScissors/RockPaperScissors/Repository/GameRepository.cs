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


        public Task<Player> CreateComputer()
        {
            throw new NotImplementedException();
        }

        public async Task<Player> CreatePlayer(string playerName, int? id = null)
        {
            var player = await GetPlayer(playerName);
            if (player != null) 
                return player;
            
            player = new Player { Name = playerName };
            if (id != null)
                player.Id = id.Value;

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

            await CreateNewRound(game.Id);
            return game;
        }

        public async Task<Game> GetGame(int gameId) => 
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

        public async Task<string> MakeTurn(int gameId, int playerId, string turn)
        {
            /*if (!IsStringOfTurnCorrect(turn))
                return default;*/

            var round = await GetLastRoundInGame(gameId);/* await dbContext.Rounds.Where(r => r.GameId == gameId)
                                              .OrderBy(r => r.RoundNumber)
                                              .LastAsync();*/

            var isFirstPlayerTurn = GetGame(gameId).Result.PlayerOneId == playerId;
            if (!isFirstPlayerTurn)
            {
                round.PlayerTwoTurn = turn;
                dbContext.Entry(round).Property(r => r.PlayerTwoTurn).IsModified=true;

                var winnerIdInRound = GetWnnerIdOfRound(round);
                if (winnerIdInRound == ResultOfGame.IncorrectResult)
                    return default;

                round.WinnerId = (int)winnerIdInRound;
                dbContext.Entry(round).Property(r => r.WinnerId).IsModified = true;

                if (round.WinnerId >= 0)
                {
                    await dbContext.SaveChangesAsync();
                    return round.WinnerId.ToString();
                }
            }

            round.PlayerOneTurn = turn;
            dbContext.Entry(round).Property(r => r.PlayerOneTurn).IsModified = true;
            await dbContext.SaveChangesAsync();

            //if (round.WinnerId >= 0)
            //    return round.WinnerId.ToString();
            
            return turn;
        }

        public async Task<Round> GetLastRoundInGame(int gameId) =>    
            await dbContext.Rounds.Where(r => r.GameId == gameId)
                                  .OrderBy(r => r.RoundNumber)
                                  .LastAsync();
        

        public async Task<bool> CheckPlayerInGame(int gameId, int playerId)
        {
            var isPalyerInGame = await dbContext.Games.Where(g => g.Id == gameId &&
                                                 (g.PlayerOneId == playerId ||
                                                 g.PlayerTwoId == playerId)).CountAsync() > 0;
            
            return isPalyerInGame;
        }

        public async Task<Player> GetPlayer(string playerName) => 
            await dbContext.Players.FirstOrDefaultAsync(p => p.Name == playerName);

        public async Task<int?> CheckWhoseTurn(int gameId)
        {
            var roundsInGame = await dbContext.Rounds.Where(r => r.GameId == gameId).ToListAsync();
            var game = await GetGame(gameId);

            // Если не сыграно ни одного раунда
            if (roundsInGame.Count() == 0)
            {
                
                CreateNewRound(gameId);

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

            /*var playerTwoTurnInLastRound = roundsInGame.Last().PlayerTwoTurn;
            if (playerTwoTurnInLastRound == null)
                return game.PlayerTwoId;

            await CreateNewRound(gameId);

            return game.PlayerOneId;*/

            var playerOneTurnInLastRound = roundsInGame.Last().PlayerOneTurn;
            if (playerOneTurnInLastRound == null)
                return game.PlayerOneId;

            var playerTwoTurnInLastRound = roundsInGame.Last().PlayerTwoTurn;
            if (playerTwoTurnInLastRound == null)
                return game.PlayerTwoId;

            await CreateNewRound(gameId);

            return game.PlayerOneId;
        }

        private async Task CreateNewRound(int gameId)
        {
            //Round newRound = null;
            List<Round> roundsInGame = await dbContext.Rounds.Where(r => r.GameId == gameId).ToListAsync();


            //var lastRoundNumber = dbContext.Rounds.Where(r => r.GameId == gameId).Max();

            if (roundsInGame.Count() < Game.MAX_ROUNDS)
            {
                Round newRound = new Round()
                {
                    RoundNumber = roundsInGame.Count() + 1,
                    GameId = gameId,
                    PlayerOneTurn = default,//"",
                    PlayerTwoTurn = default//""
                };

                await dbContext.Rounds.AddAsync(newRound);
                await dbContext.SaveChangesAsync();
            }
        }

        /*bool IsStringOfTurnCorrect(string turn)
        {
            return turn == "камень" || turn == "ножницы" || turn == "бумага";
        }*/

        private ResultOfGame GetWnnerIdOfRound(Round round)
        {
            ResultOfGame result;// = ResultOfGame.IncorrectResult;

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

        

        public enum ResultOfGame
        {
            IncorrectResult = -1,
            Draw = 0,
            PlayerOneWin = 1,
            PlayerTwoWin = 2
        }


    }

    
}
