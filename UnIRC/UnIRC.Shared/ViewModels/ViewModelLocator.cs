using System;
using GalaSoft.MvvmLight.Views;
using UnIRC.ViewModels;

namespace UnIRC.Shared.ViewModels
{
    public class ViewModelLocator : ViewModelLocatorBase
    {
        public ViewModelLocator()
        {
            Register<INavigationService>(() => new NavigationService());
            Register<NetworksViewModel>();
        }

        public INavigationService NavigationService => GetInstance<INavigationService>();
        public NetworksViewModel Networks => GetInstance<NetworksViewModel>();
    }

    public class NavService : INavigationService
    {
        public string CurrentPageKey { get; private set; }

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(string pageKey)
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(string pageKey, object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
