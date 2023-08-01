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
        public virtual Game Game { get; set; }
        
        //[Column("WinnerId")]
        public int WinnerId { get; set; }
        [ForeignKey("WinnerId")]
        public virtual Player Player { get; set; }

    }
}
