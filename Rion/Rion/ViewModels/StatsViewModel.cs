using System;
using Xamarin.Forms;

namespace Rion.ViewModels
{
    public class StatsViewModel : BaseViewModel
    {
        private double _lifetimeSpeed;
        private int _lifetimeDistance;
        private double _sessionSpeed;
        private int _sessionDistance;
        
        public double LifetimeSpeed
        {
            get => (double) Application.Current.Properties["lifetimeSpeed"];
            set
            {
                _lifetimeSpeed = value;
                OnPropertyChanged();
            }
        }

        public int LifetimeDistance
        {
            get => _lifetimeDistance;
            set
            {
                _lifetimeDistance = value;
                OnPropertyChanged();
            }
        }

        public int SessionDistance
        {
            get => _sessionDistance;
            set
            {
                _sessionDistance = value;
                OnPropertyChanged();
            }
        }

        public double SessionSpeed
        {
            get => _sessionSpeed;
            set
            {
                _sessionSpeed = value;
                OnPropertyChanged();
            }
        }
    }
}