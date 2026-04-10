using Microsoft.EntityFrameworkCore;
using SemestralniPraceNovak.Database;
using SemestralniPraceNovak.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SemestralniPraceNovak.Services
{
    public class PlayerService
    {
        private readonly AppDbContext _context;

        public PlayerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Player>> GetAllPlayersAsync()
        {
            return await _context.Players.Where(p => p.IsActive).ToListAsync();
        }

        public async Task<Player> GetPlayerByIdAsync(int id)
        {
            return await _context.Players.Include(p => p.Tournaments).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Player> CreatePlayerAsync(string name)
        {
            var player = new Player { Name = name, IsActive = true };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            return player;
        }

        public async Task<Player> UpdatePlayerAsync(int id, string name)
        {
            var player = await GetPlayerByIdAsync(id);
            if (player == null) return null;

            player.Name = name;
            await _context.SaveChangesAsync();
            return player;
        }

        public async Task<bool> DeletePlayerAsync(int id)
        {
            var player = await GetPlayerByIdAsync(id);
            if (player == null) return false;

            player.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        
        public async Task<List<Team>> GetAllTeamsAsync()
        {
            return await _context.Teams.Where(t => t.IsActive).ToListAsync();
        }

        public async Task<Team> GetTeamByIdAsync(int id)
        {
            return await _context.Teams.Include(t => t.Tournaments).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Team> CreateTeamAsync(string name)
        {
            var team = new Team { Name = name, IsActive = true };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<Team> UpdateTeamAsync(int id, string name)
        {
            var team = await GetTeamByIdAsync(id);
            if (team == null) return null;

            team.Name = name;
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<bool> DeleteTeamAsync(int id)
        {
            var team = await GetTeamByIdAsync(id);
            if (team == null) return false;

            team.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}