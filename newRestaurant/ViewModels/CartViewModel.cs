// ViewModels/CartViewModel.cs
namespace newRestaurant.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using newRestaurant.Models;
    using newRestaurant.Services.Interfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System; // For Exception
    using Microsoft.Maui.Controls; // For Shell
    using newRestaurant.Services;

    public partial class CartViewModel : BaseViewModel // Assuming BaseViewModel exists
    {
        [ObservableProperty]
        private ObservableCollection<CartPlat> _cartItems = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasItems))]
        private Cart _currentCart;

        // Update the TotalPrice property to remove the [NotifyPropertyChangedFor] attribute targeting CheckoutCommand.
        // Instead, manually call NotifyCanExecuteChanged for CheckoutCommand in the CalculateTotal method.

        [ObservableProperty]
        private decimal _totalPrice;

        public bool HasItems => CartItems?.Count > 0;

        private readonly ICartService _cartService;
        private readonly INavigationService _navigationService; // Keep if needed for post-checkout nav
        private readonly IAuthService _authService;

        public CartViewModel(ICartService cartService, INavigationService navigationService, IAuthService authService)
        {
            _cartService = cartService;
            _navigationService = navigationService;
            _authService = authService;
            Title = "My Cart";
            _authService.PropertyChanged += AuthService_PropertyChanged;
        }

        private async void AuthService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAuthService.CurrentUser))
            {
                // Reload cart when user changes
                await LoadCartCommand.ExecuteAsync(null);
            }
        }

        [RelayCommand]
        public async Task LoadCartAsync()
        {
            if (IsBusy) return;
            var userId = _authService.CurrentUser?.Id;
            if (userId == null)
            {
                CartItems.Clear(); CurrentCart = null; CalculateTotal();
                OnPropertyChanged(nameof(HasItems)); return;
            }
            IsBusy = true;
            try
            {
                CurrentCart = await _cartService.GetActiveCartAsync((int)userId);
                CartItems.Clear();
                if (CurrentCart?.CartPlats != null)
                {
                    // Re-create observable items for potential UI updates if CartPlat is ObservableObject
                    foreach (var item in CurrentCart.CartPlats.OrderBy(cp => cp.Plat?.Name ?? string.Empty))
                    {
                        // item.PropertyChanged += CartItem_PropertyChanged; // Listen to Quantity changes if needed
                        CartItems.Add(item);
                    }
                }
                CalculateTotal();
            }
            catch (Exception ex) { Debug.WriteLine($"Error loading cart: {ex.Message}"); await Shell.Current.DisplayAlert("Error", "Failed to load cart.", "OK"); }
            finally { IsBusy = false; OnPropertyChanged(nameof(HasItems)); }
        }

        // Update the CalculateTotal method to ensure CheckoutCommand.NotifyCanExecuteChanged is called when TotalPrice changes.
        private void CalculateTotal()
        {
            TotalPrice = CurrentCart?.TotalPrice ?? 0m; // Use calculated property from Cart model
                                                        // Or recalculate manually:
                                                        // TotalPrice = CartItems?.Sum(item => item.TotalLinePrice) ?? 0m;
            CheckoutCommand.NotifyCanExecuteChanged(); // Update button state based on total/items
        }

        [RelayCommand]
        private async Task UpdateQuantityAsync(CartPlat item)
        {
            if (item == null || IsBusy || CurrentCart == null) return;
            if (item.Quantity < 1) { await RemoveItemAsync(item); return; } // Remove if quantity drops below 1

            IsBusy = true;
            try
            {
                bool success = await _cartService.UpdateCartItemQuantityAsync(CurrentCart.Id, item.PlatId, item.Quantity);
                if (!success) { await Shell.Current.DisplayAlert("Error", "Failed to update quantity.", "OK"); await LoadCartAsync(); }
                else { CalculateTotal(); } // Recalculate on success
            }
            catch (Exception ex) { Debug.WriteLine($"Error updating quantity: {ex.Message}"); await Shell.Current.DisplayAlert("Error", "Error updating quantity.", "OK"); await LoadCartAsync(); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task IncreaseQuantityAsync(CartPlat item)
        {
            if (item == null || IsBusy || CurrentCart == null) return;
            item.Quantity++; // Directly modifies ObservableProperty
            await UpdateQuantityAsync(item);
        }

        [RelayCommand]
        private async Task DecreaseQuantityAsync(CartPlat item)
        {
            if (item == null || IsBusy || CurrentCart == null) return;
            if (item.Quantity > 1)
            {
                item.Quantity--; // Directly modifies ObservableProperty
                await UpdateQuantityAsync(item);
            }
            else { await RemoveItemAsync(item); } // Remove if quantity reaches 0
        }

        [RelayCommand]
        private async Task RemoveItemAsync(CartPlat item)
        {
            if (item == null || IsBusy || CurrentCart == null) return;
            IsBusy = true;
            try
            {
                bool success = await _cartService.RemoveItemFromCartAsync(CurrentCart.Id, item.PlatId);
                if (success)
                {
                    // item.PropertyChanged -= CartItem_PropertyChanged; // Unsubscribe if listening
                    CartItems.Remove(item); CalculateTotal(); OnPropertyChanged(nameof(HasItems));
                }
                else { await Shell.Current.DisplayAlert("Error", "Failed to remove item.", "OK"); await LoadCartAsync(); }
            }
            catch (Exception ex) { Debug.WriteLine($"Error removing item: {ex.Message}"); await Shell.Current.DisplayAlert("Error", "Error removing item.", "OK"); await LoadCartAsync(); }
            finally { IsBusy = false; }
        }

        // --- CHECKOUT LOGIC ---
        private bool CanCheckout() => HasItems && !IsBusy && CurrentCart != null && CurrentCart.Status == CartStatus.Active;

        [RelayCommand(CanExecute = nameof(CanCheckout))]
        private async Task CheckoutAsync()
        {
            if (!CanCheckout()) return; // Guard clause

            bool confirm = await Shell.Current.DisplayAlert("Confirm Order", $"Your total is {TotalPrice:C}. Proceed to payment simulation?", "Yes, Pay Now", "Cancel");
            if (!confirm) return;

            IsBusy = true;
            try
            {
                // Simulate payment process (replace with actual gateway later)
                await Task.Delay(1500); // Simulate network/processing time
                bool paymentSuccess = true; // Assume payment succeeds for simulation

                if (paymentSuccess)
                {
                    bool orderMarked = await _cartService.MarkCartAsOrderedAsync(CurrentCart.Id);

                    if (orderMarked)
                    {
                        await Shell.Current.DisplayAlert("Order Placed", "Payment successful! Your order has been placed.", "OK");
                        // Cart is now 'Ordered', reload to get a new 'Active' one (or show empty)
                        await LoadCartAsync();
                        // Optional: Navigate away (e.g., to menu or order history)
                        // await _navigationService.NavigateToAsync($"//{nameof(PlatsPage)}");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Checkout Error", "Payment succeeded, but failed to finalize the order. Please contact support.", "OK");
                        // State might be inconsistent, reload cart
                        await LoadCartAsync();
                    }
                }
                else
                {
                    // Payment simulation failed (in real scenario)
                    await Shell.Current.DisplayAlert("Payment Failed", "Your payment could not be processed. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during checkout: {ex.Message}");
                await Shell.Current.DisplayAlert("Checkout Error", $"An error occurred during checkout: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                // Recalculate canexecute state after operation
                CheckoutCommand.NotifyCanExecuteChanged();
            }
        }
        // Remember to implement IDisposable or similar to unsubscribe from PropertyChanged
    }
}