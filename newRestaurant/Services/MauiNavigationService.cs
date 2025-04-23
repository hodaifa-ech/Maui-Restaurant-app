// --- Services/Interfaces/INavigationService.cs ---
// No changes needed to the interface itself.

// --- Services/MauiNavigationService.cs ---
using Microsoft.Maui.Controls; // Ensure this is present
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace newRestaurant.Services
{
    public class MauiNavigationService : INavigationService
    {
        // Inject IServiceProvider to lazily get Shell when needed
        // Or inject IApplication directly if preferred
        private readonly IServiceProvider _services;

        public MauiNavigationService(IServiceProvider services)
        {
            _services = services;
        }


        // Helper property to safely get Shell
        private Shell CurrentShell => Shell.Current;

        // Helper to get the current Page's Navigation if Shell is not available
        private INavigation Navigation
        {
            get
            {
                // Prioritize Shell navigation if Shell is active
                if (CurrentShell?.Navigation != null)
                {
                    return CurrentShell.Navigation;
                }
                // Fallback to Application's MainPage navigation
                else if (Application.Current?.MainPage?.Navigation != null)
                {
                    return Application.Current.MainPage.Navigation;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Warning: No Navigation available (Shell or MainPage is null).");
                    // Returning null might cause issues downstream, consider throwing?
                    return null;
                }
            }
        }


        public Task NavigateToAsync(string route)
        {
            // Prefer Shell navigation if available
            if (CurrentShell != null)
            {
                // Use GoToAsync for Shell routing
                return CurrentShell.GoToAsync(route);
            }
            else
            {
                // Fallback: Page-based navigation if Shell isn't the MainPage yet
                // This requires resolving the page type from the route name.
                System.Diagnostics.Debug.WriteLine($"Shell not current, attempting page navigation for route: {route}");
                // Simplified: Assume route is the Page Type Name for non-shell nav
                var pageType = Type.GetType($"newRestaurant.Views.{route}"); // NEEDS correct namespace
                if (pageType != null)
                {
                    var page = _services.GetService(pageType) as Page;
                    if (page != null && Navigation != null)
                    {
                        return Navigation.PushAsync(page);
                    }
                }
                Console.WriteLine($"Warning: Could not resolve page or navigation for route '{route}' during non-Shell navigation.");
                return Task.CompletedTask; // Indicate failure or do nothing
            }
        }

        public Task NavigateToAsync(string route, IDictionary<string, object> parameters)
        {
            // Prefer Shell navigation if available
            if (CurrentShell != null)
            {
                return CurrentShell.GoToAsync(route, parameters);
            }
            else
            {
                // Page-based navigation typically doesn't handle dictionary parameters directly
                // You might need to manually pass data via constructor or properties after resolving the page
                System.Diagnostics.Debug.WriteLine($"Warning: Parameter navigation requested ('{route}') but Shell not active. Parameters ignored for page navigation.");
                // Attempt simple navigation without parameters as a fallback
                return NavigateToAsync(route);
            }
        }

        public Task GoBackAsync()
        {
            if (Navigation != null)
            {
                // Use PopAsync for standard page navigation stack
                // Shell's ".." handles Shell navigation stack correctly
                if (CurrentShell != null && Navigation == CurrentShell.Navigation)
                {
                    return CurrentShell.GoToAsync(".."); // Use Shell back navigation
                }
                else if (Navigation.NavigationStack.Count > 1) // Check if there's a page to pop back to
                {
                    return Navigation.PopAsync(); // Use page back navigation
                }
            }
            System.Diagnostics.Debug.WriteLine("Warning: Could not execute GoBackAsync - Navigation unavailable or stack empty.");
            return Task.CompletedTask;
        }
    }
}