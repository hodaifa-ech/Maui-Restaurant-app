using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))] // Notify that IsNotBusy changes when IsBusy changes
        private bool _isBusy;

        [ObservableProperty]
        private string _title = string.Empty;

        public bool IsNotBusy => !_isBusy;
    }
}
