using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RockPaperScissors.DAL.ContextModels
{
    public class Game
    {
        [Key]
        public int Id { get; set; }
        //public string Name { get; set; }
        public int PlayerOneId { get; set; }
        public int PlayerTwoId { get; set; }

        [NotMapped]
        public const int MAX_ROUNDS = 5;
        [NotMapped]
        public const int WINS_IN_ROUNDS_TO_WIN_THE_GAME = 5;

        //[ForeignKey("PlayerOneId")]
        public virtual Player PlayerOne { get; set; }
        //[ForeignKey("PlayerTwoId")]
        public virtual Player PlayerTwo { get; set; }

        //public List<Round> Rounds { get; set; }
        public virtual ICollection<Round> Rounds { get; set; }
    }
}
