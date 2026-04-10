using Microsoft.EntityFrameworkCore;
using SemestralniPraceNovak.Models;
using System.Threading.Tasks;

namespace SemestralniPraceNovak.Database
{
	public class DatabaseInitializer
	{
		public static async Task InitializeAsync()
		{
			using (var context = new AppDbContext())
			{
                // Vytvoø databázi a aplikuj migrace
                await context.Database.EnsureCreatedAsync();

                // Seed data - základní sporty
                if (!await context.Sports.AnyAsync())
				{
					context.Sports.AddRange(
						new Sport { Name = "Fotbal", Description = "Kolektivní sport s míèem" },
						new Sport { Name = "Tenis", Description = "Individuální sport s raketou" },
						new Sport { Name = "Volejbal", Description = "Týmový sport s míèem" },
						new Sport { Name = "Badminton", Description = "Individuální sport s košíèkem" },
						new Sport { Name = "Stolní tenis", Description = "Individuální sport na stole" }
					);

					await context.SaveChangesAsync();
				}
			}
		}
	}
}