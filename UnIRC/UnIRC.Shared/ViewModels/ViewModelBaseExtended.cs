using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
#if WINDOWS_UWP
using Windows.Storage;
#endif
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.Messages;

// ReSharper disable ExplicitCallerInfoArgument

namespace UnIRC.ViewModels
{
    public class ViewModelBaseExtended : ViewModelBase
    {
        protected readonly object Lock = new object();
#if WINDOWS_UWP
        protected static readonly ApplicationDataContainer RoamingSettings
            = ApplicationData.Current.RoamingSettings;

        protected static readonly ApplicationDataContainer LocalSettings
            = ApplicationData.Current.LocalSettings;
#endif

        public string Title { get; protected set; }



        public ViewModelBaseExtended()
        {
            Title = GetType().Name;
        }

        public ViewModelBaseExtended(string title)
        {
            Title = title;
        }

        protected static void SaveRoamingSetting(string key, object value)
        {

#if WINDOWS_UWP
            RoamingSettings.Values[key] = value;
#else
            throw new NotImplementedException();
#endif
        }

        protected static object GetRoamingSetting(string key)
        {
#if WINDOWS_UWP
            return RoamingSettings.Values[key];
#else
            throw new NotImplementedException();
#endif
        }

        protected static void SaveLocalSetting(string key, object value)
        {
#if WINDOWS_UWP
            LocalSettings.Values[key] = value;
#else
            throw new NotImplementedException();
#endif
        }

        protected static object GetLocalSetting(string key)
        {
#if WINDOWS_UWP
            return LocalSettings.Values[key];
#else
            throw new NotImplementedException();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void Send<T>(T message)
        {
            Messenger.Default.Send(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Register<TMessage>(Action<TMessage> action)
        {
            Messenger.Default.Register(this, action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Register<TMessage>(bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            Messenger.Default.Register(this, receiveDerivedMessagesToo, action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void NotifyReady()
        {
            Send(new ReadyMessage(GetType()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError(string message, string details = null,
            bool isError = true, bool display = true,
            [CallerFilePath] string filePath = null,
            [CallerMemberName] string memberName = null,
            [CallerLineNumber] int lineNumber = -1)
        {
            Util.RunOnUI(() =>
            Send(new ErrorMessage(message, details, isError, display,
                filePath, memberName, lineNumber)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError(string message, Exception ex,
            bool isError = true, bool display = true,
            [CallerFilePath] string filePath = null,
            [CallerMemberName] string memberName = null,
            [CallerLineNumber] int lineNumber = -1)
        {
            Util.RunOnUI(() =>
                Send(new ErrorMessage(message, ex, isError, display,
                    filePath, memberName, lineNumber)));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RaisePropertyChangedOnUI<T>(Expression<Func<T>> expr)
        {
            Util.RunOnUI(() => RaisePropertyChanged(expr));
        }


#region GetCommand


        // execute

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RelayCommand GetCommand(Action execute, bool async = false)
        {
            return new AutoRelayCommand(execute, this, async);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RelayCommand GetAsyncCommand(Action execute)
        {
            return new AutoRelayCommand(execute, this, true);
        }


        // execute + canExecute

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AutoRelayCommand GetCommand(
            Action execute, Expression<Func<bool>> canExecute, bool async = false)
        {
            return new AutoRelayCommand(execute, canExecute, this, async);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AutoRelayCommand GetAsyncCommand(
            Action execute, Expression<Func<bool>> canExecute)
        {
            return new AutoRelayCommand(execute, canExecute, this, true);
        }


        // execute + canExecute + property names

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AutoRelayCommand GetCommand(
            Action execute, Func<bool> canExecute,
            string firstDependentPropertyName, params string[] dependentPropertyNames)
        {
            return new AutoRelayCommand(execute, canExecute, this, false,
                firstDependentPropertyName, dependentPropertyNames);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AutoRelayCommand GetAsyncCommand(
            Action execute, Func<bool> canExecute, bool async,
            string firstDependentPropertyName, params string[] dependentPropertyNames)
        {
            return new AutoRelayCommand(execute, canExecute, this, async,
                firstDependentPropertyName, dependentPropertyNames);
        }


        // execute + canExecute + property names

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AutoRelayCommand GetCommand(
            Action execute, Func<bool> canExecute,
            Expression<Func<object>> firstDependentPropertyExpression,
            params Expression<Func<object>>[] dependentPropertyExpressions)
        {
            return new AutoRelayCommand(execute, canExecute, this, false,
                firstDependentPropertyExpression, dependentPropertyExpressions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AutoRelayCommand GetAsyncCommand(
            Action execute, Func<bool> canExecute, bool async,
            Expression<Func<object>> firstDependentPropertyExpression,
            params Expression<Func<object>>[] dependentPropertyExpressions)
        {
            return new AutoRelayCommand(execute, canExecute, this, async,
                firstDependentPropertyExpression, dependentPropertyExpressions);
        }


#endregion GetCommand
    }
}
