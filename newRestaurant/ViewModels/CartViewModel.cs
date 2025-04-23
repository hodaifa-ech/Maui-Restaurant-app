// ViewModels/CartViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    public partial class CartViewModel : BaseViewModel
    {

        [ObservableProperty]
        private ObservableCollection<CartPlat> _cartItems = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasItems))]
        private Cart _currentCart;

        [ObservableProperty]
        private decimal _totalPrice;

        public bool HasItems => CartItems?.Count > 0;


        private readonly ICartService _cartService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService; // ADD

        public CartViewModel(ICartService cartService, INavigationService navigationService, IAuthService authService) // ADD INJECTION
        {
            _cartService = cartService;
            _navigationService = navigationService;
            _authService = authService; // ADD Assignment
            Title = "My Cart";

            // Listen for login/logout changes to reload the cart
            _authService.PropertyChanged += AuthService_PropertyChanged;
        }
        private async void AuthService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAuthService.CurrentUser))
            {
                // Use Task.Run or MainThread.InvokeOnMainThreadAsync if needed, but LoadCartAsync handles IsBusy
                await LoadCartCommand.ExecuteAsync(null);
            }
        }

        [RelayCommand]
        public async Task LoadCartAsync()
        {
            if (IsBusy) return;

            // Get current user ID
            var userId = _authService.CurrentUser?.Id;
            if (userId == null)
            {
                // Not logged in, clear the cart display
                CartItems.Clear();
                CurrentCart = null;
                CalculateTotal();
                OnPropertyChanged(nameof(HasItems));
                return; // Exit if not logged in
            }
            IsBusy = true;
            try
            {
                CurrentCart = await _cartService.GetActiveCartAsync((int)userId);
                CartItems.Clear();
                if (CurrentCart?.CartPlats != null)
                {
                    foreach (var item in CurrentCart.CartPlats)
                    {
                        CartItems.Add(item);
                    }
                }
                CalculateTotal();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading cart: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load cart.", "OK");
            }
            finally
            {
                IsBusy = false;
                // Ensure HasItems updates after loading
                OnPropertyChanged(nameof(HasItems));
            }
        }

        private void CalculateTotal()
        {
            TotalPrice = CartItems?.Sum(item => (item.Plat?.Price ?? 0) * item.Quantity) ?? 0;
        }

        [RelayCommand]
        private async Task UpdateQuantityAsync(CartPlat item)
        {
            if (item == null || IsBusy || CurrentCart == null) return;

            // Basic check: quantity must be at least 1 if triggered from UI needing update
            if (item.Quantity < 1) item.Quantity = 1; // Reset if somehow below 1

            IsBusy = true;
            try
            {
                bool success = await _cartService.UpdateCartItemQuantityAsync(CurrentCart.Id, item.PlatId, item.Quantity);
                if (!success)
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update quantity.", "OK");
                    // Optionally reload cart fully on failure
                    await LoadCartAsync();
                }
                else
                {
                    // Recalculate total after successful update
                    CalculateTotal();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating quantity: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "An error occurred while updating quantity.", "OK");
                await LoadCartAsync(); // Reload on error
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Command specifically for increasing quantity by 1
        [RelayCommand]
        private async Task IncreaseQuantityAsync(CartPlat item)
        {
            if (item == null || IsBusy || CurrentCart == null) return;
            item.Quantity++;
            await UpdateQuantityAsync(item); // Call the main update logic
        }

        // Command specifically for decreasing quantity by 1
        [RelayCommand]
        private async Task DecreaseQuantityAsync(CartPlat item)
        {
            if (item == null || IsBusy || CurrentCart == null) return;

            if (item.Quantity > 1)
            {
                item.Quantity--;
                await UpdateQuantityAsync(item); // Call the main update logic
            }
            else
            {
                // If quantity is 1, decreasing means removing
                await RemoveItemAsync(item);
            }
        }


        [RelayCommand]
        private async Task RemoveItemAsync(CartPlat item)
        {
            if (item == null || IsBusy || CurrentCart == null) return;

            // Optional: Confirm removal
            // bool confirm = await Shell.Current.DisplayAlert("Confirm", $"Remove {item.Plat?.Name} from cart?", "Yes", "No");
            // if (!confirm) return;

            IsBusy = true;
            try
            {
                bool success = await _cartService.RemoveItemFromCartAsync(CurrentCart.Id, item.PlatId);
                if (success)
                {
                    // Remove locally and recalculate
                    CartItems.Remove(item);
                    CalculateTotal();
                    OnPropertyChanged(nameof(HasItems)); // Update HasItems state
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to remove item.", "OK");
                    await LoadCartAsync(); // Reload on failure
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing item: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "An error occurred while removing item.", "OK");
                await LoadCartAsync(); // Reload on error
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CheckoutAsync()
        {
            if (IsBusy || CurrentCart == null || !HasItems) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm Order", $"Proceed to payment simulation for a total of {TotalPrice:C}?", "Yes", "No");
            if (!confirm) return;

            IsBusy = true;
            try
            {
                // Simulate payment process... (could involve navigation to a payment page)
                await Task.Delay(1500); // Simulate network delay/payment processing

                bool success = await _cartService.MarkCartAsOrderedAsync(CurrentCart.Id);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Order Placed", "Your order has been successfully placed!", "OK");
                    // Clear local cart view / reload (should be empty or new)
                    await LoadCartAsync();
                    // Optional: Navigate to order history or back to menu
                    // await _navigationService.GoBackAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Checkout Error", "Could not place the order. The cart might be empty or an error occurred.", "OK");
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
            }
        }
    }
}