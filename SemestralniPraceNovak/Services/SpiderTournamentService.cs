using Microsoft.EntityFrameworkCore;
using SemestralniPraceNovak.Database;
using SemestralniPraceNovak.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SemestralniPraceNovak.Services
{
    public class SpiderTournamentService
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new();

        public SpiderTournamentService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<SpiderTournament> CreateSpiderTournamentAsync(string name, int tournamentId, int numberOfParticipants)
        {
            var tournament = new SpiderTournament
            {
                Name = name,
                TournamentId = tournamentId,
                NumberOfParticipants = numberOfParticipants,
                Status = SpiderTournamentStatus.Preparation
            };

            _context.SpiderTournaments.Add(tournament);
            await _context.SaveChangesAsync();
            return tournament;
        }
        public async Task<SpiderTournament> GetSpiderTournamentByIdAsync(int id)
        {
            return await _context.SpiderTournaments
                .Include(t => t.Participants).ThenInclude(p => p.Player)
                .Include(t => t.Participants).ThenInclude(p => p.Team)
                .Include(t => t.Rounds).ThenInclude(r => r.Matches)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task AddPlayerAsync(int tournamentId, int playerId)
        {
            _context.SpiderParticipants.Add(new SpiderParticipant
            {
                SpiderTournamentId = tournamentId,
                PlayerId = playerId
            });

            await _context.SaveChangesAsync();
        }
        public async Task AddTeamAsync(int tournamentId, int teamId)
        {
            _context.SpiderParticipants.Add(new SpiderParticipant
            {
                SpiderTournamentId = tournamentId,
                TeamId = teamId
            });

            await _context.SaveChangesAsync();
        }
        public async Task InitializeSpiderAsync(int tournamentId)
        {
            var tournament = await GetSpiderTournamentByIdAsync(tournamentId);
            if (tournament == null) return;

            var participants = tournament.Participants
                .OrderBy(x => _random.Next())
                .ToList();

            if (participants.Count < 2) return;

            int roundsCount = (int)Math.Ceiling(Math.Log2(participants.Count));

            var rounds = new List<SpiderRound>();
            for (int i = 0; i < roundsCount; i++)
            {
                var round = new SpiderRound
                {
                    SpiderTournamentId = tournamentId,
                    RoundNumber = i + 1,
                    Name = $"Kolo {i + 1}"
                };

                _context.SpiderRounds.Add(round);
                rounds.Add(round);
            }

            await _context.SaveChangesAsync();
            rounds = await _context.SpiderRounds
                .Where(r => r.SpiderTournamentId == tournamentId)
                .OrderBy(r => r.RoundNumber)
                .ToListAsync();

            var allMatches = new List<SpiderMatch>();
            int matchCount = participants.Count;
            foreach (var round in rounds)
            {
                matchCount = (int)Math.Ceiling(matchCount / 2.0);

                for (int i = 0; i < matchCount; i++)
                {
                    var match = new SpiderMatch
                    {
                        SpiderRoundId = round.Id,
                        SpiderTournamentId = tournamentId,
                        OrderInRound = i
                    };

                    _context.SpiderMatches.Add(match);
                    allMatches.Add(match);
                }
            }
            await _context.SaveChangesAsync();
            allMatches = await _context.SpiderMatches
                .Where(m => m.SpiderTournamentId == tournamentId)
                .OrderBy(m => m.SpiderRoundId)
                .ThenBy(m => m.OrderInRound)
                .ToListAsync();
            for (int r = 0; r < rounds.Count - 1; r++)
            {
                var current = allMatches.Where(m => m.SpiderRoundId == rounds[r].Id).ToList();
                var next = allMatches.Where(m => m.SpiderRoundId == rounds[r + 1].Id).ToList();

                for (int i = 0; i < current.Count; i++)
                {
                    current[i].NextMatchId = next[i / 2].Id;
                    current[i].NextMatchSlot = (i % 2 == 0) ? 1 : 2;
                }
            }
            var firstRound = allMatches
                .Where(m => m.SpiderRoundId == rounds[0].Id)
                .ToList();

            int index = 0;
            foreach (var match in firstRound)
            {
                if (index < participants.Count)
                    match.Participant1Id = participants[index++].Id;

                if (index < participants.Count)
                    match.Participant2Id = participants[index++].Id;

                
                if (match.Participant2Id == null && match.Participant1Id != null)
                {
                    match.WinnerId = match.Participant1Id;
                    match.IsCompleted = true;
                }
            }

            tournament.Status = SpiderTournamentStatus.InProgress;
            await _context.SaveChangesAsync();
        }
        public async Task SetMatchResultAsync(int matchId, int score1, int score2)
        {
            var match = await _context.SpiderMatches.FindAsync(matchId);
            if (match == null) return;

            match.Score1 = score1;
            match.Score2 = score2;
            match.IsCompleted = true;

            match.WinnerId = score1 > score2
                ? match.Participant1Id
                : match.Participant2Id;

            await UpdateStats(match);
            if (match.NextMatchId != null && match.WinnerId != null)
            {
                var next = await _context.SpiderMatches.FindAsync(match.NextMatchId);

                if (match.NextMatchSlot == 1)
                    next.Participant1Id = match.WinnerId;
                else
                    next.Participant2Id = match.WinnerId;
            }

            await _context.SaveChangesAsync();
        }
        private async Task UpdateStats(SpiderMatch match)
        {
            var p1 = await _context.SpiderParticipants.FindAsync(match.Participant1Id);
            var p2 = await _context.SpiderParticipants.FindAsync(match.Participant2Id);

            if (p1 != null) p1.MatchesPlayed++;
            if (p2 != null) p2.MatchesPlayed++;

            if (match.Score1 > match.Score2)
            {
                if (p1 != null) { p1.Wins++; p1.Points += 3; }
                if (p2 != null) p2.Losses++;
            }
            else if (match.Score2 > match.Score1)
            {
                if (p2 != null) { p2.Wins++; p2.Points += 3; }
                if (p1 != null) p1.Losses++;
            }
        }
        public async Task<List<SpiderMatch>> GetMatchesAsync(int roundId)
        {
            return await _context.SpiderMatches
                .Where(m => m.SpiderRoundId == roundId)
                .Include(m => m.Participant1).ThenInclude(p => p.Player)
                .Include(m => m.Participant2).ThenInclude(p => p.Player)
                .ToListAsync();
        }
        public async Task<List<SpiderParticipant>> GetStandingsAsync(int tournamentId)
        {
            return await _context.SpiderParticipants
                .Where(p => p.SpiderTournamentId == tournamentId)
                .OrderByDescending(p => p.Points)
                .ToListAsync();
        }
        public async Task<List<SpiderTournament>> GetSpiderTournamentsByTournamentIdAsync(int tournamentId)
        {
            return await _context.SpiderTournaments
                .Where(t => t.TournamentId == tournamentId)
                .ToListAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var t = await _context.SpiderTournaments.FindAsync(id);
            if (t == null) return;

            _context.SpiderTournaments.Remove(t);
            await _context.SaveChangesAsync();
        }
    }
}