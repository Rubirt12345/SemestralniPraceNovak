using Microsoft.EntityFrameworkCore;
using SemestralniPraceNovak.Database;
using SemestralniPraceNovak.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SemestralniPraceNovak.Services
{
	public class SportService
	{
		private readonly AppDbContext _context;

		public SportService(AppDbContext context)
		{
			_context = context;
		}

		public async Task<List<Sport>> GetAllSportsAsync()
		{
			return await _context.Sports.ToListAsync();
		}

		public async Task<Sport> GetSportByIdAsync(int id)
		{
			return await _context.Sports.FindAsync(id);
		}

		public async Task<Sport> CreateSportAsync(string name, string description)
		{
			var sport = new Sport { Name = name, Description = description };
			_context.Sports.Add(sport);
			await _context.SaveChangesAsync();
			return sport;
		}

		public async Task<Sport> UpdateSportAsync(int id, string name, string description)
		{
			var sport = await GetSportByIdAsync(id);
			if (sport == null) return null;

			sport.Name = name;
			sport.Description = description;
			await _context.SaveChangesAsync();
			return sport;
		}

		public async Task<bool> DeleteSportAsync(int id)
		{
			var sport = await GetSportByIdAsync(id);
			if (sport == null) return false;

			_context.Sports.Remove(sport);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}