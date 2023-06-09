using System.Collections.ObjectModel;

namespace OnlineCompiler.Client.Pages;

public static class TemplateObservableCollection
{
    public static string ObservableCollectionCode=@"
using System.Collections.Generic;

namespace System.Collections.ObjectModel
{
    public class ObservableCollection<T> : Collection<T>
    {

        private int _blockReentrancyCount;

        /// <summary>
        /// Initializes a new instance of ObservableCollection that is empty and has default initial capacity.
        /// </summary>
        public ObservableCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class that contains
        /// elements copied from the specified collection and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the collection.
        /// </remarks>
        public ObservableCollection(IEnumerable<T> collection) : base(CreateCopy(collection, nameof(collection)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class
        /// that contains elements copied from the specified list
        /// </summary>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the list.
        /// </remarks>
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

        /// <summary>
        /// Move item at oldIndex to newIndex.
        /// </summary>
        public void Move(int oldIndex, int newIndex) => MoveItem(oldIndex, newIndex);


        /// <summary>
        /// Called by base class Collection&lt;T&gt; when the list is being cleared;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is removed from list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void RemoveItem(int index)
        {
             //Нужна реализация
             throw new NotImplementedException();
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is added to list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void InsertItem(int index, T item)
        {
             //Нужна реализация
             throw new NotImplementedException();
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is set in list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
             //Нужна реализация
             throw new NotImplementedException();
        }

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when an item is to be moved within the list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {

            T removedItem = this[oldIndex];

            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, removedItem);
        }
    }
}

";
    
    public static string UserObservableCollectionCode=@"using System.Collections.Generic;

namespace System.Collections.ObjectModel
{
    public class ObservableCollection<T> : Collection<T>
    {

        private int _blockReentrancyCount;

        /// <summary>
        /// Initializes a new instance of ObservableCollection that is empty and has default initial capacity.
        /// </summary>
        public ObservableCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class that contains
        /// elements copied from the specified collection and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the collection.
        /// </remarks>
        public ObservableCollection(IEnumerable<T> collection) : base(CreateCopy(collection, nameof(collection)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class
        /// that contains elements copied from the specified list
        /// </summary>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the list.
        /// </remarks>
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

        /// <summary>
        /// Move item at oldIndex to newIndex.
        /// </summary>
        public void Move(int oldIndex, int newIndex) => MoveItem(oldIndex, newIndex);


        /// <summary>
        /// Called by base class Collection&lt;T&gt; when the list is being cleared;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is removed from list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            T removedItem = this[index];

            base.RemoveItem(index);
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is added to list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is set in list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
        }

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when an item is to be moved within the list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {

            T removedItem = this[oldIndex];

            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, removedItem);
        }
    }
}
";
}