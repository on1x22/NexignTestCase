using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RockPaperScissors.DAL.ContextModels
{
    public class Round
    {
        [Key]
        public int Id { get; set; }
        public int RoundNumber { get; set; }
        public int GameId { get; set; }
        public string PlayerOneTurn { get; set; }
        public string PlayerTwoTurn { get; set; }
        public int WinnerId { get; set; }

        //[NotMapped]
        //public const int MAX_ROUNDS = 5;


        public virtual Game Game { get; set; }
        //[Column("WinnerId")]
        
        [ForeignKey("WinnerId")]
        public virtual Player Player { get; set; }

    }
}
