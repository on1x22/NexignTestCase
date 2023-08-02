using RockPaperScissors.DAL.ContextModels;

namespace RockPaperScissors.Repository
{
    public interface IGameRepository
    {
        Task CreatePlayer_test(string playerName);

        Task<List<Player>> GetAllPlayers();
        Task<List<Game>> GetAllGames();
        Task<List<Round>> GetAllRounds();

        Task<Player> CreatePlayer(string playerName);
        Task<Game> CreateGame(Player player);
        Task<Game> FindGame(int gameId);

        Task ConnectSecondPlayerToTheGame(Game game, Player secondPlayer);
        //Task<Round> CreateRound(Round round);
    }
}
