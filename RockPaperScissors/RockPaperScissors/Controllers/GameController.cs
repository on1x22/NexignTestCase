using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RockPaperScissors.DAL.Contexts;
using RockPaperScissors.Repository;

namespace RockPaperScissors.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameRepository repository;

        public GameController(IGameRepository repository) 
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

        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromQuery] string playerName) 
        {
            var player1 = await repository.CreatePlayer(playerName);
            var game = await repository.CreateGame(player1);
            if (game == null)
                return BadRequest();

            return Ok($"Игрок с кодом {game.PlayerOneId} создал игру {game.Id}");
        }
        
        /*// GET: GameController
        public ActionResult Index()
        {
            return View();
        }

        // GET: GameController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: GameController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: GameController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: GameController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: GameController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: GameController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: GameController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }*/
    }
}
