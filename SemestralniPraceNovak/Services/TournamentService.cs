using Microsoft.EntityFrameworkCore;
using SemestralniPraceNovak.Database;
using SemestralniPraceNovak.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SemestralniPraceNovak.Services
{
    public class TournamentService
    {
        private readonly AppDbContext _context;

        public TournamentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tournament>> GetAllTournamentsAsync()
        {
            return await _context.Tournaments
                .Include(t => t.Sport)
                .Include(t => t.Rounds)
                .Include(t => t.Participants)
                .ToListAsync();
        }

        public async Task<Tournament> GetTournamentByIdAsync(int id)
        {
            return await _context.Tournaments
                .Include(t => t.Sport)
                .Include(t => t.Rounds).ThenInclude(r => r.Matches)
                .Include(t => t.Participants)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Tournament> CreateTournamentAsync(string name, int sportId)
        {
            var tournament = new Tournament { Name = name, SportId = sportId, Status = TournamentStatus.Planning };
            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();
            return tournament;
        }

        public async Task<Tournament> UpdateTournamentAsync(int id, string name, TournamentStatus status)
        {
            var tournament = await GetTournamentByIdAsync(id);
            if (tournament == null) return null;

            tournament.Name = name;
            tournament.Status = status;
            await _context.SaveChangesAsync();
            return tournament;
        }

        public async Task<bool> DeleteTournamentAsync(int id)
        {
            var tournament = await GetTournamentByIdAsync(id);
            if (tournament == null) return false;

            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<TournamentParticipant> AddPlayerToTournamentAsync(int tournamentId, int playerId)
        {
            var participant = new TournamentParticipant { TournamentId = tournamentId, PlayerId = playerId };
            _context.TournamentParticipants.Add(participant);
            await _context.SaveChangesAsync();
            return participant;
        }

        public async Task<TournamentParticipant> AddTeamToTournamentAsync(int tournamentId, int teamId)
        {
            var participant = new TournamentParticipant { TournamentId = tournamentId, TeamId = teamId };
            _context.TournamentParticipants.Add(participant);
            await _context.SaveChangesAsync();
            return participant;
        }

        public async Task<bool> RemoveParticipantAsync(int participantId)
        {
            var participant = await _context.TournamentParticipants.FindAsync(participantId);
            if (participant == null) return false;

            _context.TournamentParticipants.Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<Round> CreateRoundAsync(int tournamentId, int roundNumber, string name)
        {
            var round = new Round { TournamentId = tournamentId, RoundNumber = roundNumber, Name = name };
            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();
            return round;
        }

        public async Task<bool> DeleteRoundAsync(int roundId)
        {
            var round = await _context.Rounds.FindAsync(roundId);
            if (round == null) return false;

            _context.Rounds.Remove(round);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<List<TournamentParticipant>> GetTournamentStandingsAsync(int tournamentId)
        {
            return await _context.TournamentParticipants
                .Where(tp => tp.TournamentId == tournamentId)
                .Include(tp => tp.Player)
                .Include(tp => tp.Team)
                .OrderByDescending(tp => tp.Points)
                .ThenByDescending(tp => tp.Wins)
                .ToListAsync();
        }
        public async Task AddTeamParticipantAsync(int tournamentId, int teamId)
        {
            var participant = new TournamentParticipant
            {
                TournamentId = tournamentId,
                TeamId = teamId
            };
            _context.TournamentParticipants.Add(participant);
            await _context.SaveChangesAsync();
        }
    }
}