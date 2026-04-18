// CustomHashTable.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// References:
// Cormen, T.H. et al. (2022) Introduction to Algorithms. 4th edn. MIT Press.
// Knuth, D.E. (1998) The Art of Computer Programming, Vol 3. Addison-Wesley.
//   -- Polynomial rolling hash function design (Section 6.4)
//   -- Separate chaining collision resolution (Section 6.3)
//   -- Load factor threshold of 0.75 for resize trigger
//
// Custom Hash Table implementation using chaining for collision resolution.
// Built from scratch without using any standard template library collections.
//
// Time Complexity:
//   Add    - O(1) average, O(n) worst case (all keys in same chain)
//   Search - O(1) average, O(n) worst case
//   Delete - O(1) average, O(n) worst case
//   Resize - O(n) triggered when load factor exceeds 0.75

using System;
using System.Collections.Generic;

namespace PostalServiceWinForms.DataStructures
{
    // Represents a single node in a hash table chain
    // Each node stores a key-value pair and a pointer to the next node
    public class HashNode
    {
        public string Key;   // Tracking ID e.g. PS-2026-00001
        public object Value; // Parcel status e.g. Pending
        public HashNode Next;  // Next node in the chain (for collision handling)

        public HashNode(string k, object v)
        {
            Key = k;
            Value = v;
        }
    }

    // Custom Hash Table using separate chaining for collision resolution
    // Stores parcel tracking IDs mapped to their current delivery status
    public class CustomHashTable
    {
        private HashNode[] _buckets;  // Array of chain heads
        private int _size;     // Current number of slots in the table
        private int _count;    // Number of items currently stored

        // Initialise the hash table with a given number of slots
        public CustomHashTable(int size = 100)
        {
            _size = size;
            _buckets = new HashNode[_size];
        }

        // Polynomial rolling hash function
        // Converts a tracking ID string into an integer array index
        // Multiplies by prime 31 to reduce collision probability
        // Time complexity: O(k) where k = key length, effectively O(1) for fixed-length tracking IDs
        private int Hash(string key)
        {
            int hash = 0;
            foreach (char c in key)
                hash = (hash * 31) + c;
            return Math.Abs(hash % _size);
        }

        // Add a new key-value pair or update an existing key
        // If the key already exists, its value is updated (no duplicate created)
        // Triggers resize when load factor exceeds 0.75
        // Time complexity: O(1) average
        public void Add(string key, object value)
        {
            int index = Hash(key);
            HashNode current = _buckets[index];

            // Walk the chain at this slot to check for existing key
            while (current != null)
            {
                if (current.Key == key)
                {
                    // Key already exists - update the value and return
                    current.Value = value;
                    return;
                }
                current = current.Next;
            }

            // Key not found - insert new node at front of chain
            HashNode newNode = new HashNode(key, value);
            newNode.Next = _buckets[index];
            _buckets[index] = newNode;
            _count++;

            // Check if table needs resizing (load factor > 0.75)
            if ((double)_count / _size > 0.75)
                Resize();
        }

        // Search for a key and return its value
        // Returns null if the key does not exist
        // Time complexity: O(1) average
        public object Search(string key)
        {
            HashNode current = _buckets[Hash(key)];

            // Walk the chain at the calculated slot
            while (current != null)
            {
                if (current.Key == key)
                    return current.Value; // Found
                current = current.Next;
            }

            // Not found in chain
            return null;
        }

        // Delete a key-value pair from the table
        // Returns true if deleted, false if key was not found
        // Time complexity: O(1) average
        public bool Delete(string key)
        {
            int index = Hash(key);
            HashNode current = _buckets[index];
            HashNode previous = null;

            while (current != null)
            {
                if (current.Key == key)
                {
                    // Remove node from chain by updating pointers
                    if (previous == null)
                        _buckets[index] = current.Next; // Was first node
                    else
                        previous.Next = current.Next;   // Bypass deleted node

                    _count--;
                    return true;
                }
                previous = current;
                current = current.Next;
            }

            // Key was not found
            return false;
        }

        // Returns true if the key exists in the table
        public bool ContainsKey(string key) => Search(key) != null;

        // Number of items currently stored in the table
        public int Count => _count;

        // Returns true if the table has no items
        public bool IsEmpty => _count == 0;

        // Remove all items from the table and reset the count
        // Time complexity: O(n) - must clear every slot
        public void Clear()
        {
            _buckets = new HashNode[_size];
            _count = 0;
        }

        // Double the table size and re-insert all existing items
        // Called automatically when load factor exceeds 0.75
        // Time complexity: O(n) - must re-hash every existing item
        private void Resize()
        {
            int newSize = _size * 2;
            HashNode[] oldBuckets = _buckets;

            // Create new larger table
            _buckets = new HashNode[newSize];
            _size = newSize;
            _count = 0;

            // Re-insert every item from the old table into the new one
            // Indices will change because table size has changed
            foreach (HashNode bucket in oldBuckets)
            {
                HashNode current = bucket;
                while (current != null)
                {
                    Add(current.Key, current.Value);
                    current = current.Next;
                }
            }
        }
    }
}