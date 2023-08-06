using RockPaperScissors.DAL.ContextModels;

namespace RockPaperScissors.DAL.Repository
{
    public interface IGameRepository
    {
        Task<Player> GetPlayer(string playerName);

        Task/*<Player>*/ CreatePlayer(/*string playerName, int? id = null*/Player player);

        Task/*<Game>*/ CreateGame(/*Player player*/ Game game);

        Task CreateNewRound(Round round);

        Task<Game> GetGame(int gameId);

        //Task ConnectSecondPlayerToTheGame(Game game, Player secondPlayer);
        Task ConnectSecondPlayerToTheGame(Game game/*, Player secondPlayer*/);

        Task/*<string>*/ MakeTurn(int gameId, int playerId, string turn);

        void/*<string>*/ WriteTurn(/*string playerOneTurn, string playerTwoTurn, string winnerId*/ Round round);

        Task<Round> GetLastRoundInGame(int gameId);

        Task<bool> CheckPlayerInGame(int gameId, int playerId);

        Task<int?> CheckWhoseTurn(int gameId);

        Task<List<Round>> GetRoundsInGame(int gameId);

        Task SaveChanges();
    }
}
