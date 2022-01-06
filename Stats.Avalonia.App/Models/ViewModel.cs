using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using JetBrains.Annotations;
using System.Reactive;
// using ACalc.Models;
using ReactiveUI;

namespace Stats.Avalonia.App.Models
{
    public class ViewModel : INotifyPropertyChanged
    {

        private string _count = ""; 
        private string _countThreads = "";
        private string _expValue = "";
        private string _variance = "";
        private string _metrics = "";
        private string _elapsedTime = "";
        private double _progress = 0;

        private bool _isChecked = false;



        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value; 
                    OnPropertyChanged();
                }
            }
        }
        public double Progress
        {
            get => _progress;
            set
            {
                if (Math.Abs(value - _progress) > 0)
                {
                    _progress = value; 
                    OnPropertyChanged();
                }
            }
        }
        public string Count
        {
            get => _count;
            set
            {
                if (value != _count)
                {
                    _count = value; 
                    OnPropertyChanged(nameof(Count));
                }
            }
        }

        public string CountThreads
        {
            get => _countThreads;
            set
            {
                if (value != _countThreads)
                {
                    _countThreads = value; 
                    OnPropertyChanged(nameof(CountThreads));
                }
            }
        }
        
        
        public string ExpValue
        {
            get => _expValue;
            set
            {
                if (value != _expValue)
                {
                    _expValue = value; 
                    OnPropertyChanged();
                }
            }
        }
        
        
        public string Variance
        {
            get => _variance;
            set
            {
                if (value != _variance)
                {
                    _variance = value; 
                    OnPropertyChanged();
                }
            }
        }
        
        
        public string Metrics
        {
            get => _metrics;
            set
            {
                if (value != _metrics)
                {
                    _metrics = value;
                    OnPropertyChanged();
                }
            }
        }


        public string ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                if (value != _elapsedTime)
                {
                    _elapsedTime = value; 
                    OnPropertyChanged();
                }
            }
        }
        
        
        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}