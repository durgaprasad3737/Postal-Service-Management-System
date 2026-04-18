// CustomBST.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// References:
// Cormen, T.H. et al. (2022) Introduction to Algorithms. 4th edn. MIT Press.
// Sedgewick, R. and Wayne, K. (2011) Algorithms. 4th edn. Addison-Wesley.
//   -- BST recursive insert, search and delete (Chapter 3)
//   -- In-order traversal for sorted output (Section 3.2)
//   -- In-order successor for delete with two children (Section 3.2)
//
// Custom Binary Search Tree implementation.
// Built from scratch without using any standard template library collections.
// Stores parcel tracking IDs in sorted order for efficient sorted display.
//
// BST Property: all keys in the left subtree are smaller than the node key,
// all keys in the right subtree are larger than the node key.
//
// Time Complexity:
//   Insert   - O(log n) average, O(n) worst (degenerate tree)
//   Search   - O(log n) average, O(n) worst
//   Delete   - O(log n) average, O(n) worst
//   InOrder  - O(n) always (must visit every node)
//   FindMin  - O(log n) average, O(n) worst

using System;
using System.Collections.Generic;

namespace PostalServiceWinForms.DataStructures
{
    // Represents a single node in the Binary Search Tree
    public class BSTNode
    {
        public string Key;   // Tracking ID e.g. PS-2026-00001
        public object Value; // Parcel status e.g. Pending
        public BSTNode Left;  // Left child - contains smaller keys
        public BSTNode Right; // Right child - contains larger keys

        public BSTNode(string k, object v)
        {
            Key = k;
            Value = v;
        }
    }

    // Custom Binary Search Tree
    // Used to store parcels in sorted order by tracking ID
    // In-order traversal produces a sorted list for the dashboard
    public class CustomBST
    {
        private BSTNode _root;  // Root node of the tree
        private int _count; // Number of nodes in the tree

        // Insert a new key-value pair into the BST
        // If the key already exists, its value is updated
        // Time complexity: O(log n) average
        public void Insert(string key, object value)
        {
            _root = InsertRecursive(_root, key, value);
        }

        // Recursive helper for Insert
        // Navigates left for smaller keys, right for larger keys
        private BSTNode InsertRecursive(BSTNode node, string key, object value)
        {
            // Empty position found - create the new node here
            if (node == null)
            {
                _count++;
                return new BSTNode(key, value);
            }

            int comparison = string.Compare(key, node.Key);

            if (comparison < 0)
                // Key is smaller - go left
                node.Left = InsertRecursive(node.Left, key, value);
            else if (comparison > 0)
                // Key is larger - go right
                node.Right = InsertRecursive(node.Right, key, value);
            else
                // Key already exists - update value, no duplicate
                node.Value = value;

            return node;
        }

        // Search for a key and return its value
        // Returns null if the key does not exist
        // Time complexity: O(log n) average
        public object Search(string key)
        {
            BSTNode result = SearchRecursive(_root, key);
            return result?.Value;
        }

        // Recursive helper for Search
        private BSTNode SearchRecursive(BSTNode node, string key)
        {
            // Reached end of tree without finding key
            if (node == null)
                return null;

            int comparison = string.Compare(key, node.Key);

            if (comparison < 0)
                return SearchRecursive(node.Left, key);  // Go left
            else if (comparison > 0)
                return SearchRecursive(node.Right, key); // Go right
            else
                return node; // Found
        }

        // Returns true if the key exists in the BST
        public bool Contains(string key) => Search(key) != null;

        // Delete a node from the BST by key
        // Returns true if deleted, false if key not found
        // Handles three cases: leaf node, one child, two children
        // Time complexity: O(log n) average
        public bool Delete(string key)
        {
            if (!Contains(key))
                return false;

            _root = DeleteRecursive(_root, key);
            _count--;
            return true;
        }

        // Recursive helper for Delete
        // Three cases:
        //   Case 1: Node has no children - simply remove it
        //   Case 2: Node has one child - replace with that child
        //   Case 3: Node has two children - replace with in-order successor
        private BSTNode DeleteRecursive(BSTNode node, string key)
        {
            if (node == null)
                return null;

            int comparison = string.Compare(key, node.Key);

            if (comparison < 0)
                node.Left = DeleteRecursive(node.Left, key);
            else if (comparison > 0)
                node.Right = DeleteRecursive(node.Right, key);
            else
            {
                // Found the node to delete

                // Case 1 and 2: No left child - replace with right child
                if (node.Left == null)
                    return node.Right;

                // Case 2: No right child - replace with left child
                if (node.Right == null)
                    return node.Left;

                // Case 3: Two children
                // Find in-order successor (smallest node in right subtree)
                BSTNode successor = FindMinNode(node.Right);

                // Copy successor data into current node
                node.Key = successor.Key;
                node.Value = successor.Value;

                // Delete the successor from the right subtree
                node.Right = DeleteRecursive(node.Right, successor.Key);
            }

            return node;
        }

        // Find the node with the smallest key in the BST
        // The smallest key is always in the leftmost node
        // Time complexity: O(log n) average
        public BSTNode FindMin() => FindMinNode(_root);

        // Helper to find the minimum node starting from a given node
        private BSTNode FindMinNode(BSTNode node)
        {
            // Follow left child pointers until there is no left child
            while (node?.Left != null)
                node = node.Left;
            return node;
        }

        // In-order traversal - returns all nodes sorted by key (ascending)
        // Left subtree first, then current node, then right subtree
        // Used to display the sorted parcel dashboard
        // Time complexity: O(n) - must visit every node
        public List<KeyValuePair<string, object>> InOrder()
        {
            var result = new List<KeyValuePair<string, object>>();
            InOrderRecursive(_root, result);
            return result;
        }

        // Recursive helper for InOrder traversal
        private void InOrderRecursive(BSTNode node, List<KeyValuePair<string, object>> result)
        {
            if (node == null)
                return;

            // Visit left subtree first (smaller keys)
            InOrderRecursive(node.Left, result);

            // Visit current node
            result.Add(new KeyValuePair<string, object>(node.Key, node.Value));

            // Visit right subtree last (larger keys)
            InOrderRecursive(node.Right, result);
        }

        // Number of nodes currently in the tree
        public int Count => _count;

        // Returns true if the tree has no nodes
        public bool IsEmpty => _count == 0;

        // Remove all nodes from the tree
        // Setting root to null makes all nodes unreachable for garbage collection
        // Time complexity: O(1)
        public void Clear()
        {
            _root = null;
            _count = 0;
        }
    }
}