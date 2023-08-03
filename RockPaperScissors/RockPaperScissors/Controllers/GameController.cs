using Microsoft.AspNetCore.Mvc;
using RockPaperScissors.DAL.ContextModels;
using RockPaperScissors.Repository;
using System.Text;

namespace RockPaperScissors.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class gameController : ControllerBase
    {
        private readonly IGameRepository repository;

        public gameController(IGameRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromQuery] string playerName, bool withComputer = false)
        {
            Player computer = null;
            if (withComputer == true)
                computer = await repository.CreatePlayer("computer", id:Player.COMPUTER_ID);

            var player1 = await repository.CreatePlayer(playerName);
            var game = await repository.CreateGame(player1);
            if (game == null)
                return BadRequest();

            if (computer != null)
                return await ConnectSecondPlayerToTheGame(game.Id, computer.Name);

            return Ok($"Игрок с кодом {game.PlayerOneId} создал игру {game.Id}");
        }

        [HttpPost("{gameId}/join/{playerTwoName}")]
        public async Task<IActionResult> ConnectSecondPlayerToTheGame(int gameId, string playerTwoName)
        {
            var game = await repository.GetGame(gameId);
            if (game == null)
                return BadRequest($"Игры с Id {gameId} не существует");

            // Если игрок уже подключен к этой игре
            var player2 = await repository.GetPlayer(playerTwoName);
            if (player2 != null && (game.PlayerOneId == player2.Id ||
                                    game.PlayerTwoId == player2.Id))
                return BadRequest($"Игрок с Id {player2.Id} уже подключен к игре с Id {gameId}");

            // Если игрок подключается к существующей игре
            if(game.PlayerOneId != 0 && game.PlayerTwoId != 0)
                return BadRequest($"Нельзя подключиться к существующей игре с Id {gameId}");

            // Gдключение игрока к игре
            player2 = await repository.CreatePlayer(playerTwoName);

            await repository.ConnectSecondPlayerToTheGame(game, player2);

            return Ok($"Игрок с кодом {game.PlayerTwoId} подключился к игре {game.Id}");
        }

        [HttpPost("{gameId}/user/{playerId}/{turn}")]
        public async Task<IActionResult> MakeTurn(int gameId, int playerId, string turn)
        {
            var game = await repository.GetGame(gameId);
            if (game == null)
                return BadRequest($"Игры с Id {gameId} не существует");

            bool isPlayerInGame = await repository.CheckPlayerInGame(gameId, playerId);
            if (!isPlayerInGame)
                return BadRequest($"Игрок с кодом {playerId} не играл в игру {gameId}");

            var playerNumberWhoseTurn = await repository.CheckWhoseTurn(gameId);
            if (playerNumberWhoseTurn == null)
                return BadRequest($"Игра с Id {gameId} уже закончена. Начните новую игру");

            if (playerNumberWhoseTurn != playerId)
                return BadRequest($"Игрок с Id {playerId} ходит не в свой ход. " +
                    $"Ход необходимо выполнить другому игроку");

            if (!IsStringOfTurnCorrect(turn))
                return BadRequest("Задан некорректный ход. Должны быть только " +
                    "\"камень\", \"ножницы\" или \"бумага\"");

            await repository.MakeTurn(gameId, playerId, turn);
           
            var currentLastRound = await repository.GetLastRoundInGame(gameId);
            var resultTurn = currentLastRound.WinnerId.ToString();

            if (int.TryParse(resultTurn, out _))
            {
                return Ok(GetStatisticsOfRound(game, currentLastRound));                
            }
                        
            var winnerInGame = await CheckWinnerOfGame(gameId);
            if (winnerInGame != Round.ResultOfGame.IncorrectResult)
            {
                var winnerId = await GetWinnerOfRoundId(gameId, winnerInGame);
                return Ok($"Игрок {winnerId} победил в игре {gameId}");
            }

            if (game.PlayerTwoId == Player.COMPUTER_ID && currentLastRound.WinnerId == null)
            {
                return await MakeTurn(gameId, game.PlayerTwoId, GetComputerTurn());
            }

            return Ok($"Игрок {playerId} выполнил ход");
        }

        [HttpGet("{gameId}/stat")]
        public async Task<IActionResult> GetStatisticsOfGame(int gameId)
        {
            var game = await repository.GetGame(gameId);
            if(game == null)
                return BadRequest($"Игра с Id {gameId} не существует");

            var roundsInGame = await repository.GetRoundsInGame(gameId);

            if (await CheckWinnerOfGame(gameId) == Round.ResultOfGame.IncorrectResult)
                return BadRequest($"Статистика по игре с Id {gameId} не доступна, " +
                                  $"так как игра ещё не завершена");
                        
            var resultString = new StringBuilder();
            resultString.AppendLine($"Id игры {gameId}");
            foreach (var round in roundsInGame)
            {
                if (round.WinnerId != null)
                    resultString.Append(GetStatisticsOfRound(game, round));
            }

            resultString.Append(GetWinnerOfGame(await CheckWinnerOfGame(gameId)) + "\n\n");

            return Ok(resultString.ToString());
        }

        [HttpGet("{gameId}/stat/current")]
        public async Task<IActionResult> GetCurrentStatisticsOfGame(int gameId)
        {
            var game = await repository.GetGame(gameId);
            if (game == null)
                return BadRequest($"Игра с Id {gameId} не существует");

            var roundsInGame = await repository.GetRoundsInGame(gameId);
            if (roundsInGame.Count() == 0 ||
                (roundsInGame.Count() == 1 && roundsInGame[0].WinnerId == null))
                return BadRequest($"В игре с Id {gameId} ещё не сыграно ни одного раунда");

            var resultString = new StringBuilder();
            resultString.AppendLine($"Id игры {gameId}\nСтатистика по сыгранным раундам:");

            foreach (var round in roundsInGame)
            {
                if (round.WinnerId != null)
                    resultString.Append(GetStatisticsOfRound(game, round));
            }

            return Ok(resultString.ToString());
        }

        private string GetStatisticsOfRound (Game game, Round round)
        {
            string result = string.Empty;

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

        private async Task<Round.ResultOfGame> CheckWinnerOfGame(int gameId)
        {
            var roundsInGame = await repository.GetRoundsInGame(gameId);

            var winsOfPlayerOne = roundsInGame
                .Where(r => r.WinnerId == (int)Round.ResultOfGame.PlayerOneWin).Count();
            var winsOfPlayerTwo = roundsInGame
                .Where(r => r.WinnerId == (int)Round.ResultOfGame.PlayerTwoWin).Count();

            if (winsOfPlayerOne == Game.WINS_IN_ROUNDS_TO_WIN_THE_GAME)
                return Round.ResultOfGame.PlayerOneWin;

            if (winsOfPlayerTwo == Game.WINS_IN_ROUNDS_TO_WIN_THE_GAME)
                return Round.ResultOfGame.PlayerTwoWin;

            if (roundsInGame.Count() == Game.MAX_ROUNDS &&
                roundsInGame[Game.MAX_ROUNDS-1].PlayerTwoTurn != null)
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

        private bool IsStringOfTurnCorrect(string turn)
        {
            return turn == "камень" || turn == "ножницы" || turn == "бумага";
        }

        private async Task<string> GetWinnerOfRoundId (int gameId, Round.ResultOfGame resultOfGame)
        {
            string winnerId = string.Empty;

            var game = await repository.GetGame (gameId);

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

        private string GetWinnerOfGame(Round.ResultOfGame resultOfGame)
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

        private string GetComputerTurn()
        {
            var random = new Random();
            var values = new List<string>{
                "камень",
                "ножницы",
                "бумага"};
            int index = random.Next(values.Count);
            
            return values[index];
        }
    }
}
