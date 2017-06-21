using System;
using System.Collections.Generic;

namespace CoreDht.Utils
{
    public class ObjectCache<TKey, TValue>
    {
        protected Func<TKey, TValue> Factory { get; set; }

        protected Dictionary<TKey, TValue> Cache { get; }

        public ObjectCache(Func<TKey, TValue> factory = null)
        {
            Factory = factory;
            Cache = new Dictionary<TKey, TValue>();
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!Cache.TryGetValue(key, out value))
                {
                    if (Factory == null)
                    {
                        throw new FactoryNotAssignedException();
                    }

                    value = Factory(key);
                    Cache[key] = value;
                }
                return value;
            }
        }

        public class FactoryNotAssignedException : Exception
        {
            public FactoryNotAssignedException()
                : base("Object Factory must be defined before retreiving from ObjectCache")
            { }
        }

        public virtual void Remove(TKey key)
        {
            Cache.Remove(key);
        }
    }
}