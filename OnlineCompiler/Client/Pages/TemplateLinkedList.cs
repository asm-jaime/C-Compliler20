namespace OnlineCompiler.Client.Pages;

public static class TemplateLinkedList
{
    public static string LinkedListCode=@"
using System;
using System.Collections;
using System.Collections.Generic;

class LinkedList<T> : IEnumerable<T>
{
    private class LinkedListNode
    {
        public T Value { get; }
        public LinkedListNode? Next { get; internal set; }

        public LinkedListNode(T value)
        {
            Value = value;
            Next = null;
        }
    }

    private LinkedListNode? head;
    private LinkedListNode? tail;
    private int count;

    public LinkedList()
    {
        head = null;
        tail = null;
        count = 0;
    }

    public void Add(T item)
    {
        LinkedListNode newNode = new LinkedListNode(item);

        if (head == null)
        {
            head = newNode;
            tail = newNode;
        }
        else
        {
            tail!.Next = newNode;
            tail = newNode;
        }

        count++;
    }

    public bool Remove(T item)
    {
        LinkedListNode? previous = null;
        LinkedListNode? current = head;

        while (current != null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Value, item))
            {
                if (previous == null)
                {
                    // Remove the head node
                    head = current.Next;
                    if (head == null)
                    {
                        // If the list becomes empty, update the tail
                        tail = null;
                    }
                }
                else
                {
                    previous.Next = current.Next;
                    if (current.Next == null)
                    {
                        // If the removed node was the tail, update the tail
                        tail = previous;
                    }
                }

                count--;
                return true;
            }

            previous = current;
            current = current.Next;
        }

        return false;
    }

    public int Count
    {
        get { return count; }
    }

    public void Clear()
    {
        head = null;
        tail = null;
        count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        LinkedListNode? current = head;

        while (current != null)
        {
            yield return current.Value;
            current = current.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
} 
 
";
    
    public static string UserLinkedListCode=@"
using System;
using System.Collections;
using System.Collections.Generic;

class LinkedList<T> : IEnumerable<T>
{
    private class LinkedListNode
    {

    }

    private LinkedListNode? head;
    private LinkedListNode? tail;
    private int count;

    public LinkedList()
    {

    }

    public void Add(T item)
    {

    }

    public bool Remove(T item)
    {

    }

    public int Count
    {

    }

    public void Clear()
    {

    }

    public IEnumerator<T> GetEnumerator()
    {

    }

    IEnumerator IEnumerable.GetEnumerator()
    {
    }
} 
";
}