using RockPaperScissors.DAL.ContextModels;

namespace RockPaperScissors.DAL.Repository
{
    public interface IGameRepository
    {
        Task<Player> GetPlayer(string playerName);

        Task CreatePlayer(Player player);

        Task CreateGame( Game game);

        Task CreateNewRound(Round round);

        Task<Game> GetGame(int gameId);

        Task ConnectSecondPlayerToTheGame(Game game);

        void WriteTurn(Round round);

        Task<Round> GetLastRoundInGame(int gameId);

        Task<bool> CheckPlayerInGame(int gameId, int playerId);

        Task<List<Round>> GetRoundsInGame(int gameId);

        Task SaveChanges();
    }
}
