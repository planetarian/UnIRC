using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UnIRC.Shared.Helpers
{
    public class FunctionWrapper<T> : IDisposable where T : class
    {
        private readonly bool _setTrue;
        private T _source;
        private PropertyInfo _field;

        public FunctionWrapper(T source, Expression<Func<T, bool>> expr, bool setTrue = true)
        {
            _setTrue = setTrue;
            _source = source;
            var body = (MemberExpression)expr.Body;
            _field = (PropertyInfo)body.Member;
            _field.SetValue(_source, _setTrue);
        }

        public void Dispose()
        {
            _field.SetValue(_source, !_setTrue);
            _field = null;
            _source = null;
        }
    }
}
