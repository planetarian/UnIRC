using UnIRC.ViewModels;

namespace UnIRC.Shared.ViewModels
{
    public class ViewModelLocator : ViewModelLocatorBase
    {
        public ViewModelLocator()
        {
            Register<NetworksViewModel>();
        }

        public NetworksViewModel Networks => GetInstance<NetworksViewModel>();
    }
}
