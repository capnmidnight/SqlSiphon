using System.Collections.Generic;
using System.ComponentModel;

namespace SqlSiphon
{
    /// <summary>
    /// An abstract class that is fairly easy to subclass and use to create objects 
    /// that notify listeners of changes to their member values.
    /// </summary>
    public abstract class BoundObject : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private static readonly Dictionary<string, PropertyChangedEventArgs> propertyChangedCache = new Dictionary<string, PropertyChangedEventArgs>();
        private static readonly Dictionary<string, PropertyChangingEventArgs> propertyChangingCache = new Dictionary<string, PropertyChangingEventArgs>();

        private static string GetMethodName()
        {
            // skip GetMethodName and whatever Scan* method that called it
            var frame = new System.Diagnostics.StackTrace(2).GetFrame(0);
            var name = frame.GetMethod().Name;
            name = name.Substring(4); // trim off "_set";
            return name;
        }

        protected Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        protected T Get<T>()
        {
            var propertyName = GetMethodName();
            return Get<T>(propertyName);
        }

        protected T Get<T>(string propertyName)
        {
            if (propertyName == null)
            {
                propertyName = GetMethodName();
            }

            OnPropertyAccessed(propertyName);
            if (Values.ContainsKey(propertyName) && Values[propertyName] != null)
            {
                try
                {
                    return (T)Values[propertyName];
                }
                catch { }
            }

            return default;
        }

        protected void Set<T>(T value)
        {
            var propertyName = GetMethodName();
            Set(value, propertyName);
        }

        protected void Set<T>(T value, string propertyName)
        {
            propertyName ??= GetMethodName();

            OnPropertyChanging(propertyName);

            if (!Values.ContainsKey(propertyName))
            {
                Values.Add(propertyName, value);
                OnPropertyChanged(propertyName);
            }
            else if (Values[propertyName] == null)
            {
                if ((value as object) != null)
                {
                    Values[propertyName] = value;
                    OnPropertyChanged(propertyName);
                }
            }
            else if (!Values[propertyName].Equals(value))
            {
                Values[propertyName] = value;
                OnPropertyChanged(propertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(
                this,
                propertyChangedCache.Cache(
                    property,
                    v => new PropertyChangedEventArgs(v)));
        }

        public event PropertyChangingEventHandler PropertyChanging;
        private void OnPropertyChanging(string property)
        {
            PropertyChanging?.Invoke(
                this,
                propertyChangingCache.Cache(
                    property,
                    v => new PropertyChangingEventArgs(v)));
        }

        public delegate void PropertyAccessedEventHandler(object sender, string propertyName);
        public event PropertyAccessedEventHandler PropertyAccessed;
        private void OnPropertyAccessed(string propertyName)
        {
            PropertyAccessed?.Invoke(this, propertyName);
        }

        private bool CompareFields(object obj)
        {
            if (!(obj is BoundObject b)
                || Values.Count != b.Values.Count)
            {
                return false;
            }

            foreach (var key in Values.Keys)
            {
                if (!b.Values.ContainsKey(key)
                    || (Values[key] != null && !Values[key].Equals(b.Values[key]))
                    || (b.Values[key] != null && !b.Values[key].Equals(Values[key])))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
