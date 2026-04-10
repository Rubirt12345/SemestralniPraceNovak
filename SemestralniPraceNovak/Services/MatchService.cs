using Microsoft.EntityFrameworkCore;
using SemestralniPraceNovak.Database;
using SemestralniPraceNovak.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SemestralniPraceNovak.Services
{
    public class MatchService
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new Random();

        public MatchService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Match>> GetRoundMatchesAsync(int roundId)
        {
            return await _context.Matches
                .Where(m => m.RoundId == roundId)
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .ToListAsync();
        }

        
        public async Task DrawPlayersMatchesAsync(int roundId)
        {
            var round = await _context.Rounds
                .Include(r => r.Tournament).ThenInclude(t => t.Participants)
                .FirstOrDefaultAsync(r => r.Id == roundId);

            if (round == null) return;

            var participants = round.Tournament.Participants
                .Where(p => p.PlayerId.HasValue)
                .Select(p => p.PlayerId.Value)
                .ToList();

            
            for (int i = participants.Count - 1; i > 0; i--)
            {
                int randomIndex = _random.Next(i + 1);
                (participants[i], participants[randomIndex]) = (participants[randomIndex], participants[i]);
            }

           
            for (int i = 0; i < participants.Count - 1; i += 2)
            {
                var match = new Match
                {
                    RoundId = roundId,
                    Player1Id = participants[i],
                    Player2Id = participants[i + 1]
                };
                _context.Matches.Add(match);
            }

            await _context.SaveChangesAsync();
        }

        
        public async Task DrawTeamsMatchesAsync(int roundId)
        {
            var round = await _context.Rounds
                .Include(r => r.Tournament).ThenInclude(t => t.Participants)
                .FirstOrDefaultAsync(r => r.Id == roundId);

            if (round == null) return;

            var participants = round.Tournament.Participants
                .Where(p => p.TeamId.HasValue)
                .Select(p => p.TeamId.Value)
                .ToList();

            
            for (int i = participants.Count - 1; i > 0; i--)
            {
                int randomIndex = _random.Next(i + 1);
                (participants[i], participants[randomIndex]) = (participants[randomIndex], participants[i]);
            }

            
            for (int i = 0; i < participants.Count - 1; i += 2)
            {
                var match = new Match
                {
                    RoundId = roundId,
                    Team1Id = participants[i],
                    Team2Id = participants[i + 1]
                };
                _context.Matches.Add(match);
            }

            await _context.SaveChangesAsync();
        }

        
        public async Task<Match> SetMatchResultAsync(int matchId, int score1, int score2)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null) return null;

            match.Score1 = score1;
            match.Score2 = score2;
            match.IsCompleted = true;

            
            if (score1 > score2)
            {
                match.WinnerId = match.Player1Id ?? match.Team1Id;
            }
            else if (score2 > score1)
            {
                match.WinnerId = match.Player2Id ?? match.Team2Id;
            }

            
            await UpdateParticipantStatsAsync(match);

            await _context.SaveChangesAsync();
            return match;
        }

        private async Task UpdateParticipantStatsAsync(Match match)
        {
            var round = await _context.Rounds.FindAsync(match.RoundId);
            var tournament = await _context.Tournaments.FindAsync(round.TournamentId);

            if (match.Player1Id.HasValue)
            {
                var participant1 = await _context.TournamentParticipants
                    .FirstOrDefaultAsync(p => p.TournamentId == tournament.Id && p.PlayerId == match.Player1Id);
                var participant2 = await _context.TournamentParticipants
                    .FirstOrDefaultAsync(p => p.TournamentId == tournament.Id && p.PlayerId == match.Player2Id);

                if (participant1 != null && participant2 != null)
                {
                    if (match.Score1 > match.Score2)
                    {
                        participant1.Wins++;
                        participant1.Points += 3;
                        participant2.Losses++;
                    }
                    else if (match.Score2 > match.Score1)
                    {
                        participant2.Wins++;
                        participant2.Points += 3;
                        participant1.Losses++;
                    }
                    else
                    {
                        participant1.Draws++;
                        participant2.Draws++;
                        participant1.Points += 1;
                        participant2.Points += 1;
                    }
                }
            }
            else if (match.Team1Id.HasValue)
            {
                var participant1 = await _context.TournamentParticipants
                    .FirstOrDefaultAsync(p => p.TournamentId == tournament.Id && p.TeamId == match.Team1Id);
                var participant2 = await _context.TournamentParticipants
                    .FirstOrDefaultAsync(p => p.TournamentId == tournament.Id && p.TeamId == match.Team2Id);

                if (participant1 != null && participant2 != null)
                {
                    if (match.Score1 > match.Score2)
                    {
                        participant1.Wins++;
                        participant1.Points += 3;
                        participant2.Losses++;
                    }
                    else if (match.Score2 > match.Score1)
                    {
                        participant2.Wins++;
                        participant2.Points += 3;
                        participant1.Losses++;
                    }
                    else
                    {
                        participant1.Draws++;
                        participant2.Draws++;
                        participant1.Points += 1;
                        participant2.Points += 1;
                    }
                }
            }
        }

        
        public async Task<List<Match>> GetEliminationBracketAsync(int tournamentId)
        {
            return await _context.Matches
                .Include(m => m.Round)
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .Where(m => m.Round.TournamentId == tournamentId)
                .OrderBy(m => m.Round.RoundNumber)
                .ThenBy(m => m.Id)
                .ToListAsync();
        }
    }
}