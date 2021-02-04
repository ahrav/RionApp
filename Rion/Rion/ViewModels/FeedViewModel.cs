using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Rion.ViewModels
{
    public class FeedViewModel : BaseViewModel
    {
        private int _speed = 0;
        private double _voltage = 00.0;
        private double _amps = 0.0;
        private double _temp = 0.0;

        public int Speed
        {
            get => _speed;
            set
            {
                if ((_speed == value) || (Math.Abs(_speed - value) > 15)) return;
                _speed = value;

                OnPropertyChanged();
            }
        }

        public double Voltage
        {
            get => _voltage;
            set
            {
                if (Math.Abs(_voltage - value) < 0.01) return;
                _voltage = value;
                OnPropertyChanged();
            }
        }

        public double Amps
        {
            get => _amps;
            set
            {
                if (Math.Abs(_amps - value) < 0.1) return;
                _amps = value;
                OnPropertyChanged();
            }
        }

        public double Temp
        {
            get => _temp;
            set
            {
                if (Math.Abs(_temp - value) < 0.1) return;
                _temp = value;
                OnPropertyChanged();
            }
        }
    }
}