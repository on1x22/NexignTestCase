using RockPaperScissors.DAL.ContextModels;

namespace RockPaperScissors.Repository
{
    public interface IGameRepository
    {
        Task<Player> GetPlayer(string playerName);

        Task<Player> CreatePlayer(string playerName, int? id = null);

        Task<Game> CreateGame(Player player);

        Task<Game> GetGame(int gameId);

        Task ConnectSecondPlayerToTheGame(Game game, Player secondPlayer);

        Task<string> MakeTurn(int gameId, int playerId, string turn);

        Task<Round> GetLastRoundInGame(int gameId);

        Task<bool> CheckPlayerInGame(int gameId, int playerId);

        Task<int?> CheckWhoseTurn(int gameId);

        Task<List<Round>> GetRoundsInGame(int gameId);
    }
}
