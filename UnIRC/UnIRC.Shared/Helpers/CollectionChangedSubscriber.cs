using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UnIRC.Shared.Helpers
{
    public static class CollectionChangedExtensions
    {
        public static CollectionChangedSubscriber<TSource, TCollection>
            OnCollectionChanged<TSource, TCollection>(this TSource source,
                Expression<Func<TSource, TCollection>> property)
            where TSource : class, INotifyPropertyChanged
            where TCollection : class, INotifyCollectionChanged
        {
            return new CollectionChangedSubscriber<TSource, TCollection>(source, property);
        }
    }

    public class CollectionChangedSubscriber<TSource, TCollection> : IDisposable
        where TSource : class, INotifyPropertyChanged
        where TCollection : class, INotifyCollectionChanged
    {
        private readonly Expression<Func<TSource, TCollection>> _propertyExpression;
        private readonly string _propertyName;
        private readonly TSource _source;
        private readonly List<Action<TSource>> _onChangeWithParameter = new List<Action<TSource>>();
        private readonly List<Action> _onChange = new List<Action>();
        private TCollection _currentValue;

        public CollectionChangedSubscriber(TSource source, Expression<Func<TSource, TCollection>> property)
        {
            _propertyExpression = property;
            _source = source;
            _propertyName = GetPropertyName(_propertyExpression);
            source.PropertyChanged += SourcePropertyChanged;
        }

        private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != _propertyName)
                return;
            if (_currentValue != null)
                _currentValue.CollectionChanged -= HandleCollectionChanged;

            PropertyInfo property = sender.GetType().GetProperty(_propertyName);
            var newValue = property?.GetValue(sender, null) as TCollection;
            if (newValue == null)
                return;

            _currentValue = newValue;
            _currentValue.CollectionChanged += HandleCollectionChanged;
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _onChangeWithParameter.ForEach(o => o(sender as TSource));
            _onChange.ForEach(o => o());
        }

        private static string GetPropertyName(Expression<Func<TSource, TCollection>> propertyExpression)
        {
            PropertyInfo propertyInfo = null;
            var mExpr = propertyExpression.Body as MemberExpression;
            if (mExpr != null)
            {
                propertyInfo = mExpr.Member as PropertyInfo;
            }
            else
            {
                var uExpr = propertyExpression.Body as UnaryExpression;
                if (uExpr != null)
                {
                    mExpr = uExpr.Operand as MemberExpression;
                    if (mExpr != null)
                        propertyInfo = mExpr.Member as PropertyInfo;
                }
            }

            if (propertyInfo == null)
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");

            return propertyInfo.Name;
        }

        public CollectionChangedSubscriber<TSource, TCollection> Do(Action<TSource> onChanged)
        {
            _onChangeWithParameter.Add(onChanged);
            return this;
        }

        public CollectionChangedSubscriber<TSource, TCollection> Do(Action onChanged)
        {
            _onChange.Add(onChanged);
            return this;
        }

        public CollectionChangedSubscriber<TSource, TCollection> DoOnce(Action<TSource> onChanged)
        {
            Action<TSource> remove = x => RemoveAction(onChanged);
            _onChangeWithParameter.Add((Action<TSource>)Delegate.Combine(onChanged, remove));
            return this;
        }

        public CollectionChangedSubscriber<TSource, TCollection> DoOnce(Action onChanged)
        {
            Action remove = () => RemoveAction(onChanged);
            _onChange.Add((Action)Delegate.Combine(onChanged, remove));
            return this;
        }

        private void RemoveAction(Action onChanged)
        {
            _onChange.Remove(onChanged);
            DisposeCheck();
        }

        private void RemoveAction(Action<TSource> onChanged)
        {
            _onChangeWithParameter.Remove(onChanged);
            DisposeCheck();
        }

        private void DisposeCheck()
        {
            if (_onChangeWithParameter.Count + _onChange.Count == 0)
                Dispose();
        }


        public void Dispose()
        {
            if (_currentValue != null)
                _currentValue.CollectionChanged -= HandleCollectionChanged;
            _source.PropertyChanged -= SourcePropertyChanged;
        }
    }
}
