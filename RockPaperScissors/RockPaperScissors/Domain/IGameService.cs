using RockPaperScissors.DAL.ContextModels;

namespace RockPaperScissors.Domain
{
    public interface IGameService
    {
        Task<Player> CreatePlayer(string playerName, int? id = null);

        Task<Game> CreateGame(Player player);

        Task<Game> GetGame(int gameId);

        Task<Player> GetPlayer(string playerName);

        Task ConnectSecondPlayerToTheGame(Game game, int secondPlayerId);

        Task<bool> CheckPlayerInGame(int gameId, int playerId);

        Task<int?> CheckWhoseTurn(int gameId);

        Task<List<Round>> GetRoundsInGame(int gameId);

        Task<string> MakeTurn(int gameId, int playerId, string turn);

        Task<Round> GetLastRoundInGame(int gameId);
    }
}
