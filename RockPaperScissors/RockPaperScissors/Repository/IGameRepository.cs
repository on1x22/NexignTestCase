using RockPaperScissors.DAL.ContextModels;

namespace RockPaperScissors.Repository
{
    public interface IGameRepository
    {
        Task CreatePlayer_test(string playerName);

        Task<List<Player>> GetAllPlayers();

        Task<Player> CreatePlayer(string playerName);
        Task<Game> CreateGame(Player player);
    }
}
