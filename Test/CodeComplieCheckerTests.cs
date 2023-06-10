using NUnit.Framework;
using OnlineCompiler.Server.Handlers;

namespace test;

[TestFixture]
public class CodeComplieCheckerTests
{
    [Test]
    /*
    [TestCase(@"""using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    public class Stack<T> : System.Collections.ICollection,
        IReadOnlyCollection<T>
    {
        private T[] _array; // Storage for stack elements. Do not rename (binary serialization)
        private int _size; // Number of items in the stack. Do not rename (binary serialization)
        private int _version; // Used to keep enumerator in sync w/ collection. Do not rename (binary serialization)

        private const int DefaultCapacity = 4;

        public Stack()
        {
            _array = Array.Empty<T>();
        }

        // Create a stack with a specific initial capacity.  The initial capacity
        // must be a non-negative number.
        public Stack(int capacity)
        {
            if (capacity < 0)
                throw new InvalidOperationException();
            _array = new T[capacity];
        }

        // Fills a Stack with the contents of a particular collection.  The items are
        // pushed onto the stack in the same order they are read by the enumerator.


        public int Count
        {
            get { return _size; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot => this;

        // Removes all Objects from the Stack.
        public void Clear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_array, 0, _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            }
            _size = 0;
            _version++;
        }

        public bool Contains(T item)
        {
            // Compare items using the default equality comparer

            // PERF: Internally Array.LastIndexOf calls
            // EqualityComparer<T>.Default.LastIndexOf, which
            // is specialized for different types. This
            // boosts performance since instead of making a
            // virtual method call each iteration of the loop,
            // via EqualityComparer<T>.Default.Equals, we
            // only make one virtual call to EqualityComparer.LastIndexOf.

            return _size != 0 && Array.LastIndexOf(_array, item, _size - 1) != -1;
        }

        // Copies the stack into an array.
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new InvalidOperationException();
            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new InvalidOperationException();
            }

            if (array.Length - arrayIndex < _size)
            {
                throw new InvalidOperationException();
            }
            
            int srcIndex = 0;
            int dstIndex = arrayIndex + _size;
            while (srcIndex < _size)
            {
                array[--dstIndex] = _array[srcIndex++];
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
            {
                throw new InvalidOperationException();
            }

            if (array.Rank != 1)
            {
                throw new InvalidOperationException();
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new InvalidOperationException();
            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new InvalidOperationException();
            }

            if (array.Length - arrayIndex < _size)
            {
                throw new InvalidOperationException();
            }

            try
            {
                Array.Copy(_array, 0, array, arrayIndex, _size);
                Array.Reverse(array, arrayIndex, _size);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new InvalidOperationException();
            }
        }

        // Returns an IEnumerator for this Stack.
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <internalonly/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void TrimExcess()
        {
            int threshold = (int)(((double)_array.Length) * 0.9);
            if (_size < threshold)
            {
                Array.Resize(ref _array, _size);
                _version++;
            }
        }

        // Returns the top object on the stack without removing it.  If the stack
        // is empty, Peek throws an InvalidOperationException.
        public T Peek()
        {
            int size = _size - 1;
            T[] array = _array;

            if ((uint)size >= (uint)array.Length)
            {
                ThrowForEmptyStack();
            }

            return array[size];
        }

        public bool TryPeek(out T result)
        {
            int size = _size - 1;
            T[] array = _array;

            if ((uint)size >= (uint)array.Length)
            {
                result = default!;
                return false;
            }
            result = array[size];
            return true;
        }

        // Pops an item from the top of the stack.  If the stack is empty, Pop
        // throws an InvalidOperationException.
        public T Pop()
        {
             //Нужна реализация
             throw new NotImplementedException();
        }

        public bool TryPop(out T result)
        {
            int size = _size - 1;
            T[] array = _array;

            if ((uint)size >= (uint)array.Length)
            {
                result = default!;
                return false;
            }

            _version++;
            _size = size;
            result = array[size];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                array[size] = default!;
            }
            return true;
        }

        // Pushes an item to the top of the stack.
        public void Push(T item)
        {
             //Нужна реализация
             throw new NotImplementedException();
        }
        
        private void PushWithResize(T item)
        {
            Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);
            _array[_size] = item;
            _version++;
            _size++;
        }

        // Copies the Stack to an array, in the same order Pop would return the items.
        public T[] ToArray()
        {
            if (_size == 0)
                return Array.Empty<T>();

            T[] objArray = new T[_size];
            int i = 0;
            while (i < _size)
            {
                objArray[i] = _array[_size - i - 1];
                i++;
            }
            return objArray;
        }

        private void ThrowForEmptyStack()
        {
            throw new InvalidOperationException();
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly Stack<T> _stack;
            private readonly int _version;
            private int _index;
            private T? _currentElement;

            internal Enumerator(Stack<T> stack)
            {
                _stack = stack;
                _version = stack._version;
                _index = -2;
                _currentElement = default;
            }

            public void Dispose()
            {
                _index = -1;
            }

            public bool MoveNext()
            {
                bool retval;
                if (_version != _stack._version) throw new InvalidOperationException();
                if (_index == -2)
                {  // First call to enumerator.
                    _index = _stack._size - 1;
                    retval = (_index >= 0);
                    if (retval)
                        _currentElement = _stack._array[_index];
                    return retval;
                }
                if (_index == -1)
                {  // End of enumeration.
                    return false;
                }

                retval = (--_index >= 0);
                if (retval)
                    _currentElement = _stack._array[_index];
                else
                    _currentElement = default;
                return retval;
            }

            public T Current
            {
                get
                {
                    if (_index < 0)
                        ThrowEnumerationNotStartedOrEnded();
                    return _currentElement!;
                }
            }

            private void ThrowEnumerationNotStartedOrEnded()
            {
                throw new InvalidOperationException();
            }

            object? System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            void IEnumerator.Reset()
            {
                if (_version != _stack._version) throw new InvalidOperationException();
                _index = -2;
                _currentElement = default;
            }
        }
    }
}
""", false)]
    */

    [TestCase("\n    using System.Runtime.CompilerServices;\n\nnamespace System.Collections.Generic\n{\n    public class Stack<T> : System.Collections.ICollection,\n        IReadOnlyCollection<T>\n    {\n        private T[] _array; // Storage for stack elements. Do not rename (binary serialization)\n        private int _size; // Number of items in the stack. Do not rename (binary serialization)\n        private int _version; // Used to keep enumerator in sync w/ collection. Do not rename (binary serialization)\n\n        private const int DefaultCapacity = 4;\n\n        public Stack()\n        {\n            _array = Array.Empty<T>();\n        }\n\n        // Create a stack with a specific initial capacity.  The initial capacity\n        // must be a non-negative number.\n        public Stack(int capacity)\n        {\n            if (capacity < 0)\n                throw new InvalidOperationException();\n            _array = new T[capacity];\n        }\n\n        // Fills a Stack with the contents of a particular collection.  The items are\n        // pushed onto the stack in the same order they are read by the enumerator.\n\n\n        public int Count\n        {\n            get { return _size; }\n        }\n\n        bool ICollection.IsSynchronized\n        {\n            get { return false; }\n        }\n\n        object ICollection.SyncRoot => this;\n\n        // Removes all Objects from the Stack.\n        public void Clear()\n        {\n            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())\n            {\n                Array.Clear(_array, 0, _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.\n            }\n            _size = 0;\n            _version++;\n        }\n\n        public bool Contains(T item)\n        {\n            // Compare items using the default equality comparer\n\n            // PERF: Internally Array.LastIndexOf calls\n            // EqualityComparer<T>.Default.LastIndexOf, which\n            // is specialized for different types. This\n            // boosts performance since instead of making a\n            // virtual method call each iteration of the loop,\n            // via EqualityComparer<T>.Default.Equals, we\n            // only make one virtual call to EqualityComparer.LastIndexOf.\n\n            return _size != 0 && Array.LastIndexOf(_array, item, _size - 1) != -1;\n        }\n\n        // Copies the stack into an array.\n        public void CopyTo(T[] array, int arrayIndex)\n        {\n            if (array == null)\n            {\n                throw new InvalidOperationException();\n            }\n\n            if (arrayIndex < 0 || arrayIndex > array.Length)\n            {\n                throw new InvalidOperationException();\n            }\n\n            if (array.Length - arrayIndex < _size)\n            {\n                throw new InvalidOperationException();\n            }\n            \n            int srcIndex = 0;\n            int dstIndex = arrayIndex + _size;\n            while (srcIndex < _size)\n            {\n                array[--dstIndex] = _array[srcIndex++];\n            }\n        }\n\n        void ICollection.CopyTo(Array array, int arrayIndex)\n        {\n            if (array == null)\n            {\n                throw new InvalidOperationException();\n            }\n\n            if (array.Rank != 1)\n            {\n                throw new InvalidOperationException();\n            }\n\n            if (array.GetLowerBound(0) != 0)\n            {\n                throw new InvalidOperationException();\n            }\n\n            if (arrayIndex < 0 || arrayIndex > array.Length)\n            {\n                throw new InvalidOperationException();\n            }\n\n            if (array.Length - arrayIndex < _size)\n            {\n                throw new InvalidOperationException();\n            }\n\n            try\n            {\n                Array.Copy(_array, 0, array, arrayIndex, _size);\n                Array.Reverse(array, arrayIndex, _size);\n            }\n            catch (ArrayTypeMismatchException)\n            {\n                throw new InvalidOperationException();\n            }\n        }\n\n        // Returns an IEnumerator for this Stack.\n        public Enumerator GetEnumerator()\n        {\n            return new Enumerator(this);\n        }\n\n        /// <internalonly/>\n        IEnumerator<T> IEnumerable<T>.GetEnumerator()\n        {\n            return new Enumerator(this);\n        }\n\n        IEnumerator IEnumerable.GetEnumerator()\n        {\n            return new Enumerator(this);\n        }\n\n        public void TrimExcess()\n        {\n            int threshold = (int)(((double)_array.Length) * 0.9);\n            if (_size < threshold)\n            {\n                Array.Resize(ref _array, _size);\n                _version++;\n            }\n        }\n\n        // Returns the top object on the stack without removing it.  If the stack\n        // is empty, Peek throws an InvalidOperationException.\n        public T Peek()\n        {\n            int size = _size - 1;\n            T[] array = _array;\n\n            if ((uint)size >= (uint)array.Length)\n            {\n                ThrowForEmptyStack();\n            }\n\n            return array[size];\n        }\n\n        public bool TryPeek(out T result)\n        {\n            int size = _size - 1;\n            T[] array = _array;\n\n            if ((uint)size >= (uint)array.Length)\n            {\n                result = default!;\n                return false;\n            }\n            result = array[size];\n            return true;\n        }\n\n        // Pops an item from the top of the stack.  If the stack is empty, Pop\n        // throws an InvalidOperationException.\n        public T Pop()\n        {\n             //Нужна реализация\n T result = default!; return result;\n        }\n\n        public bool TryPop(out T result)\n        {\n            int size = _size - 1;\n            T[] array = _array;\n\n            if ((uint)size >= (uint)array.Length)\n            {\n                result = default!;\n                return false;\n            }\n\n            _version++;\n            _size = size;\n            result = array[size];\n            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())\n            {\n                array[size] = default!;\n            }\n            return true;\n        }\n\n        // Pushes an item to the top of the stack.\n        public void Push(T item)\n        {\n             //Нужна реализация\n  return;\n        }\n        \n        private void PushWithResize(T item)\n        {\n            Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);\n            _array[_size] = item;\n            _version++;\n            _size++;\n        }\n\n        // Copies the Stack to an array, in the same order Pop would return the items.\n        public T[] ToArray()\n        {\n            if (_size == 0)\n                return Array.Empty<T>();\n\n            T[] objArray = new T[_size];\n            int i = 0;\n            while (i < _size)\n            {\n                objArray[i] = _array[_size - i - 1];\n                i++;\n            }\n            return objArray;\n        }\n\n        private void ThrowForEmptyStack()\n        {\n            throw new InvalidOperationException();\n        }\n\n        public struct Enumerator : IEnumerator<T>\n        {\n            private readonly Stack<T> _stack;\n            private readonly int _version;\n            private int _index;\n            private T? _currentElement;\n\n            internal Enumerator(Stack<T> stack)\n            {\n                _stack = stack;\n                _version = stack._version;\n                _index = -2;\n                _currentElement = default;\n            }\n\n            public void Dispose()\n            {\n                _index = -1;\n            }\n\n            public bool MoveNext()\n            {\n                bool retval;\n                if (_version != _stack._version) throw new InvalidOperationException();\n                if (_index == -2)\n                {  // First call to enumerator.\n                    _index = _stack._size - 1;\n                    retval = (_index >= 0);\n                    if (retval)\n                        _currentElement = _stack._array[_index];\n                    return retval;\n                }\n                if (_index == -1)\n                {  // End of enumeration.\n                    return false;\n                }\n\n                retval = (--_index >= 0);\n                if (retval)\n                    _currentElement = _stack._array[_index];\n                else\n                    _currentElement = default;\n                return retval;\n            }\n\n            public T Current\n            {\n                get\n                {\n                    if (_index < 0)\n                        ThrowEnumerationNotStartedOrEnded();\n                    return _currentElement!;\n                }\n            }\n\n            private void ThrowEnumerationNotStartedOrEnded()\n            {\n                throw new InvalidOperationException();\n            }\n\n            object? System.Collections.IEnumerator.Current\n            {\n                get { return Current; }\n            }\n\n            void IEnumerator.Reset()\n            {\n                if (_version != _stack._version) throw new InvalidOperationException();\n                _index = -2;\n                _currentElement = default;\n            }\n        }\n    }\n}", false)]
    public void TestStack(string code, bool expected)
    {
        // Arrange
        var stackType = DynamicClassCreator.CreateClassFromCode(code, "Stack");
        Type constructedType = stackType.MakeGenericType(typeof(int));
        var stackInstance = Activator.CreateInstance(constructedType);

        // Act
        constructedType.GetMethod("Push").Invoke(stackInstance, new Object[] {1});
        constructedType.GetMethod("Push").Invoke(stackInstance, new Object[] {2});
        constructedType.GetMethod("Push").Invoke(stackInstance, new Object[] {3});

        var poppedItem = constructedType.GetMethod("Pop").Invoke(stackInstance, null);

        // Assert
        Assert.AreEqual(CodeCompileChecker<int>.Check(new Stack<int>(), constructedType, stackInstance), expected);
    }
}

