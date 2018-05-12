using System;
using System.Collections.Generic;

namespace Wibblr.Collections
{
    /// <summary>
    /// Like a normal enumerator, but with the ability to look ahead to the 
    /// next item. Works by encapsulating a normal enumerator, and is always
    /// one position ahead of where the client has actually advanced to. 
    /// 
    /// The 'current' item (as seen by the caller) is cached from when it
    /// was read previously; the 'next' item (as seen by the caller) is 
    /// actually the encapsulated enumerator's current item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LookAheadEnumerator<T>
    {
        private IEnumerator<T> e;
        private T previous;

        public bool HasNext { get; private set; }
        public bool HasCurrent { get; private set; }

        public T Current { get => HasCurrent ? previous : throw new InvalidOperationException(); }

        public T Next { get => e.Current; }

        public LookAheadEnumerator(IEnumerable<T> items)
        {
            e = items.GetEnumerator();
            HasNext = e.MoveNext();
        }

        public bool MoveNext()
        {
            HasCurrent = HasNext;

            if (HasCurrent)
                previous = e.Current;

            if (HasNext)
                HasNext = e.MoveNext();

            return HasCurrent;
        }
    }
}
