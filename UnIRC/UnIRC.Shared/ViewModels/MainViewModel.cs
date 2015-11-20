using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using GalaSoft.MvvmLight.Views;
using UnIRC.Shared.Messages;
using UnIRC.ViewModels;

namespace UnIRC.Shared.ViewModels
{
    public class MainViewModel : ViewModelBaseExtended
    {
        private INavigationService _navigationService;

        public ObservableCollection<ErrorMessage> Errors
        {
            get { return _errors; }
            set { Set(ref _errors, value); }
        }
        private ObservableCollection<ErrorMessage> _errors
            = new ObservableCollection<ErrorMessage>();

        public ObservableCollection<Message> Messages
        {
            get { return _messages; }
            set { Set(ref _messages, value); }
        }
        private ObservableCollection<Message> _messages
            = new ObservableCollection<Message>();
        
        public ICommand ClearErrorsCommand { get; set; }

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            // ReSharper disable once ExplicitCallerInfoArgument
            //Errors.CollectionChanged += (s, a) => RaisePropertyChanged(nameof(Errors));
            // ReSharper disable once ExplicitCallerInfoArgument
            //Messages.CollectionChanged += (s, a) => RaisePropertyChanged(nameof(Messages));

            Register<ErrorMessage>(m => Errors.Add(m));
            Register<Message>(true, m => Messages.Add(m));
        }
    }
}
