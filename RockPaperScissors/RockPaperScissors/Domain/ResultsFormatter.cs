using RockPaperScissors.DAL.ContextModels;
using System.Text;

namespace RockPaperScissors.Domain
{
    public class ResultsFormatter
    {
        private readonly IGameService service;

        public ResultsFormatter(IGameService service)
        {
            this.service = service;
        }

        public string GetCurrentResults(Game game, List<Round> roundsInGame)
        {
            var resultString = new StringBuilder();

            resultString.AppendLine($"Id игры {game.Id}\nСтатистика по сыгранным раундам:");

            AddStatisticsByRounds(ref resultString, game, roundsInGame);

            return resultString.ToString();
        }

        public async Task<string> GetResultsOfGame(Game game, List<Round> roundsInGame)
        {
            var resultString = new StringBuilder();
            resultString.AppendLine($"Id игры {game.Id}");
            
            AddStatisticsByRounds(ref resultString, game, roundsInGame);

            resultString.Append(service.GetWinnerOfGame(await service.CheckWinnerOfGame(game.Id)) + "\n\n");

            return resultString.ToString();
        }

        private void AddStatisticsByRounds(ref StringBuilder resultString, Game game, List<Round> roundsInGame)
        {
            foreach (var round in roundsInGame)
            {
                if (round.WinnerId != null)
                    resultString.Append(service.GetStatisticsOfRound(game, round));
            }
        }
    }
}
