using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RockPaperScissors.DAL.Contexts;
using RockPaperScissors.Repository;

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
            var game = await repository.FindGame(gameId);
            if (game == null)
                return BadRequest($"Игры с Id {gameId} не существует");

            var player2 = await repository.CreatePlayer(playerTwoName);
            //game.PlayerTwoId = player2.Id;
            await repository.ConnectSecondPlayerToTheGame(game, player2);

            return Ok($"Игрок с кодом {game.PlayerTwoId} подключился к игре {game.Id}");
        }
    }
}
