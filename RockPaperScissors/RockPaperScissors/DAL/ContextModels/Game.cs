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

        //[ForeignKey("PlayerOneId")]
        public virtual Player PlayerOne { get; set; }
        //[ForeignKey("PlayerTwoId")]
        public virtual Player PlayerTwo { get; set; }

        //public List<Round> Rounds { get; set; }
        public virtual ICollection<Round> Rounds { get; set; }
    }
}
