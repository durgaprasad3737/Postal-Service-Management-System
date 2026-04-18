// CustomQueue.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// References:
// Goodrich, M.T., Tamassia, R. and Goldwasser, M.H. (2013) Data Structures
//   and Algorithms in Java. 6th edn. Wiley.
//   -- FIFO Queue as singly linked list with front and rear pointers (Section 6.2)
//   -- O(1) enqueue and dequeue using tail pointer (Section 6.2)
//
// Custom Queue implementation using a singly linked list.
// Built from scratch without using any standard template library collections.
// Processes delivery assignments in First In First Out (FIFO) order.
//
// Time Complexity:
//   Enqueue - O(1) - adds to rear pointer directly
//   Dequeue - O(1) - removes from front pointer directly
//   Peek    - O(1) - reads front pointer value
//   IsEmpty - O(1) - checks if front pointer is null
//   ToList  - O(n) - must traverse every node

using System;
using System.Collections.Generic;

namespace PostalServiceWinForms.DataStructures
{
    // Represents a single node in the Queue linked list
    public class QueueNode
    {
        public object Value; // Delivery ID e.g. DEL-001
        public QueueNode Next;  // Pointer to the next node in the queue

        public QueueNode(object v)
        {
            Value = v;
        }
    }

    // Custom FIFO Queue implemented as a singly linked list
    // Used to manage pending delivery assignments in PostalMS
    // First delivery assigned is always the first one processed
    public class CustomQueue
    {
        private QueueNode _front; // Points to the first item (for dequeue)
        private QueueNode _rear;  // Points to the last item (for enqueue)
        private int _count; // Number of items currently in the queue

        // Add a new item to the back of the queue
        // Time complexity: O(1) - rear pointer gives direct access
        public void Enqueue(object value)
        {
            QueueNode newNode = new QueueNode(value);

            if (_rear == null)
            {
                // Queue is empty - new node is both front and rear
                _front = newNode;
                _rear = newNode;
            }
            else
            {
                // Attach new node to the end and move rear pointer
                _rear.Next = newNode;
                _rear = newNode;
            }

            _count++;
        }

        // Remove and return the item at the front of the queue (FIFO)
        // Throws InvalidOperationException if the queue is empty
        // Time complexity: O(1) - front pointer gives direct access
        public object Dequeue()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Queue is empty - no deliveries to process");

            // Save the front value before removing
            object value = _front.Value;

            // Move front pointer to the next node
            _front = _front.Next;

            // If queue is now empty, reset rear pointer too
            if (_front == null)
                _rear = null;

            _count--;
            return value;
        }

        // Return the value at the front without removing it
        // Throws InvalidOperationException if the queue is empty
        // Time complexity: O(1)
        public object Peek()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Queue is empty - nothing to peek at");

            // Return front value without changing any pointers
            return _front.Value;
        }

        // Returns true if the queue has no items
        // Time complexity: O(1)
        public bool IsEmpty() => _front == null;

        // Number of items currently in the queue
        public int Count => _count;

        // Remove all items from the queue
        // Setting pointers to null makes all nodes unreachable for garbage collection
        // Time complexity: O(1)
        public void Clear()
        {
            _front = null;
            _rear = null;
            _count = 0;
        }

        // Return all items in the queue as a list in FIFO order
        // Does not remove any items from the queue
        // Time complexity: O(n) - must traverse every node
        public List<object> ToList()
        {
            var result = new List<object>();
            QueueNode current = _front;

            // Walk from front to rear collecting all values
            while (current != null)
            {
                result.Add(current.Value);
                current = current.Next;
            }

            return result;
        }
    }
}