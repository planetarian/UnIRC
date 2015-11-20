using System;
using GalaSoft.MvvmLight.Views;
using UnIRC.ViewModels;

namespace UnIRC.Shared.ViewModels
{
    public class ViewModelLocator : ViewModelLocatorBase
    {
        public ViewModelLocator()
        {
#if WINDOWS_UWP
            Register<INavigationService>(() => new NavigationService());
#endif
            Register<MainViewModel>();
            Register<NetworksViewModel>();
        }

        public INavigationService NavigationService => GetInstance<INavigationService>();
        public MainViewModel Main => GetInstance<MainViewModel>();
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
