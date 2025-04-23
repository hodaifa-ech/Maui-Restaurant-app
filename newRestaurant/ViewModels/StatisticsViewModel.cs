// ViewModels/StatisticsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models; // For UserRole
using newRestaurant.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls; // For Shell

namespace newRestaurant.ViewModels
{
    public partial class StatisticsViewModel : BaseViewModel // Assuming BaseViewModel exists
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private decimal _totalRevenue;

        [ObservableProperty]
        private ObservableCollection<KeyValuePair<string, int>> _popularDishes = new();

        // Role property to control visibility/access
        [ObservableProperty]
        private bool _canViewStatistics;

        public StatisticsViewModel(IStatisticsService statisticsService, IAuthService authService)
        {
            _statisticsService = statisticsService;
            _authService = authService;
            Title = "Restaurant Statistics";

            _authService.PropertyChanged += AuthService_PropertyChanged;
            UpdateRolePermissions();
        }

        private void AuthService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAuthService.CurrentUser))
            {
                UpdateRolePermissions();
                // Reload stats if user changes (e.g., logs out, different admin logs in)
                if (CanViewStatistics) LoadStatisticsCommand.ExecuteAsync(null);
                else ClearStatistics(); // Clear if user no longer has permission
            }
        }

        private void UpdateRolePermissions()
        {
            var user = _authService.CurrentUser;
            CanViewStatistics = user != null && (user.Role == UserRole.Staff || user.Role == UserRole.Admin);
        }

        private void ClearStatistics()
        {
            TotalRevenue = 0;
            PopularDishes.Clear();
        }

        [RelayCommand(CanExecute = nameof(CanViewStatistics))] // Only load if allowed
        private async Task LoadStatisticsAsync()
        {
            // Extra guard
            if (!CanViewStatistics)
            {
                ClearStatistics();
                return;
            }

            if (IsBusy) return;
            IsBusy = true;
            try
            {
                TotalRevenue = await _statisticsService.GetTotalRevenueAsync();

                var dishesData = await _statisticsService.GetPopularDishesAsync(5); // Get top 5
                PopularDishes.Clear();
                foreach (var dish in dishesData)
                {
                    PopularDishes.Add(dish);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading statistics: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load statistics.", "OK");
                ClearStatistics(); // Clear on error
            }
            finally
            {
                IsBusy = false;
            }
        }
        // Remember to implement IDisposable or similar to unsubscribe PropertyChanged
    }
}