using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System;

namespace UnIRC.ViewModels
{
    public abstract class ViewModelLocatorBase
    {
        protected ViewModelLocatorBase()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
        }

        protected void Register<T>() where T : class
        {
            SimpleIoc.Default.Register<T>();
        }

        protected void Register<T>(Func<T> factory) where T : class
        {
            SimpleIoc.Default.Register(factory);
        }


        protected T GetInstance<T>() where T : class
        {
            return ServiceLocator.Current.GetInstance<T>();
        }

        public virtual void Cleanup()
        {
        }
    }
}
