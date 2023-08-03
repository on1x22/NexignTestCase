using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace RockPaperScissors.DAL.ContextModels
{
    public class Player
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        //public Guid Code { get; set; }
        [NotMapped]
        public const int COMPUTER_ID = -1;

        public virtual ICollection<Game> PlayerOneGames { get; set; }
        public virtual ICollection<Game> PlayerTwoGames { get; set; }

        public virtual ICollection<Round> Rounds { get; set; }
    }
}
