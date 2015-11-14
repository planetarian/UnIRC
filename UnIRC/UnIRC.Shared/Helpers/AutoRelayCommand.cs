using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;

namespace UnIRC.Shared.Helpers
{
    public class AutoRelayCommand : RelayCommand, IDisposable
    {
        private readonly bool _async;
        private readonly List<string> _dependentPropertyNames;
        private INotifyPropertyChanged _target;

        public AutoRelayCommand(Action execute,
            INotifyPropertyChanged target, bool async = false)
            : base(execute)
        {
            _async = async;
            _dependentPropertyNames = new List<string>();
            _target = target;

            _target.PropertyChanged += TargetPropertyChanged;
        }

        public AutoRelayCommand(Action execute, Expression<Func<bool>> canExecute,
            INotifyPropertyChanged target, bool async = false)
            : base(execute, canExecute.Compile())
        {
            _async = async;
            _dependentPropertyNames = new List<string> { canExecute.GetMemberName() };
            _target = target;

            _target.PropertyChanged += TargetPropertyChanged;
        }

        public AutoRelayCommand(Action execute, Func<bool> canExecute,
            INotifyPropertyChanged target, bool async,
            string firstDependentPropertyName, params string[] dependentPropertyNames)
            : base(execute, canExecute)
        {
            _async = async;

            _dependentPropertyNames = new List<string> { firstDependentPropertyName };
            _dependentPropertyNames.AddRange(dependentPropertyNames);

            _target = target;
            _target.PropertyChanged += TargetPropertyChanged;
        }

        public AutoRelayCommand(Action execute, Func<bool> canExecute,
            INotifyPropertyChanged target, bool async,
            Expression<Func<object>> firstDependentPropertyExpression, params Expression<Func<object>>[] dependentPropertyExpressions)
            : base(execute, canExecute)
        {
            _async = async;

            _dependentPropertyNames = new List<string>();

            if (firstDependentPropertyExpression != null)
                _dependentPropertyNames.Add(firstDependentPropertyExpression.GetMemberName());

            _dependentPropertyNames.AddRange(
                dependentPropertyExpressions.Select(x => x.GetMemberName()));

            _target = target;
            _target.PropertyChanged += TargetPropertyChanged;
        }


        public static AutoRelayCommand GetAutomatic(Action execute, Expression<Func<bool>> canExecute,
            INotifyPropertyChanged target, bool async = false)
        {
            return new AutoRelayCommand(execute, canExecute.Compile(), target, async,
                ((MemberExpression)canExecute.Body).Member.Name);
        }

        private void TargetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_dependentPropertyNames.Contains(e.PropertyName))
                DispatcherHelper.CheckBeginInvokeOnUI(RaiseCanExecuteChanged);
        }

        public override void Execute(object parameter)
        {
            if (_async)
                Task.Run(() => base.Execute(parameter));
            else
                base.Execute(parameter);
        }



        public void Dispose()
        {
            _target.PropertyChanged -= TargetPropertyChanged;
            _target = null;
        }
    }
}
