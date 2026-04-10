using System.Collections.Generic;

namespace SemestralniPraceNovak.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<TournamentParticipant> Tournaments { get; set; } = new List<TournamentParticipant>();
        public ICollection<Match> MatchesAsTeam1 { get; set; } = new List<Match>();
        public ICollection<Match> MatchesAsTeam2 { get; set; } = new List<Match>();
    }
}