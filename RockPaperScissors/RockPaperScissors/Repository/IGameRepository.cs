using RockPaperScissors.DAL.ContextModels;

namespace RockPaperScissors.Repository
{
    public interface IGameRepository
    {
        Task CreatePlayer_test(string playerName);

        Task<List<Player>> GetAllPlayers();
        Task<List<Game>> GetAllGames();
        Task<List<Round>> GetAllRounds();

        Task<Player> GetPlayer(string playerName);
        Task<Player> CreatePlayer(string playerName);
        Task<Game> CreateGame(Player player);
        Task<Game> GetGame(int gameId);

        Task ConnectSecondPlayerToTheGame(Game game, Player secondPlayer);
        //Task<Round> CreateRound(Round round);

        Task<string> MakeTurn(int gameId, int playerId, string turn);
        Task<Round> GetLastRoundInGame(int gameId);

        Task<bool> CheckPlayerInGame(int gameId, int playerId);

        Task<int?> CheckWhoseTurn(int gameId);

        Task<List<Round>> GetRoundsInGame(int gameId);
    }
}
