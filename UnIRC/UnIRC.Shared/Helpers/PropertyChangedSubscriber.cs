using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;

namespace UnIRC.Shared.Helpers
{
    public static class PropertyChangedExtensions
    {
        /// <summary>
        /// returns a PropertyChangedSubscriber so taht you can hook to PropertyChanged
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source">The source that is the INotifyPropertyChanged</param>
        /// <param name="properties">THe properties to attach to</param>
        /// <returns>Returns the subscriber</returns>
        public static PropertyChangedSubscriber<TSource>
            OnChanged<TSource>(this TSource source, params Expression<Func<TSource, object>>[] properties)
            where TSource : class, INotifyPropertyChanged
        {
            return new PropertyChangedSubscriber<TSource>(source, properties);
        }
    }

    public class PropertyChangedSubscriber<TSource>
        : IDisposable where TSource : class, INotifyPropertyChanged
    {
        private readonly Expression<Func<TSource, object>>[] _propertyExpressions;
        private readonly TSource _source;
        private readonly List<Action<TSource>> _onChangeWithParameter = new List<Action<TSource>>();
        private readonly List<Action<TSource>> _onChangeAsyncWithParameter = new List<Action<TSource>>();
        private readonly List<Action<TSource>> _onChangeOnUIWithParameter = new List<Action<TSource>>();
        private readonly List<Action> _onChange = new List<Action>();
        private readonly List<Action> _onChangeAsync = new List<Action>();
        private readonly List<Action> _onChangeOnUI = new List<Action>();

        public PropertyChangedSubscriber(TSource source, Expression<Func<TSource, object>>[] properties)
        {
            _propertyExpressions = properties;
            _source = source;
            source.PropertyChanged += SourcePropertyChanged;
        }

        private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsPropertyValid(e.PropertyName))
            {
                _onChangeWithParameter.ForEach(o => o(sender as TSource));
                _onChange.ForEach(o => o());
                _onChangeAsyncWithParameter.ForEach(o => Task.Run(() => o(sender as TSource)));
                _onChangeAsync.ForEach(o => Task.Run(o));
                _onChangeOnUIWithParameter.ForEach(
                    o => DispatcherHelper.CheckBeginInvokeOnUI(() => o(sender as TSource)));
                _onChangeOnUI.ForEach(DispatcherHelper.CheckBeginInvokeOnUI);
            }
        }

        /// <summary>
        /// Executes the action and returns an IDisposable so that you can unregister 
        /// </summary>
        /// <param name="onChanged">The action to execute</param>
        /// <returns>The IDisposable so that you can unregister</returns>
        public PropertyChangedSubscriber<TSource> Do(Action<TSource> onChanged)
        {
            _onChangeWithParameter.Add(onChanged);
            return this;
        }
        public PropertyChangedSubscriber<TSource> DoAsync(Action<TSource> onChanged)
        {
            _onChangeAsyncWithParameter.Add(onChanged);
            return this;
        }
        public PropertyChangedSubscriber<TSource> DoOnUI(Action<TSource> onChanged)
        {
            _onChangeOnUIWithParameter.Add(onChanged);
            return this;
        }

        public PropertyChangedSubscriber<TSource> Do(Action onChanged)
        {
            _onChange.Add(onChanged);
            return this;
        }

        public PropertyChangedSubscriber<TSource> DoAsync(Action onChanged)
        {
            _onChangeAsync.Add(onChanged);
            return this;
        }
        public PropertyChangedSubscriber<TSource> DoOnUI(Action onChanged)
        {
            _onChangeOnUI.Add(onChanged);
            return this;
        }

        /// <summary>
        /// Executes the action only once and automatically unregisters
        /// </summary>
        /// <param name="onChanged">The action to be executed</param>
        public void DoOnce(Action<TSource> onChanged)
        {
            //Action<TSource> dispose = x => Dispose();
            Action<TSource> remove = x => RemoveAction(onChanged);
            _onChangeWithParameter.Add((Action<TSource>)Delegate.Combine(onChanged, remove));
        }

        /// <summary>co
        /// Executes the action only once and automatically unregisters
        /// </summary>
        /// <param name="onChanged">The action to be executed</param>
        public void DoOnce(Action onChanged)
        {
            //Action<TSource> dispose = x => Dispose();
            Action<TSource> remove = x => RemoveAction(onChanged);
            _onChange.Add((Action)Delegate.Combine(onChanged, remove));
        }

        private void RemoveAction(Action<TSource> onChanged)
        {
            _onChangeWithParameter.Remove(onChanged);
            _onChangeAsyncWithParameter.Remove(onChanged);
            _onChangeOnUIWithParameter.Remove(onChanged);
            DisposeCheck();
        }

        private void RemoveAction(Action onChanged)
        {
            _onChange.Remove(onChanged);
            _onChangeAsync.Remove(onChanged);
            _onChangeOnUI.Remove(onChanged);
            DisposeCheck();
        }

        private void DisposeCheck()
        {
            if (_onChangeWithParameter.Count + _onChangeAsyncWithParameter.Count + _onChangeOnUIWithParameter.Count +
                _onChange.Count + _onChangeAsync.Count + _onChangeOnUI.Count == 0)
                Dispose();
        }

        private bool IsPropertyValid(string propertyName)
        {
            foreach (Expression<Func<TSource, object>> expr in _propertyExpressions)
            {
                PropertyInfo propertyInfo = null;
                var mExpr = expr.Body as MemberExpression;
                if (mExpr != null)
                {
                    propertyInfo = mExpr.Member as PropertyInfo;
                }
                else
                {
                    var uExpr = expr.Body as UnaryExpression;
                    if (uExpr != null)
                    {
                        mExpr = uExpr.Operand as MemberExpression;
                        if (mExpr != null)
                            propertyInfo = mExpr.Member as PropertyInfo;
                    }
                }

                if (propertyInfo == null)
                    throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
                if (propertyInfo.Name == propertyName)
                    return true;
            }

            return false;
        }

        #region Implementation of IDisposable

        /// <summary>
        ///   Unregisters the property
        /// </summary>
        public void Dispose()
        {
            _source.PropertyChanged -= SourcePropertyChanged;
        }

        #endregion
    }
}
