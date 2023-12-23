using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DeadSplitEditor.Extensions
{
    public static class IListExtensions
    {
        public static void Sort<T>(this IList<T> list)
        {
            if (list is List<T> lst)
            {
                lst.Sort();
            }
            else
            {
                var copy = new List<T>(list);
                copy.Sort();
                Copy(copy, 0, list, 0, list.Count);
            }
        }

        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list is List<T> lst)
            {
                lst.Sort(comparison);
            }
            else
            {
                var copy = new List<T>(list);
                copy.Sort(comparison);
                Copy(copy, 0, list, 0, list.Count);
            }
        }

        public static void Sort<T>(this IList<T> list, IComparer<T> comparer)
        {
            if (list is List<T> lst)
            {
                lst.Sort(comparer);
            }
            else
            {
                var copy = new List<T>(list);
                copy.Sort(comparer);
                Copy(copy, 0, list, 0, list.Count);
            }
        }

        public static void Sort<T>(this IList<T> list, int index, int count,
            IComparer<T> comparer)
        {
            if (list is List<T> lst)
            {
                lst.Sort(index, count, comparer);
            }
            else
            {
                var range = new List<T>(count);
                for (int i = 0; i < count; i++)
                {
                    range.Add(list[index + i]);
                }
                range.Sort(comparer);
                Copy(range, 0, list, index, count);
            }
        }

        private static void Copy<T>(IList<T> sourceList, int sourceIndex,
            IList<T> destinationList, int destinationIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                destinationList[destinationIndex + i] = sourceList[sourceIndex + i];
            }
        }
    }

    // Wraps a generic Comparison<T> delegate in an IComparer to make it easy
    // to use a lambda expression for methods that take an IComparer or IComparer<T>
    public class ComparisonComparer<T> : IComparer<T>, IComparer
    {
        private readonly Comparison<T> _comparison;

        public ComparisonComparer(Comparison<T> comparison)
        {
            _comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return _comparison(x, y);
        }

        public int Compare(object o1, object o2)
        {
            return _comparison((T)o1, (T)o2);
        }
    }

    /// <summary>
    /// Defines the contract for an object that has a parent object
    /// </summary>
    /// <typeparam name="P">Type of the parent object</typeparam>
    public interface IChildItem<P> where P : class
    {
        P Parent { get; set; }
    }

    /// <summary>
    /// Collection of child items. This collection automatically set the
    /// Parent property of the child items when they are added or removed
    /// </summary>
    /// <typeparam name="P">Type of the parent object</typeparam>
    /// <typeparam name="T">Type of the child items</typeparam>
    public class ChildItemCollection<P, T> : IList<T>, INotifyCollectionChanged
        where P : class
        where T : IChildItem<P>
    {
        private P _parent;
        private IList<T> _collection;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ChildItemCollection(P parent)
        {
            this._parent = parent;
            this._collection = new List<T>();
        }

        public ChildItemCollection(P parent, IList<T> collection)
        {
            this._parent = parent;
            this._collection = collection;
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return _collection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (item != null)
                item.Parent = _parent;
            _collection.Insert(index, item);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void RemoveAt(int index)
        {
            T oldItem = _collection[index];
            _collection.RemoveAt(index);
            if (oldItem != null)
                oldItem.Parent = null;

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
        }

        public T this[int index]
        {
            get
            {
                return _collection[index];
            }
            set
            {
                T oldItem = _collection[index];
                if (!value.Equals(oldItem))
                {
                    if (value != null)
                        value.Parent = _parent;

                    _collection[index] = value;

                    if (oldItem != null && !_collection.Contains(oldItem))
                        oldItem.Parent = null;

                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            if (item != null)
                item.Parent = _parent;
            _collection.Add(item);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _collection.Count - 1));
        }

        public void Clear()
        {
            foreach (T item in _collection)
            {
                if (item != null)
                    item.Parent = null;
            }
            _collection.Clear();

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _collection.Count; }
        }

        public bool IsReadOnly
        {
            get { return _collection.IsReadOnly; }
        }

        public bool Remove(T item)
        {
            bool wasRemoved = false;

            if (_collection.Contains(item))
            {
                int index = _collection.IndexOf(item);
                wasRemoved = _collection.Remove(item);
                if (item != null)
                    item.Parent = null;

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }

            return wasRemoved;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (_collection as System.Collections.IEnumerable).GetEnumerator();
        }

        #endregion
    }
}
