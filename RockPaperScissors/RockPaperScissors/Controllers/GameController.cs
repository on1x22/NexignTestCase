using Microsoft.AspNetCore.Mvc;
using RockPaperScissors.DAL.ContextModels;
using RockPaperScissors.Domain;
using System.Text;

namespace RockPaperScissors.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class gameController : ControllerBase
    {
        private readonly IGameService service;

        public gameController(IGameService service)
        {
            this.service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromQuery] string playerName, bool withComputer = false)
        {
            Player computer = null;
            if (withComputer == true)
                computer = await service.CreatePlayer("computer", id:Player.COMPUTER_ID);

            var player1 = await service.CreatePlayer(playerName);
            var game = await service.CreateGame(player1);
            if (game == null)
                return BadRequest();

            if (computer != null)
                return await ConnectSecondPlayerToTheGame(game.Id, computer.Name);

            return Ok($"Игрок с кодом {game.PlayerOneId} создал игру {game.Id}");
        }

        [HttpPost("{gameId}/join/{playerTwoName}")]
        public async Task<IActionResult> ConnectSecondPlayerToTheGame(int gameId, string playerTwoName)
        {
            var game = await service.GetGame(gameId);
            if (game == null)
                return BadRequest($"Игры с Id {gameId} не существует");

            // Если игрок уже подключен к этой игре
            var player2 = await service.GetPlayer(playerTwoName);
            if (player2 != null && (game.PlayerOneId == player2.Id ||
                                    game.PlayerTwoId == player2.Id))
                return BadRequest($"Игрок с Id {player2.Id} уже подключен к игре с Id {gameId}");

            // Если игрок подключается к существующей игре
            if(game.PlayerOneId != 0 && game.PlayerTwoId != 0)
                return BadRequest($"Нельзя подключиться к существующей игре с Id {gameId}");

            // Подключение игрока к игре
            player2 = await service.CreatePlayer(playerTwoName);

            await service.ConnectSecondPlayerToTheGame(game, player2.Id);

            return Ok($"Игрок с кодом {game.PlayerTwoId} подключился к игре {game.Id}");
        }

        [HttpPost("{gameId}/user/{playerId}/{turn}")]
        public async Task<IActionResult> MakeTurn(int gameId, int playerId, string turn)
        {
            var game = await service.GetGame(gameId);
            if (game == null)
                return BadRequest($"Игры с Id {gameId} не существует");

            bool isPlayerInGame = await service.CheckPlayerInGame(gameId, playerId);
            if (!isPlayerInGame)
                return BadRequest($"Игрок с кодом {playerId} не играл в игру {gameId}");

            var playerNumberWhoseTurn = await service.CheckWhoseTurn(gameId);
            if (playerNumberWhoseTurn == null)
                return BadRequest($"Игра с Id {gameId} уже закончена. Начните новую игру");

            if (playerNumberWhoseTurn != playerId)
                return BadRequest($"Игрок с Id {playerId} ходит не в свой ход. " +
                    $"Ход необходимо выполнить другому игроку");

            if (!service.IsStringOfTurnCorrect(turn))
                return BadRequest("Задан некорректный ход. Должны быть только " +
                    "\"камень\", \"ножницы\" или \"бумага\"");

            await service.MakeTurn(gameId, playerId, turn);

            var currentLastRound = await service.GetLastRoundInGame(gameId);
            var resultTurn = currentLastRound.WinnerId.ToString();

            if (int.TryParse(resultTurn, out _))
            {
                return Ok(service.GetStatisticsOfRound(game, currentLastRound));                
            }
                        
            var winnerInGame = await service.CheckWinnerOfGame(gameId);
            if (winnerInGame != Round.ResultOfGame.IncorrectResult)
            {
                var winnerId = service.ConvertWinnerIdToString(winnerInGame);
                return Ok($"Игрок {winnerId} победил в игре {gameId}");
            }

            if (game.PlayerTwoId == Player.COMPUTER_ID && currentLastRound.WinnerId == null)
            {
                return await MakeTurn(gameId, game.PlayerTwoId, ComputerPlayer.GetTurn());
            }

            return Ok($"Игрок {playerId} выполнил ход");
        }

        [HttpGet("{gameId}/stat")]
        public async Task<IActionResult> GetStatisticsOfGame(int gameId)
        {
            var game = await service.GetGame(gameId);
            if (game == null)
                return BadRequest($"Игра с Id {gameId} не существует");

            var roundsInGame = await service.GetRoundsInGame(gameId);

            if (await service.CheckWinnerOfGame(gameId) == Round.ResultOfGame.IncorrectResult)
                return BadRequest($"Статистика по игре с Id {gameId} не доступна, " +
                                  $"так как игра ещё не завершена");

            var resultsFormatter = new ResultsFormatter(service);
            var resultString = resultsFormatter.GetResultsOfGame(game, roundsInGame);

            return Ok(resultString);
        }

        [HttpGet("{gameId}/stat/current")]
        public async Task<IActionResult> GetCurrentStatisticsOfGame(int gameId)
        {
            var game = await service.GetGame(gameId);
            if (game == null)
                return BadRequest($"Игра с Id {gameId} не существует");

            var roundsInGame = await service.GetRoundsInGame(gameId);
            if (roundsInGame.Count() == 0 ||
                (roundsInGame.Count() == 1 && roundsInGame[0].WinnerId == null))
                return BadRequest($"В игре с Id {gameId} ещё не сыграно ни одного раунда");

            var resultsFormatter = new ResultsFormatter(service);
            var resultString = resultsFormatter.GetCurrentResults(game, roundsInGame);
            
            return Ok(resultString);
        }
    }
}
