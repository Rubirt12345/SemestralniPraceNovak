namespace SemestralniPraceNovak.Models
{
    public class Match
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public Round Round { get; set; }

        
        public int? Player1Id { get; set; }
        public Player Player1 { get; set; }

        public int? Player2Id { get; set; }
        public Player Player2 { get; set; }

       
        public int? Team1Id { get; set; }
        public Team Team1 { get; set; }

        public int? Team2Id { get; set; }
        public Team Team2 { get; set; }

        
        public int? Score1 { get; set; }
        public int? Score2 { get; set; }
        public int? WinnerId { get; set; }
        public bool IsCompleted { get; set; }
    }
}