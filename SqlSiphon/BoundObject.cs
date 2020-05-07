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

        protected Dictionary<string, object> values;

        public BoundObject()
        {
            values = new Dictionary<string, object>();
        }

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
            if (values.ContainsKey(propertyName) && values[propertyName] != null)
            {
                try
                {
                    return (T)values[propertyName];
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
            if (propertyName == null)
            {
                propertyName = GetMethodName();
            }

            if (!values.ContainsKey(propertyName))
            {
                values.Add(propertyName, value);
                OnPropertyChanged(propertyName);
            }
            else if (values[propertyName] == null)
            {
                if ((value as object) != null)
                {
                    values[propertyName] = value;
                    OnPropertyChanged(propertyName);
                }
            }
            else if (!values[propertyName].Equals(value))
            {
                values[propertyName] = value;
                OnPropertyChanged(propertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                if (!propertyChangedCache.ContainsKey(property))
                {
                    propertyChangedCache.Add(property, new PropertyChangedEventArgs(property));
                }
                PropertyChanged(this, propertyChangedCache[property]);
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;
        private void OnPropertyChanging(string property)
        {
            if (PropertyChanging != null)
            {
                if (!propertyChangingCache.ContainsKey(property))
                {
                    propertyChangingCache.Add(property, new PropertyChangingEventArgs(property));
                }
                PropertyChanging(this, propertyChangingCache[property]);
            }
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
                || values.Count != b.values.Count)
            {
                return false;
            }

            foreach (var key in values.Keys)
            {
                if (!b.values.ContainsKey(key)
                    || (values[key] != null && !values[key].Equals(b.values[key]))
                    || (b.values[key] != null && !b.values[key].Equals(values[key])))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
