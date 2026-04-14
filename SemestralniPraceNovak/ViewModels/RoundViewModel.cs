using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SemestralniPraceNovak.ViewModels
{
    public partial class RoundViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        
        public ObservableCollection<MatchViewModel> Matches { get; } = new();

        
        private readonly TournamentViewModel _parentTournament;

        public RoundViewModel(string name, TournamentViewModel parent)
        {
            Name = name;
            _parentTournament = parent;
        }

        [RelayCommand]
        private void GenerateMatches()
        {
           
            Matches.Clear();


			var teams = _parentTournament.Standings.Select(s => s.Team).ToList();

			if (teams.Count < 2) return; 

            
            var rng = new Random();
            var shuffled = teams.OrderBy(_ => rng.Next()).ToList();

            for (int i = 0; i < shuffled.Count - 1; i += 2)
            {
                Matches.Add(new MatchViewModel 
                { 
                    HomeTeam = shuffled[i].Name, 
                    AwayTeam = shuffled[i+1].Name 
                });
            }
        }
    }

    
    public partial class MatchViewModel : ObservableObject
    {
        [ObservableProperty] private string _homeTeam;
        [ObservableProperty] private string _awayTeam;
        public string MatchText => $"{HomeTeam} vs {AwayTeam}";
    }
}