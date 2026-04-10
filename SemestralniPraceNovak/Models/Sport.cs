using System.Collections.Generic;

namespace SemestralniPraceNovak.Models
{
    public class Sport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
    }
}