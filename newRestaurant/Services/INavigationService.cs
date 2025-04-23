using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newRestaurant.Services
{
    public interface INavigationService
    {
        // Navigate to a specific route (page)
        Task NavigateToAsync(string route);

        // Navigate with parameters
        Task NavigateToAsync(string route, IDictionary<string, object> parameters);

        // Go back
        Task GoBackAsync();
    }
}
