using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RockPaperScissors.DAL.ContextModels;
using RockPaperScissors.DAL.Contexts;
using RockPaperScissors.Repository;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        [HttpPost("addplayer")]
        public async Task<IActionResult> AddPlayer([FromQuery]  string playerName)
        {
            if (playerName == null)
                return BadRequest("Не задано имя игрока");
            if (!ModelState.IsValid)
                return BadRequest("Некорректный запрос");

            await repository.CreatePlayer_test(playerName);

            return Ok();
        }

        [HttpGet("getallplayers")]
        public async Task<IActionResult> GetAllPlayers()
        {
            return Ok(await repository.GetAllPlayers());

        }

        [HttpGet("getallgames")]
        public async Task<IActionResult> GetAllGames()
        {
            return Ok(await repository.GetAllGames());

        }

        [HttpGet("getallrounds")]
        public async Task<IActionResult> GetAllRounds()
        {
            return Ok(await repository.GetAllRounds());

        }

        /*// GET: GameController
        public ActionResult Index()
        {
            return View();
        }*/

        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromQuery] string playerName) 
        {
            var player1 = await repository.CreatePlayer(playerName);
            var game = await repository.CreateGame(player1);
            if (game == null)
                return BadRequest();

            return Ok($"Игрок с кодом {game.PlayerOneId} создал игру {game.Id}");
        }

        [HttpPost("{gameId}/join/{playerTwoName}")]
        public async Task<IActionResult> ConnectSecondPlayerToTheGame(int gameId, string playerTwoName)
        {
            var game = await repository.GetGame(gameId);
            if (game == null)
                return BadRequest($"Игры с Id {gameId} не существует");

            var player2 = await repository.CreatePlayer(playerTwoName);
            //game.PlayerTwoId = player2.Id;
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
                return BadRequest($"Игрок с Id {gameId} ходит не в свой ход. " +
                    $"Ход необходимо выполнить другому игроку");

            var resultTurn = await repository.MakeTurn(gameId, playerId, turn);
            if (resultTurn == null) 
                return BadRequest("Задан некорректный ход. Должны быть только " +
                    "\"камень\", \"ножницы\" или \"бумага\"");

            return Ok($"Игрок {playerId} выполнил ход");
        }



    }
}
