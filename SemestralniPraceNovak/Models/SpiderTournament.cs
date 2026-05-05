using System;
using System.Collections.Generic;

namespace SemestralniPraceNovak.Models
{
    public class SpiderTournament
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public SpiderTournamentStatus Status { get; set; } = SpiderTournamentStatus.Preparation;
        public int NumberOfRounds { get; set; }
        public int NumberOfParticipants { get; set; }
        public ICollection<SpiderRound> Rounds { get; set; } = new List<SpiderRound>();
        public ICollection<SpiderMatch> Matches { get; set; } = new List<SpiderMatch>();
        public ICollection<SpiderParticipant> Participants { get; set; } = new List<SpiderParticipant>();
    }
    public enum SpiderTournamentStatus
    {
        Preparation,    
        InProgress,     
        Completed       
    }
    public class SpiderRound
    {
        public int Id { get; set; }
        public int SpiderTournamentId { get; set; }
        public SpiderTournament SpiderTournament { get; set; }
        public int RoundNumber { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<SpiderMatch> Matches { get; set; } = new List<SpiderMatch>();
    }
    public class SpiderMatch
    {
        public int Id { get; set; }

        public int SpiderRoundId { get; set; }
        public SpiderRound Round { get; set; }

        public int SpiderTournamentId { get; set; }
        public SpiderTournament SpiderTournament { get; set; }

        public int? Participant1Id { get; set; }
        public SpiderParticipant Participant1 { get; set; }

        public int? Participant2Id { get; set; }
        public SpiderParticipant Participant2 { get; set; }

        public int? Score1 { get; set; }
        public int? Score2 { get; set; }

        public bool IsCompleted { get; set; }
        public int? WinnerId { get; set; }

        
        public int? NextMatchId { get; set; }
        public SpiderMatch NextMatch { get; set; }

        
        public int? NextMatchSlot { get; set; }

       
        public int OrderInRound { get; set; }
    }
    public class SpiderParticipant
    {
        public int Id { get; set; }
        public int SpiderTournamentId { get; set; }
        public SpiderTournament SpiderTournament { get; set; }

        
        public int? PlayerId { get; set; }
        public Player Player { get; set; }

        public int? TeamId { get; set; }
        public Team Team { get; set; }

        
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int Points { get; set; }
        public int MatchesPlayed { get; set; }

        public string GetName() => Player != null ? Player.Name : Team?.Name ?? "Unknown";
    }
}