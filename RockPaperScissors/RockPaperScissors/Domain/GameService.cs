using Microsoft.EntityFrameworkCore;
using RockPaperScissors.DAL.ContextModels;
using RockPaperScissors.DAL.Repository;

namespace RockPaperScissors.Domain
{
    public class GameService : IGameService
    {
        private readonly IGameRepository repository;

        public GameService(IGameRepository repository)
        {
            this.repository = repository;
        }

        private async Task SaveChanges() =>
            await repository.SaveChanges();

        public async Task<Player> CreatePlayer(string playerName, int? id = null)
        {
            var player = await GetPlayer(playerName);
            if (player != null)
                return player;

            player = new Player { Name = playerName };
            if (id != null)
                player.Id = id.Value;

            await repository.CreatePlayer(player);
            await SaveChanges();

            return player;
        }

        public async Task<Game> CreateGame(Player player)
        {
            if (player == null)
                return default;

            var game = new Game();
            game.PlayerOneId = player.Id;
            //await dbContext.Games.AddAsync(game);
            await repository.CreateGame(game);
            
            await CreateNewRound(game.Id);
            //await repository.SaveChanges();
            await SaveChanges();
            return game;
        }

        private async Task CreateNewRound(int gameId)
        {
            List<Round> roundsInGame = await repository.GetRoundsInGame(gameId);

            if (roundsInGame.Count() < Game.MAX_ROUNDS)
            {
                Round newRound = new Round()
                {
                    RoundNumber = roundsInGame.Count() + 1,
                    GameId = gameId,
                    PlayerOneTurn = default,
                    PlayerTwoTurn = default
                };

                await repository.CreateNewRound(newRound);
                //await dbContext.SaveChangesAsync();
            }
        }

        public async Task<Game> GetGame(int gameId) =>
            await repository.GetGame(gameId);

        public async Task<Player> GetPlayer(string playerName) =>
            await repository.GetPlayer(playerName);

        public async Task ConnectSecondPlayerToTheGame(Game game, int secondPlayerId)
        {
            game.PlayerTwoId = secondPlayerId;
            //game.PlayerTwo = secondPlayer;
            //dbContext.Attach(game);
            //dbContext.Entry(game).Property(g => g.PlayerTwoId).IsModified = true;
            await repository.ConnectSecondPlayerToTheGame(game);
            await SaveChanges();
        }

        public async Task<bool> CheckPlayerInGame(int gameId, int playerId) => 
            await repository.CheckPlayerInGame(gameId, playerId);

        public async Task<int?> CheckWhoseTurn(int gameId)
        {
            var roundsInGame = await GetRoundsInGame(gameId);
            var game = await GetGame(gameId);

            // Если не сыграно ни одного раунда
            if (roundsInGame.Count == 0)
            {
                await CreateNewRound(/*round*/gameId);

                return game.PlayerOneId;
            }

            // Если сыграны все раунды
            if (roundsInGame.Count >= Game.MAX_ROUNDS &&
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

            await CreateNewRound(/*newRound*/gameId);
            await SaveChanges();

            return game.PlayerOneId;
        }

        public async Task<List<Round>> GetRoundsInGame(int gameId) =>
            await repository.GetRoundsInGame(gameId);

        public async Task<string> MakeTurn(int gameId, int playerId, string turn)
        {
            var round = await GetLastRoundInGame(gameId);

            var isFirstPlayerTurn = GetGame(gameId).Result.PlayerOneId == playerId;
            if (!isFirstPlayerTurn)
            {
                round.PlayerTwoTurn = turn;
                //dbContext.Entry(round).Property(r => r.PlayerTwoTurn).IsModified = true;

                var winnerIdInRound = GetWinnerIdOfRound(round);
                if (winnerIdInRound == ResultOfGame.IncorrectResult)
                    return default;

                round.WinnerId = (int)winnerIdInRound;
                //dbContext.Entry(round).Property(r => r.WinnerId).IsModified = true;

                if (round.WinnerId >= 0)
                {
                    //await dbContext.SaveChangesAsync();
                    repository.WriteTurn(round);
                    await SaveChanges();
                    return round.WinnerId.ToString();
                }
            }

            round.PlayerOneTurn = turn;
            //dbContext.Entry(round).Property(r => r.PlayerOneTurn).IsModified = true;
            //await dbContext.SaveChangesAsync();
            repository.WriteTurn(round);
            await SaveChanges();

            return turn;
        }

        public async Task<Round> GetLastRoundInGame(int gameId) =>
            await repository.GetLastRoundInGame(gameId);

        private static ResultOfGame GetWinnerIdOfRound(Round round)
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

        public bool IsStringOfTurnCorrect(string turn)
        {
            return turn == "камень" || turn == "ножницы" || turn == "бумага";
        }

        public string GetStatisticsOfRound(Game game, Round round)
        {
            if (round.WinnerId == (int)Round.ResultOfGame.Draw)
                return $"   Раунд {round.RoundNumber}\n" +
                       $"   Игрок 1 (Id {game.PlayerOneId}): {round.PlayerOneTurn}\n" +
                       $"   Игрок 2 (Id {game.PlayerTwoId}): {round.PlayerTwoTurn}\n" +
                       $"   Результат: ничья\n\n";

            return $"   Раунд {round.RoundNumber}\n" +
                   $"   Игрок 1 (Id {game.PlayerOneId}): {round.PlayerOneTurn}\n" +
                   $"   Игрок 2 (Id {game.PlayerTwoId}): {round.PlayerTwoTurn}\n" +
                   $"   Результат: победа игрока {round.WinnerId}\n\n";
        }

        public async Task<Round.ResultOfGame> CheckWinnerOfGame(int gameId)
        {
            var roundsInGame = await GetRoundsInGame(gameId);

            var winsOfPlayerOne = roundsInGame
                .Where(r => r.WinnerId == (int)Round.ResultOfGame.PlayerOneWin).Count();
            var winsOfPlayerTwo = roundsInGame
                .Where(r => r.WinnerId == (int)Round.ResultOfGame.PlayerTwoWin).Count();

            if (winsOfPlayerOne == Game.WINS_IN_ROUNDS_TO_WIN_THE_GAME)
                return Round.ResultOfGame.PlayerOneWin;

            if (winsOfPlayerTwo == Game.WINS_IN_ROUNDS_TO_WIN_THE_GAME)
                return Round.ResultOfGame.PlayerTwoWin;

            if (roundsInGame.Count() == Game.MAX_ROUNDS &&
                roundsInGame[Game.MAX_ROUNDS - 1].PlayerTwoTurn != null)
            {
                if (winsOfPlayerOne > winsOfPlayerTwo)
                    return Round.ResultOfGame.PlayerOneWin;

                if (winsOfPlayerOne < winsOfPlayerTwo)
                    return Round.ResultOfGame.PlayerTwoWin;

                if (winsOfPlayerOne == winsOfPlayerTwo)
                    return Round.ResultOfGame.Draw;
            }

            return Round.ResultOfGame.IncorrectResult;
        }

        public /*Task<*/string/*>*/ ConvertWinnerIdToString(/*int gameId,*/ Round.ResultOfGame resultOfGame)
        {
            string winnerId = string.Empty;

            //var game = await GetGame(gameId);

            switch (resultOfGame)
            {
                case Round.ResultOfGame.PlayerOneWin:
                    winnerId = ((int)Round.ResultOfGame.PlayerOneWin).ToString();
                    break;
                case Round.ResultOfGame.PlayerTwoWin:
                    winnerId = ((int)Round.ResultOfGame.PlayerTwoWin).ToString();
                    break;
                case Round.ResultOfGame.Draw:
                    winnerId = ((int)Round.ResultOfGame.Draw).ToString();
                    break;
            }

            return winnerId;
        }

        public string GetWinnerOfGame(Round.ResultOfGame resultOfGame)
        {
            string result = string.Empty;

            switch (resultOfGame)
            {
                case (Round.ResultOfGame.PlayerOneWin):
                case (Round.ResultOfGame.PlayerTwoWin):
                    result = $"Результат игры: победа игрока {(int)resultOfGame}";
                    break;
                case (Round.ResultOfGame.Draw):
                    result = $"Результат игры: ничья";
                    break;
                case (Round.ResultOfGame.IncorrectResult):
                    result = $"Результат игры: ошибка";
                    break;
            }
            return result;
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
