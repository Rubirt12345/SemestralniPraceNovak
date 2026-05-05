using System.Collections.Generic;

namespace SemestralniPraceNovak.Models
{
    public class Round
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }
        public int RoundNumber { get; set; }
        public string Name { get; set; }
        public IList<Match> Matches { get; set; } = new List<Match>();
    }
}