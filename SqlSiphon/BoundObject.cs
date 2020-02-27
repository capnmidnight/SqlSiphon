/*
https://www.github.com/capnmidnight/SqlSiphon
Copyright (c) 2009 - 2014 Sean T. McBeth
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this 
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or 
  other materials provided with the distribution.

* Neither the name of McBeth Software Systems nor the names of its contributors
  may be used to endorse or promote products derived from this software without 
  specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
OF THE POSSIBILITY OF SUCH DAMAGE.
*/

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
        #region statics
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
        #endregion

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
