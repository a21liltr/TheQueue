using System.Collections;

namespace TheQueue.Server.Core.Models
{
    public class ConcurrentList<T> : IList<T>
    {
        private object _lock = new();
        private List<T> _list = new();

        public T this[int index] {
            get { lock (_lock) return _list[index]; }
            set { lock (_lock) _list[index] = value; }
        }

        public int Count => _list.Count;

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T item)
        {
            lock (_lock)
                _list.Add(item);
        }

        public void Clear()
        {
            lock (_lock)
                _list.Clear();
        }

        public bool Contains(T item)
        {
            lock ( _lock)
                return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock(_lock)
                return _list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            lock( _lock)
                return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            lock (_lock)
                _list.Insert(index, item);
        }

        public bool Remove(T item)
        {
            lock (_lock)
                return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            lock (_lock)
                _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
