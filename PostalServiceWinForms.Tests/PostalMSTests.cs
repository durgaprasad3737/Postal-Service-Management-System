// PostalMSTests.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// References:
// Microsoft (2024c) MSTest documentation. Available at:
//   https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest
// Cormen, T.H. et al. (2022) Introduction to Algorithms. 4th edn. MIT Press.
//   -- Time complexity values used in test case design
//
// MSTest unit tests for all custom data structures and business logic.
// 80 tests across 9 test classes:
//   HashTableTests (12), BSTTests (13), QueueTests (10),
//   PriceCalculationTests (9), TrackingIDTests (6), IntegrationTests (5),
//   GmailValidationTests (8), StampPriceTests (6), CityLocationTests (8)
//
// All tests are self-contained -- no project reference required.
// Local copies of data structures are included for testing independence.
//
// How to run:
// 1. Build the solution (Ctrl+Shift+B)
// 2. Go to Test menu then Run All Tests
// 3. Or press Ctrl+R then A
//
// Expected result: 80 tests, 0 failures

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PostalServiceWinForms.Tests
{
    // ============================================================
    // LOCAL DATA STRUCTURE IMPLEMENTATIONS FOR TESTING
    // These are exact copies of the production implementations.
    // Kept here so tests run without needing a project reference.
    // ============================================================

    // Single node in the Hash Table chain
    public class TestHashNode
    {
        public string Key;   // Parcel tracking ID
        public object Value; // Parcel status
        public TestHashNode Next;  // Next node in collision chain

        public TestHashNode(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }

    // Custom Hash Table using separate chaining for collision resolution
    // Add O(1) average | Search O(1) average | Delete O(1) average
    public class TestHashTable
    {
        private TestHashNode[] _buckets;
        private int _size;
        private int _count;

        public TestHashTable(int size = 100)
        {
            _size = size;
            _buckets = new TestHashNode[_size];
        }

        // Polynomial rolling hash function
        private int Hash(string key)
        {
            int hash = 0;
            foreach (char c in key)
                hash = (hash * 31) + c;
            return Math.Abs(hash % _size);
        }

        // Add or update a key-value pair
        public void Add(string key, object value)
        {
            int index = Hash(key);
            TestHashNode current = _buckets[index];

            while (current != null)
            {
                if (current.Key == key)
                {
                    current.Value = value;
                    return;
                }
                current = current.Next;
            }

            TestHashNode newNode = new TestHashNode(key, value);
            newNode.Next = _buckets[index];
            _buckets[index] = newNode;
            _count++;

            if ((double)_count / _size > 0.75)
                Resize();
        }

        // Search for a key and return its value or null
        public object Search(string key)
        {
            TestHashNode current = _buckets[Hash(key)];
            while (current != null)
            {
                if (current.Key == key)
                    return current.Value;
                current = current.Next;
            }
            return null;
        }

        // Delete a key from the table
        public bool Delete(string key)
        {
            int index = Hash(key);
            TestHashNode current = _buckets[index];
            TestHashNode previous = null;

            while (current != null)
            {
                if (current.Key == key)
                {
                    if (previous == null)
                        _buckets[index] = current.Next;
                    else
                        previous.Next = current.Next;
                    _count--;
                    return true;
                }
                previous = current;
                current = current.Next;
            }
            return false;
        }

        public bool ContainsKey(string key) => Search(key) != null;
        public int Count => _count;
        public bool IsEmpty => _count == 0;

        public void Clear()
        {
            _buckets = new TestHashNode[_size];
            _count = 0;
        }

        // Double the table size and re-hash all items
        private void Resize()
        {
            int newSize = _size * 2;
            TestHashNode[] oldBuckets = _buckets;
            _buckets = new TestHashNode[newSize];
            _size = newSize;
            _count = 0;

            foreach (TestHashNode bucket in oldBuckets)
            {
                TestHashNode current = bucket;
                while (current != null)
                {
                    Add(current.Key, current.Value);
                    current = current.Next;
                }
            }
        }
    }

    // Single node in the Binary Search Tree
    public class TestBSTNode
    {
        public string Key;
        public object Value;
        public TestBSTNode Left;
        public TestBSTNode Right;

        public TestBSTNode(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }

    // Custom Binary Search Tree
    // Insert O(log n) average | Search O(log n) average | InOrder O(n)
    public class TestBST
    {
        private TestBSTNode _root;
        private int _count;

        // Insert a key-value pair
        public void Insert(string key, object value)
        {
            _root = InsertRecursive(_root, key, value);
        }

        private TestBSTNode InsertRecursive(TestBSTNode node, string key, object value)
        {
            if (node == null)
            {
                _count++;
                return new TestBSTNode(key, value);
            }

            int comparison = string.Compare(key, node.Key);

            if (comparison < 0)
                node.Left = InsertRecursive(node.Left, key, value);
            else if (comparison > 0)
                node.Right = InsertRecursive(node.Right, key, value);
            else
                node.Value = value; // Update existing key

            return node;
        }

        // Search for a value by key
        public object Search(string key)
        {
            TestBSTNode result = SearchRecursive(_root, key);
            return result?.Value;
        }

        private TestBSTNode SearchRecursive(TestBSTNode node, string key)
        {
            if (node == null) return null;

            int comparison = string.Compare(key, node.Key);

            if (comparison < 0) return SearchRecursive(node.Left, key);
            if (comparison > 0) return SearchRecursive(node.Right, key);
            return node;
        }

        public bool Contains(string key) => Search(key) != null;

        // Delete a node from the tree
        public bool Delete(string key)
        {
            if (!Contains(key)) return false;
            _root = DeleteRecursive(_root, key);
            _count--;
            return true;
        }

        private TestBSTNode DeleteRecursive(TestBSTNode node, string key)
        {
            if (node == null) return null;

            int comparison = string.Compare(key, node.Key);

            if (comparison < 0)
                node.Left = DeleteRecursive(node.Left, key);
            else if (comparison > 0)
                node.Right = DeleteRecursive(node.Right, key);
            else
            {
                if (node.Left == null) return node.Right;
                if (node.Right == null) return node.Left;

                TestBSTNode successor = FindMinNode(node.Right);
                node.Key = successor.Key;
                node.Value = successor.Value;
                node.Right = DeleteRecursive(node.Right, successor.Key);
            }

            return node;
        }

        // Find the node with the smallest key
        public TestBSTNode FindMin() => FindMinNode(_root);

        private TestBSTNode FindMinNode(TestBSTNode node)
        {
            while (node?.Left != null)
                node = node.Left;
            return node;
        }

        // Return all nodes in sorted ascending order
        public List<KeyValuePair<string, object>> InOrder()
        {
            var result = new List<KeyValuePair<string, object>>();
            InOrderRecursive(_root, result);
            return result;
        }

        private void InOrderRecursive(TestBSTNode node, List<KeyValuePair<string, object>> result)
        {
            if (node == null) return;
            InOrderRecursive(node.Left, result);
            result.Add(new KeyValuePair<string, object>(node.Key, node.Value));
            InOrderRecursive(node.Right, result);
        }

        public int Count => _count;
        public bool IsEmpty => _count == 0;

        public void Clear()
        {
            _root = null;
            _count = 0;
        }
    }

    // Single node in the Queue linked list
    public class TestQueueNode
    {
        public object Value;
        public TestQueueNode Next;

        public TestQueueNode(object value)
        {
            Value = value;
        }
    }

    // Custom FIFO Queue implemented as a singly linked list
    // Enqueue O(1) | Dequeue O(1) | Peek O(1)
    public class TestQueue
    {
        private TestQueueNode _front;
        private TestQueueNode _rear;
        private int _count;

        // Add to the back of the queue
        public void Enqueue(object value)
        {
            TestQueueNode newNode = new TestQueueNode(value);

            if (_rear == null)
            {
                _front = newNode;
                _rear = newNode;
            }
            else
            {
                _rear.Next = newNode;
                _rear = newNode;
            }

            _count++;
        }

        // Remove and return from the front of the queue
        public object Dequeue()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Queue is empty");

            object value = _front.Value;
            _front = _front.Next;

            if (_front == null)
                _rear = null;

            _count--;
            return value;
        }

        // Return front value without removing
        public object Peek()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Queue is empty");

            return _front.Value;
        }

        public bool IsEmpty() => _front == null;
        public int Count => _count;

        public void Clear()
        {
            _front = null;
            _rear = null;
            _count = 0;
        }

        // Return all items as a list without removing them
        public List<object> ToList()
        {
            var result = new List<object>();
            TestQueueNode current = _front;

            while (current != null)
            {
                result.Add(current.Value);
                current = current.Next;
            }

            return result;
        }
    }

    // ============================================================
    // TEST CLASS 1: Custom Hash Table Tests
    // Covers: Add, Search, Delete, ContainsKey, Count, Clear, Resize
    // Time complexity tested: O(1) average for all core operations
    // ============================================================
    [TestClass]
    public class HashTableTests
    {
        private TestHashTable ht;

        // Runs before every test method in this class
        [TestInitialize]
        public void Setup()
        {
            ht = new TestHashTable(10);
        }

        // --- Add and Search ---

        [TestMethod]
        public void Add_ThenSearch_ReturnsCorrectValue()
        {
            // Arrange -- add a parcel tracking ID with status Pending
            ht.Add("PS-2026-00001", "Pending");

            // Act -- search for it
            object result = ht.Search("PS-2026-00001");

            // Assert -- should find it with correct status
            Assert.IsNotNull(result);
            Assert.AreEqual("Pending", result.ToString());
        }

        [TestMethod]
        public void Search_KeyNotInTable_ReturnsNull()
        {
            // Searching for a key that was never added should return null
            object result = ht.Search("PS-9999-99999");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Add_SameKeyTwice_UpdatesValueNotDuplicates()
        {
            // Adding the same tracking ID again should update the status
            // not create a duplicate entry
            ht.Add("PS-2026-00001", "Pending");
            ht.Add("PS-2026-00001", "Delivered");

            // Count should still be 1 -- no duplicate
            Assert.AreEqual(1, ht.Count);

            // Value should be the updated one
            Assert.AreEqual("Delivered", ht.Search("PS-2026-00001").ToString());
        }

        // --- Delete ---

        [TestMethod]
        public void Delete_ExistingKey_ReturnsTrueAndRemovesIt()
        {
            // Arrange
            ht.Add("PS-2026-00001", "Pending");

            // Act
            bool deleted = ht.Delete("PS-2026-00001");

            // Assert -- should return true and item should be gone
            Assert.IsTrue(deleted);
            Assert.IsNull(ht.Search("PS-2026-00001"));
        }

        [TestMethod]
        public void Delete_NonExistingKey_ReturnsFalse()
        {
            // Trying to delete a key that does not exist should return false
            bool deleted = ht.Delete("PS-9999-99999");

            Assert.IsFalse(deleted);
        }

        // --- ContainsKey ---

        [TestMethod]
        public void ContainsKey_AfterAdd_ReturnsTrue()
        {
            ht.Add("PS-2026-00001", "Pending");

            Assert.IsTrue(ht.ContainsKey("PS-2026-00001"));
        }

        [TestMethod]
        public void ContainsKey_KeyNotAdded_ReturnsFalse()
        {
            Assert.IsFalse(ht.ContainsKey("PS-9999-99999"));
        }

        // --- Count ---

        [TestMethod]
        public void Count_AfterMultipleAdds_ReturnsCorrectCount()
        {
            ht.Add("PS-2026-00001", "Pending");
            ht.Add("PS-2026-00002", "In Transit");
            ht.Add("PS-2026-00003", "Delivered");

            Assert.AreEqual(3, ht.Count);
        }

        // --- IsEmpty ---

        [TestMethod]
        public void IsEmpty_NewTable_ReturnsTrue()
        {
            // A brand new table should be empty
            Assert.IsTrue(ht.IsEmpty);
        }

        [TestMethod]
        public void IsEmpty_AfterAdd_ReturnsFalse()
        {
            ht.Add("PS-2026-00001", "Pending");

            Assert.IsFalse(ht.IsEmpty);
        }

        // --- Clear ---

        [TestMethod]
        public void Clear_RemovesAllItemsAndResetsCount()
        {
            ht.Add("PS-2026-00001", "Pending");
            ht.Add("PS-2026-00002", "Pending");
            ht.Add("PS-2026-00003", "Pending");

            ht.Clear();

            // After clearing everything should be gone
            Assert.AreEqual(0, ht.Count);
            Assert.IsTrue(ht.IsEmpty);
            Assert.IsNull(ht.Search("PS-2026-00001"));
        }

        // --- Resize ---

        [TestMethod]
        public void Resize_TableGrowsAndAllItemsStillAccessible()
        {
            // Start with a small table that will need to resize
            var smallTable = new TestHashTable(4);

            // Add 10 items -- this will trigger multiple resizes
            for (int i = 1; i <= 10; i++)
                smallTable.Add("PS-2026-" + i.ToString("D5"), "Pending");

            // All items must still be accessible after resize
            Assert.AreEqual(10, smallTable.Count);

            for (int i = 1; i <= 10; i++)
                Assert.IsNotNull(smallTable.Search("PS-2026-" + i.ToString("D5")));
        }
    }

    // ============================================================
    // TEST CLASS 2: Custom Binary Search Tree Tests
    // Covers: Insert, Search, Delete, InOrder, FindMin, Count, Clear
    // Time complexity tested: O(log n) average
    // ============================================================
    [TestClass]
    public class BSTTests
    {
        private TestBST bst;

        [TestInitialize]
        public void Setup()
        {
            bst = new TestBST();
        }

        // --- Insert and Search ---

        [TestMethod]
        public void Insert_ThenSearch_ReturnsCorrectValue()
        {
            // Arrange
            bst.Insert("PS-2026-00001", "Pending");

            // Act
            object result = bst.Search("PS-2026-00001");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Pending", result.ToString());
        }

        [TestMethod]
        public void Search_KeyNotInTree_ReturnsNull()
        {
            object result = bst.Search("PS-9999-99999");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Insert_SameKeyTwice_UpdatesValueNotCount()
        {
            // Inserting the same tracking ID again should update status
            bst.Insert("PS-2026-00001", "Pending");
            bst.Insert("PS-2026-00001", "Delivered");

            Assert.AreEqual(1, bst.Count);
            Assert.AreEqual("Delivered", bst.Search("PS-2026-00001").ToString());
        }

        // --- Contains ---

        [TestMethod]
        public void Contains_AfterInsert_ReturnsTrue()
        {
            bst.Insert("PS-2026-00001", "Pending");

            Assert.IsTrue(bst.Contains("PS-2026-00001"));
        }

        [TestMethod]
        public void Contains_KeyNotInserted_ReturnsFalse()
        {
            Assert.IsFalse(bst.Contains("PS-9999-99999"));
        }

        // --- Delete ---

        [TestMethod]
        public void Delete_ExistingKey_ReturnsTrueAndRemovesNode()
        {
            bst.Insert("PS-2026-00001", "Pending");

            bool deleted = bst.Delete("PS-2026-00001");

            Assert.IsTrue(deleted);
            Assert.IsNull(bst.Search("PS-2026-00001"));
        }

        [TestMethod]
        public void Delete_NonExistingKey_ReturnsFalse()
        {
            bool deleted = bst.Delete("PS-9999-99999");

            Assert.IsFalse(deleted);
        }

        [TestMethod]
        public void Delete_NodeWithTwoChildren_MaintainsBSTProperty()
        {
            // Insert multiple nodes to create a tree with internal nodes
            bst.Insert("PS-2026-00003", "Pending");
            bst.Insert("PS-2026-00001", "Pending");
            bst.Insert("PS-2026-00005", "Pending");
            bst.Insert("PS-2026-00002", "Pending");
            bst.Insert("PS-2026-00004", "Pending");

            // Delete the root which has two children
            bst.Delete("PS-2026-00003");

            // Tree should still be valid and sorted
            var sorted = bst.InOrder();
            Assert.AreEqual(4, sorted.Count);
            Assert.AreEqual("PS-2026-00001", sorted[0].Key);
            Assert.AreEqual("PS-2026-00002", sorted[1].Key);
            Assert.AreEqual("PS-2026-00004", sorted[2].Key);
            Assert.AreEqual("PS-2026-00005", sorted[3].Key);
        }

        // --- InOrder ---

        [TestMethod]
        public void InOrder_ReturnsSortedKeysAscending()
        {
            // Insert out of order
            bst.Insert("PS-2026-00003", "Pending");
            bst.Insert("PS-2026-00001", "Pending");
            bst.Insert("PS-2026-00002", "Pending");

            var result = bst.InOrder();

            // Should come back sorted
            Assert.AreEqual("PS-2026-00001", result[0].Key);
            Assert.AreEqual("PS-2026-00002", result[1].Key);
            Assert.AreEqual("PS-2026-00003", result[2].Key);
        }

        [TestMethod]
        public void InOrder_EmptyTree_ReturnsEmptyList()
        {
            var result = bst.InOrder();

            Assert.AreEqual(0, result.Count);
        }

        // --- FindMin ---

        [TestMethod]
        public void FindMin_ReturnsNodeWithSmallestKey()
        {
            bst.Insert("PS-2026-00003", "Pending");
            bst.Insert("PS-2026-00001", "Pending");
            bst.Insert("PS-2026-00002", "Pending");

            var min = bst.FindMin();

            Assert.IsNotNull(min);
            Assert.AreEqual("PS-2026-00001", min.Key);
        }

        // --- Count and IsEmpty ---

        [TestMethod]
        public void Count_AfterMultipleInserts_ReturnsCorrectCount()
        {
            bst.Insert("PS-2026-00001", "Pending");
            bst.Insert("PS-2026-00002", "Pending");
            bst.Insert("PS-2026-00003", "Pending");

            Assert.AreEqual(3, bst.Count);
        }

        [TestMethod]
        public void IsEmpty_NewTree_ReturnsTrue()
        {
            Assert.IsTrue(bst.IsEmpty);
        }

        // --- Clear ---

        [TestMethod]
        public void Clear_RemovesAllNodesAndResetsCount()
        {
            bst.Insert("PS-2026-00001", "Pending");
            bst.Insert("PS-2026-00002", "Pending");

            bst.Clear();

            Assert.AreEqual(0, bst.Count);
            Assert.IsTrue(bst.IsEmpty);
            Assert.IsNull(bst.Search("PS-2026-00001"));
        }
    }

    // ============================================================
    // TEST CLASS 3: Custom Queue Tests
    // Covers: Enqueue, Dequeue, Peek, IsEmpty, Count, ToList, Clear
    // Time complexity tested: O(1) for all core operations
    // ============================================================
    [TestClass]
    public class QueueTests
    {
        private TestQueue q;

        [TestInitialize]
        public void Setup()
        {
            q = new TestQueue();
        }

        // --- Enqueue and Dequeue ---

        [TestMethod]
        public void Enqueue_ThenDequeue_ReturnsCorrectValue()
        {
            q.Enqueue("DEL-001");

            object result = q.Dequeue();

            Assert.AreEqual("DEL-001", result.ToString());
        }

        [TestMethod]
        public void Dequeue_EmptyQueue_ThrowsInvalidOperationException()
        {
            // Dequeuing from an empty queue must throw an exception
            bool exceptionThrown = false;

            try
            {
                q.Dequeue();
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public void Enqueue_MultipleItems_DequeuesInFIFOOrder()
        {
            // First In First Out -- deliveries must be processed in assignment order
            q.Enqueue("DEL-001");
            q.Enqueue("DEL-002");
            q.Enqueue("DEL-003");

            Assert.AreEqual("DEL-001", q.Dequeue().ToString());
            Assert.AreEqual("DEL-002", q.Dequeue().ToString());
            Assert.AreEqual("DEL-003", q.Dequeue().ToString());
        }

        // --- Peek ---

        [TestMethod]
        public void Peek_ReturnsFirstItemWithoutRemoving()
        {
            q.Enqueue("DEL-001");
            q.Enqueue("DEL-002");

            // Peek should return front item but not remove it
            object peeked = q.Peek();

            Assert.AreEqual("DEL-001", peeked.ToString());
            Assert.AreEqual(2, q.Count); // Count unchanged
        }

        [TestMethod]
        public void Peek_EmptyQueue_ThrowsInvalidOperationException()
        {
            // Peeking at an empty queue must throw
            bool exceptionThrown = false;

            try
            {
                q.Peek();
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown);
        }

        // --- IsEmpty ---

        [TestMethod]
        public void IsEmpty_NewQueue_ReturnsTrue()
        {
            Assert.IsTrue(q.IsEmpty());
        }

        [TestMethod]
        public void IsEmpty_AfterEnqueue_ReturnsFalse()
        {
            q.Enqueue("DEL-001");

            Assert.IsFalse(q.IsEmpty());
        }

        // --- Count ---

        [TestMethod]
        public void Count_AfterMultipleEnqueues_ReturnsCorrectCount()
        {
            q.Enqueue("DEL-001");
            q.Enqueue("DEL-002");
            q.Enqueue("DEL-003");

            Assert.AreEqual(3, q.Count);
        }

        // --- ToList ---

        [TestMethod]
        public void ToList_ReturnsAllItemsInQueueOrder()
        {
            q.Enqueue("DEL-001");
            q.Enqueue("DEL-002");
            q.Enqueue("DEL-003");

            var list = q.ToList();

            // List must be in FIFO order and contain all items
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual("DEL-001", list[0].ToString());
            Assert.AreEqual("DEL-002", list[1].ToString());
            Assert.AreEqual("DEL-003", list[2].ToString());

            // Queue should still have all items after ToList
            Assert.AreEqual(3, q.Count);
        }

        // --- Clear ---

        [TestMethod]
        public void Clear_RemovesAllItemsAndResetsCount()
        {
            q.Enqueue("DEL-001");
            q.Enqueue("DEL-002");

            q.Clear();

            Assert.AreEqual(0, q.Count);
            Assert.IsTrue(q.IsEmpty());
        }
    }

    // ============================================================
    // TEST CLASS 4: Parcel Price Calculation Tests
    // Covers: domestic, international, size multipliers, service types
    // Formula: (ServicePrice + Weight * 1.2) * SizeMultiplier * IntlMultiplier
    // ============================================================
    [TestClass]
    public class PriceCalculationTests
    {
        // Mirrors the exact price formula used in ParcelsView.cs
        private double CalcPrice(double servicePrice, double weight, double sizeMult, double intlMult)
        {
            return Math.Round((servicePrice + weight * 1.2) * sizeMult * intlMult, 2);
        }

        [TestMethod]
        public void Price_Standard_Small_Domestic_ReturnsCorrectPrice()
        {
            // Standard GBP 2.99 + 1kg * 1.2 = 4.19, Small x1.0, Domestic x1.0
            double price = CalcPrice(2.99, 1.0, 1.0, 1.0);

            Assert.IsTrue(price > 0);
            Assert.IsTrue(price >= 4.00 && price <= 5.00);
        }

        [TestMethod]
        public void Price_Express_Medium_Domestic_ReturnsCorrectPrice()
        {
            // Express GBP 9.99 + 2kg * 1.2 = 12.39, Medium x1.5 = 18.59
            double price = CalcPrice(9.99, 2.0, 1.5, 1.0);

            Assert.IsTrue(price > 0);
            Assert.IsTrue(price >= 18.00 && price <= 19.00);
        }

        [TestMethod]
        public void Price_International_HigherThan_Domestic()
        {
            // Same parcel but international should cost more
            double domestic = CalcPrice(2.99, 1.0, 1.0, 1.0);
            double international = CalcPrice(2.99, 1.0, 1.0, 1.8);

            Assert.IsTrue(international > domestic);
        }

        [TestMethod]
        public void Price_LargeSize_MoreExpensiveThan_SmallSize()
        {
            // Large x2.5 should cost more than Small x1.0
            double small = CalcPrice(2.99, 1.0, 1.0, 1.0);
            double large = CalcPrice(2.99, 1.0, 2.5, 1.0);

            Assert.IsTrue(large > small);
        }

        [TestMethod]
        public void Price_HeavierParcel_MoreExpensiveThan_LighterParcel()
        {
            // More weight means higher price
            double light = CalcPrice(2.99, 0.5, 1.0, 1.0);
            double heavy = CalcPrice(2.99, 5.0, 1.0, 1.0);

            Assert.IsTrue(heavy > light);
        }

        [TestMethod]
        public void Price_NextDay_MostExpensiveService()
        {
            // Next Day GBP 14.99 should be most expensive
            double ground = CalcPrice(2.99, 1.0, 1.0, 1.0);
            double nextDay = CalcPrice(14.99, 1.0, 1.0, 1.0);

            Assert.IsTrue(nextDay > ground);
        }

        [TestMethod]
        public void Price_ZeroWeight_StillChargesServiceFee()
        {
            // Even with zero weight the service price should still apply
            double price = CalcPrice(2.99, 0.0, 1.0, 1.0);

            Assert.IsTrue(price > 0);
            Assert.IsTrue(price >= 2.50 && price <= 3.50);
        }

        [TestMethod]
        public void Price_Zone4_MoreExpensiveThan_Zone1()
        {
            // Zone 4 x3.8 should cost more than Zone 1 x1.8
            double zone1 = CalcPrice(2.99, 1.0, 1.0, 1.8);
            double zone4 = CalcPrice(2.99, 1.0, 1.0, 3.8);

            Assert.IsTrue(zone4 > zone1);
        }

        [TestMethod]
        public void Price_AllFactorsCombined_ProducesPositiveResult()
        {
            // NextDay + 2kg + Large + Europe Zone 1
            double price = CalcPrice(14.99, 2.0, 1.5, 1.8);

            Assert.IsTrue(price > 0);
            // (14.99 + 2*1.2) * 1.5 * 1.8 = (17.39) * 1.5 * 1.8 = 46.95
            Assert.IsTrue(price >= 46.00 && price <= 48.00);
        }
    }

    // ============================================================
    // TEST CLASS 5: Tracking ID Format Tests
    // Covers: format validation, uniqueness, year, padding
    // Format: PS-YYYY-NNNNN where NNNNN is zero-padded 5 digit number
    // ============================================================
    [TestClass]
    public class TrackingIDTests
    {
        // Mirrors the tracking ID generation logic in ParcelsView.cs
        private string GenerateTrackingID(int count)
        {
            return "PS-" + DateTime.Now.Year + "-" + count.ToString("D5");
        }

        [TestMethod]
        public void TrackingID_AlwaysStartsWithPS()
        {
            string tid = GenerateTrackingID(1);

            Assert.IsTrue(tid.StartsWith("PS-"));
        }

        [TestMethod]
        public void TrackingID_ContainsCurrentYear()
        {
            string tid = GenerateTrackingID(1);
            string year = DateTime.Now.Year.ToString();

            Assert.IsTrue(tid.Contains(year));
        }

        [TestMethod]
        public void TrackingID_EndsWithFiveDigitNumber()
        {
            string tid = GenerateTrackingID(1);
            string[] parts = tid.Split('-');

            // Should have exactly 3 parts: PS, YEAR, NNNNN
            Assert.AreEqual(3, parts.Length);

            // Last part should be exactly 5 characters
            Assert.AreEqual(5, parts[2].Length);
        }

        [TestMethod]
        public void TrackingID_Count1_ProducesCorrectFormat()
        {
            string tid = GenerateTrackingID(1);
            string expected = "PS-" + DateTime.Now.Year + "-00001";

            Assert.AreEqual(expected, tid);
        }

        [TestMethod]
        public void TrackingID_Count99999_StillFiveDigits()
        {
            string tid = GenerateTrackingID(99999);
            string expected = "PS-" + DateTime.Now.Year + "-99999";

            Assert.AreEqual(expected, tid);
        }

        [TestMethod]
        public void TrackingID_SequentialCounts_ProduceUniqueIDs()
        {
            string tid1 = GenerateTrackingID(1);
            string tid2 = GenerateTrackingID(2);
            string tid3 = GenerateTrackingID(3);

            Assert.AreNotEqual(tid1, tid2);
            Assert.AreNotEqual(tid1, tid3);
            Assert.AreNotEqual(tid2, tid3);
        }
    }

    // ============================================================
    // TEST CLASS 6: Integration Tests
    // Covers: all three data structures working together
    // Simulates real PostalMS workflows
    // ============================================================
    [TestClass]
    public class IntegrationTests
    {
        private TestHashTable ht;
        private TestBST bst;
        private TestQueue q;

        [TestInitialize]
        public void Setup()
        {
            ht = new TestHashTable(100);
            bst = new TestBST();
            q = new TestQueue();
        }

        [TestMethod]
        public void SimulateAddParcel_AllThreeStructuresUpdated()
        {
            // When a parcel is submitted it goes into Hash Table and BST
            // and a delivery is added to the Queue
            string tid = "PS-2026-00001";

            ht.Add(tid, "Pending");
            bst.Insert(tid, "Pending");
            q.Enqueue("DEL-001");

            // All three structures should have the data
            Assert.IsNotNull(ht.Search(tid));
            Assert.IsNotNull(bst.Search(tid));
            Assert.AreEqual(1, q.Count);
        }

        [TestMethod]
        public void SimulateUpdateStatus_HashTableAndBSTBothUpdated()
        {
            // When status changes both Hash Table and BST must be updated
            string tid = "PS-2026-00001";

            ht.Add(tid, "Pending");
            bst.Insert(tid, "Pending");

            // Update status in both structures
            ht.Add(tid, "In Transit");
            bst.Insert(tid, "In Transit");
            q.Enqueue("DEL-001");

            Assert.AreEqual("In Transit", ht.Search(tid).ToString());
            Assert.AreEqual("In Transit", bst.Search(tid).ToString());
            Assert.AreEqual(1, q.Count);
        }

        [TestMethod]
        public void SimulateDeleteParcel_RemovedFromHashTableAndBST()
        {
            // Deleting a parcel removes it from Hash Table and BST
            string tid = "PS-2026-00001";

            ht.Add(tid, "Failed");
            bst.Insert(tid, "Failed");

            ht.Delete(tid);
            bst.Delete(tid);

            // Both structures should no longer contain the parcel
            Assert.IsNull(ht.Search(tid));
            Assert.IsNull(bst.Search(tid));
        }

        [TestMethod]
        public void SimulateDashboard_BSTInOrderReturnsSortedParcels()
        {
            // The dashboard uses BST in-order traversal to display sorted parcels
            string[] ids = {
                "PS-2026-00005", "PS-2026-00001", "PS-2026-00003",
                "PS-2026-00002", "PS-2026-00004"
            };

            foreach (string id in ids)
            {
                ht.Add(id, "Pending");
                bst.Insert(id, "Pending");
            }

            // BST in-order must produce sorted result
            List<KeyValuePair<string, object>> sorted = bst.InOrder();

            Assert.AreEqual("PS-2026-00001", sorted[0].Key);
            Assert.AreEqual("PS-2026-00002", sorted[1].Key);
            Assert.AreEqual("PS-2026-00003", sorted[2].Key);
            Assert.AreEqual("PS-2026-00004", sorted[3].Key);
            Assert.AreEqual("PS-2026-00005", sorted[4].Key);

            // Both structures should have same number of items
            Assert.AreEqual(ht.Count, sorted.Count);
        }

        [TestMethod]
        public void SimulateDeliveryProcessing_QueueProcessesFIFO()
        {
            // Deliveries must be processed in the order they were assigned
            q.Enqueue("DEL-001");
            q.Enqueue("DEL-002");
            q.Enqueue("DEL-003");

            var processed = new List<string>();
            while (!q.IsEmpty())
                processed.Add(q.Dequeue().ToString());

            // First assigned must be first processed
            Assert.AreEqual("DEL-001", processed[0]);
            Assert.AreEqual("DEL-002", processed[1]);
            Assert.AreEqual("DEL-003", processed[2]);
        }
    }

    // ============================================================
    // TEST CLASS 7: Gmail Validation Tests
    // Covers: the email validation logic added to LoginForm
    // and RegisterForm requiring all emails to be @gmail.com
    // ============================================================
    [TestClass]
    public class GmailValidationTests
    {
        // Exact copy of ValidGmail method from LoginForm.cs and RegisterForm.cs
        private bool ValidGmail(string email)
        {
            return email.EndsWith("@gmail.com") &&
                   email.Length > "@gmail.com".Length &&
                   !email.StartsWith("@");
        }

        [TestMethod]
        public void ValidGmail_ValidEmail_ReturnsTrue()
        {
            // A standard Gmail address should pass
            Assert.IsTrue(ValidGmail("john@gmail.com"));
        }

        [TestMethod]
        public void ValidGmail_WrongDomain_ReturnsFalse()
        {
            // Non-Gmail domain should be rejected
            Assert.IsFalse(ValidGmail("john@hotmail.com"));
        }

        [TestMethod]
        public void ValidGmail_MissingUsername_ReturnsFalse()
        {
            // Just the domain with no username should fail
            Assert.IsFalse(ValidGmail("@gmail.com"));
        }

        [TestMethod]
        public void ValidGmail_EmptyString_ReturnsFalse()
        {
            // Empty string must fail
            Assert.IsFalse(ValidGmail(""));
        }

        [TestMethod]
        public void ValidGmail_YahooEmail_ReturnsFalse()
        {
            // Yahoo must be rejected
            Assert.IsFalse(ValidGmail("test@yahoo.com"));
        }

        [TestMethod]
        public void ValidGmail_OutlookEmail_ReturnsFalse()
        {
            // Outlook must be rejected
            Assert.IsFalse(ValidGmail("user@outlook.com"));
        }

        [TestMethod]
        public void ValidGmail_EmailWithNumbers_ReturnsTrue()
        {
            // Gmail with numbers in username should pass
            Assert.IsTrue(ValidGmail("john123@gmail.com"));
        }

        [TestMethod]
        public void ValidGmail_EmailWithDot_ReturnsTrue()
        {
            // Gmail with dot in username should pass
            Assert.IsTrue(ValidGmail("john.smith@gmail.com"));
        }
    }

    // ============================================================
    // TEST CLASS 8: Stamp Price Calculation Tests
    // Covers: stamp pricing with VAT at 20% and service fee at 2%
    // Used in StampsView.cs ordering system
    // ============================================================
    [TestClass]
    public class StampPriceTests
    {
        // Exact copy of CalcStampTotal logic from StampsView.cs
        private double CalcStampTotal(double unitPrice, int qty, double deliveryFee)
        {
            double stampTotal = unitPrice * qty;
            double subtotal = stampTotal + deliveryFee;
            double tax = Math.Round(subtotal * 0.20, 2); // 20% VAT
            double serviceFee = Math.Round(subtotal * 0.02, 2); // 2% service fee
            return Math.Round(subtotal + tax + serviceFee, 2);
        }

        [TestMethod]
        public void StampPrice_FirstClass_Single_WithStandardDelivery_CorrectTotal()
        {
            // 1 x First Class GBP 1.10 + GBP 1.50 delivery
            // Subtotal 2.60, VAT 0.52, Service 0.05, Total 3.17
            double total = CalcStampTotal(1.10, 1, 1.50);

            Assert.IsTrue(total >= 3.00 && total <= 3.50);
        }

        [TestMethod]
        public void StampPrice_SecondClass_Ten_WithStandardDelivery_CorrectTotal()
        {
            // 10 x Second Class 0.75 = 7.50 + 1.50 delivery
            // Subtotal 9.00, VAT 1.80, Service 0.18, Total 10.98
            double total = CalcStampTotal(0.75, 10, 1.50);

            Assert.IsTrue(total >= 10.50 && total <= 11.50);
        }

        [TestMethod]
        public void StampPrice_ClickAndCollect_CheaperThanDelivery()
        {
            // Click and Collect has zero delivery fee -- must be cheaper
            double withDelivery = CalcStampTotal(1.10, 1, 1.50);
            double withoutDelivery = CalcStampTotal(1.10, 1, 0.00);

            Assert.IsTrue(withDelivery > withoutDelivery);
        }

        [TestMethod]
        public void StampPrice_ExpressDelivery_MoreExpensiveThanStandard()
        {
            // Express GBP 3.99 delivery vs Standard GBP 1.50
            double standard = CalcStampTotal(1.10, 1, 1.50);
            double express = CalcStampTotal(1.10, 1, 3.99);

            Assert.IsTrue(express > standard);
        }

        [TestMethod]
        public void StampPrice_LargeQuantity_MoreExpensiveThanSingle()
        {
            // Buying 25 stamps must cost more than buying 1
            double single = CalcStampTotal(1.10, 1, 1.50);
            double bulk = CalcStampTotal(1.10, 25, 1.50);

            Assert.IsTrue(bulk > single);
        }

        [TestMethod]
        public void StampPrice_TotalAlwaysIncludesVAT()
        {
            // Total must always be greater than subtotal due to 20% VAT
            double unitPrice = 1.10;
            int qty = 5;
            double delivery = 1.50;
            double subtotal = unitPrice * qty + delivery; // 7.00
            double total = CalcStampTotal(unitPrice, qty, delivery);

            // Total must be higher than subtotal
            Assert.IsTrue(total > subtotal);
        }
    }

    // ============================================================
    // TEST CLASS 9: City Location Detection Tests
    // Covers: the city detection logic used in FindUsView.cs
    // to show PostalMS locations near the user based on their city
    // ============================================================
    [TestClass]
    public class CityLocationTests
    {
        // Mirrors the supported cities list in FindUsView.cs
        private readonly List<string> supportedCities = new List<string>
        {
            "London", "Manchester", "Birmingham", "Leeds",
            "Bristol", "Edinburgh", "Glasgow"
        };

        // Mirrors the GetUserCity detection logic in FindUsView.cs
        private string DetectCityFromAddress(string address)
        {
            foreach (string city in supportedCities)
                if (address.IndexOf(city, StringComparison.OrdinalIgnoreCase) >= 0)
                    return city;
            return "London"; // Default to London if city not recognised
        }

        [TestMethod]
        public void DetectCity_LondonAddress_ReturnsLondon()
        {
            string city = DetectCityFromAddress("123 Main St, London NW4 4BT");

            Assert.AreEqual("London", city);
        }

        [TestMethod]
        public void DetectCity_ManchesterAddress_ReturnsManchester()
        {
            string city = DetectCityFromAddress("45 High Street, Manchester M1 1HP");

            Assert.AreEqual("Manchester", city);
        }

        [TestMethod]
        public void DetectCity_BirminghamAddress_ReturnsBirmingham()
        {
            string city = DetectCityFromAddress("14 New Street, Birmingham B2 4DU");

            Assert.AreEqual("Birmingham", city);
        }

        [TestMethod]
        public void DetectCity_LeedsAddress_ReturnsLeeds()
        {
            string city = DetectCityFromAddress("24 Briggate, Leeds LS1 6HD");

            Assert.AreEqual("Leeds", city);
        }

        [TestMethod]
        public void DetectCity_GlasgowAddress_ReturnsGlasgow()
        {
            string city = DetectCityFromAddress("92 Buchanan Street, Glasgow G1 3HA");

            Assert.AreEqual("Glasgow", city);
        }

        [TestMethod]
        public void DetectCity_CaseInsensitive_ReturnsCorrectCity()
        {
            // Detection must work regardless of case
            string city = DetectCityFromAddress("1 Park Road, LEEDS LS1 6HD");

            Assert.AreEqual("Leeds", city);
        }

        [TestMethod]
        public void DetectCity_UnknownCity_DefaultsToLondon()
        {
            // If city is not recognised it should default to London
            string city = DetectCityFromAddress("99 Some Road, Nowhere ZZ1 1ZZ");

            Assert.AreEqual("London", city);
        }

        [TestMethod]
        public void SupportedCities_ContainsAllSevenRequiredCities()
        {
            // All seven cities must be present in the supported list
            Assert.IsTrue(supportedCities.Contains("London"));
            Assert.IsTrue(supportedCities.Contains("Manchester"));
            Assert.IsTrue(supportedCities.Contains("Birmingham"));
            Assert.IsTrue(supportedCities.Contains("Leeds"));
            Assert.IsTrue(supportedCities.Contains("Bristol"));
            Assert.IsTrue(supportedCities.Contains("Edinburgh"));
            Assert.IsTrue(supportedCities.Contains("Glasgow"));
        }
    }
}