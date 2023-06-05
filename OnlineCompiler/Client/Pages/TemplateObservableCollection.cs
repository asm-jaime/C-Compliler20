namespace OnlineCompiler.Client.Pages;

public static class TemplateObservableCollection
{
    public static string ObservableCollectionCode=@"
    using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace System.Collections.ObjectModel
{
    public class MyICollectionDebugView<T>
    {
        private readonly ICollection<T> collection;

        public MyICollectionDebugView(ICollection<T> collection)
        {
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] items = new T[collection.Count];
                collection.CopyTo(items, 0);
                return items;
            }
        }
    }

    [DebuggerTypeProxy(typeof(MyICollectionDebugView<>))]
    public class ObservableCollection<T> : Collection<T>
    {
        private SimpleMonitor _monitor;

        public ObservableCollection()
        {
        }

        public ObservableCollection(IEnumerable<T> collection) : base(CreateCopy(collection, nameof(collection)))
        {
        }

        public ObservableCollection(List<T> list) : base(CreateCopy(list, nameof(list)))
        {
        }

        private static List<T> CreateCopy(IEnumerable<T> collection, string paramName)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(paramName);
            }

            return new List<T>(collection);
        }

        protected override void ClearItems()
        {
            CheckReentrancy();
            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            CheckReentrancy();
            base.RemoveItem(index);
        }

        protected override void InsertItem(int index, T item)
        {
            CheckReentrancy();
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, T item)
        {
            CheckReentrancy();
            base.SetItem(index, item);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();
            T removedItem = this[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, removedItem);
        }

        private void CheckReentrancy()
        {
            if (_monitor.BusyCount > 0)
            {
                throw new InvalidOperationException();
            }
        }

        [Serializable]
        private sealed class SimpleMonitor
        {
            private int _busyCount;

            public int BusyCount => _busyCount;

            public void Enter()
            {
                _busyCount++;
            }

            public void Exit()
            {
                _busyCount--;
            }
        }
    }
}
 
";
    
    public static string UserObservableCollectionCode=@"using System;
using System.Collections;
using System.Collections.Generic;

public class ObservableCollection<T> : IEnumerable<T>
{
    private List<T> items = new List<T>();

    public event Action<T> ItemAdded;

    public event Action<T> ItemRemoved;

    public int Count => items.Count;

    public T this[int index]
    {

    }

    public void Add(T item)
    {

    }

    public bool Remove(T item)
    {

    }

    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected virtual void OnItemAdded(T item) => ItemAdded?.Invoke(item);

    protected virtual void OnItemRemoved(T item) => ItemRemoved?.Invoke(item);
}
";
}