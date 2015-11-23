using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UnIRC.Shared.Helpers
{
    public class TypeSwitch
    {
        private readonly Dictionary<Type, Action<object>> _matches
            = new Dictionary<Type, Action<object>>();
        private readonly Dictionary<Type, Func<object, Task>> _matchesAsync
            = new Dictionary<Type, Func<object, Task>>();

        public TypeSwitch Case<T>(Action<T> action)
        {
            _matches.Add(typeof(T), x => action((T)x));
            return this;
        }
        public TypeSwitch Case<T>(Func<T, Task> func)
        {
            _matches.Add(typeof(T), x => func((T)x));
            return this;
        }

        public void Switch(object x)
        {
            Type type = x.GetType();
            if (_matches.ContainsKey(type))
                _matches[type](x);
            else if (_matchesAsync.ContainsKey(type))
            {
                Task task = _matchesAsync[type](x);
                task.RunSynchronously();
            }
            else throw new KeyNotFoundException();
        }
        public async Task SwitchAsync(object x)
        {
            Type type = x.GetType();
            if (_matchesAsync.ContainsKey(type))
                await _matchesAsync[type](x);
            else if (_matches.ContainsKey(type))
                await Task.Run(() => _matches[type](x));
            else throw new KeyNotFoundException();
        }
    }

    public class TypeSwitchAsync
    {
        private readonly Dictionary<Type, Func<object, Task>> _matches
            = new Dictionary<Type, Func<object, Task>>();

        public TypeSwitchAsync Case<T>(Func<object, Task> func)
        {
            _matches.Add(typeof(T), x => func((T)x));
            return this;
        }

        public async Task SwitchAsync(object x)
        {
            await _matches[x.GetType()](x);
        }
    }
}
