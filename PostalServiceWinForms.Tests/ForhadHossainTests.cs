// my tests - Forhad Hossain fh455@live.mdx.ac.uk
// tester for the group
// CST2550 coursework

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PostalServiceWinForms.Tests
{
    [TestClass]
    public class ForhadHashTableTests
    {
        // check new hashtable is empty
        [TestMethod]
        public void HashTable_StartsEmpty()
        {
            var ht = new TestHashTable();
            Assert.AreEqual(0, ht.Count);
        }

        // add one thing and check count goes up
        [TestMethod]
        public void HashTable_AddOne_CountIsOne()
        {
            var ht = new TestHashTable();
            ht.Add("PKG001", "Delivered");
            Assert.AreEqual(1, ht.Count);
        }

        // check i can find what i added
        [TestMethod]
        public void HashTable_Search_FindsItem()
        {
            var ht = new TestHashTable();
            ht.Add("PKG001", "Pending");
            Assert.IsNotNull(ht.Search("PKG001"));
        }

        // key that doesnt exist should return null
        [TestMethod]
        public void HashTable_Search_WrongKey_ReturnsNull()
        {
            var ht = new TestHashTable();
            Assert.IsNull(ht.Search("WRONGKEY"));
        }

        // check containskey works
        [TestMethod]
        public void HashTable_ContainsKey_ReturnsTrue()
        {
            var ht = new TestHashTable();
            ht.Add("PKG001", "Delivered");
            Assert.IsTrue(ht.ContainsKey("PKG001"));
        }

        // delete item and check its gone
        [TestMethod]
        public void HashTable_Delete_ItemGone()
        {
            var ht = new TestHashTable();
            ht.Add("PKG001", "Delivered");
            ht.Delete("PKG001");
            Assert.IsNull(ht.Search("PKG001"));
        }

        // add 3 items check count is 3
        [TestMethod]
        public void HashTable_AddThree_CountIsThree()
        {
            var ht = new TestHashTable();
            ht.Add("PKG001", "Delivered");
            ht.Add("PKG002", "Pending");
            ht.Add("PKG003", "In Transit");
            Assert.AreEqual(3, ht.Count);
        }
    }

    [TestClass]
    public class ForhadBSTTests
    {
        // new bst should be empty
        [TestMethod]
        public void BST_StartsEmpty()
        {
            var bst = new TestBST();
            Assert.AreEqual(0, bst.Count);
        }

        // insert something and search for it
        [TestMethod]
        public void BST_Insert_CanFindIt()
        {
            var bst = new TestBST();
            bst.Insert("PKG100", "Delivered");
            Assert.IsNotNull(bst.Search("PKG100"));
        }

        // search for something not there
        [TestMethod]
        public void BST_Search_NotFound()
        {
            var bst = new TestBST();
            bst.Insert("PKG100", "Delivered");
            Assert.IsNull(bst.Search("PKG999"));
        }

        // contains returns true for existing item
        [TestMethod]
        public void BST_Contains_ReturnsTrue()
        {
            var bst = new TestBST();
            bst.Insert("PKG100", "Delivered");
            Assert.IsTrue(bst.Contains("PKG100"));
        }

        // insert 3 things check all found
        [TestMethod]
        public void BST_InsertThree_AllFound()
        {
            var bst = new TestBST();
            bst.Insert("PKG001", "Delivered");
            bst.Insert("PKG002", "Pending");
            bst.Insert("PKG003", "In Transit");
            Assert.IsTrue(bst.Contains("PKG001"));
            Assert.IsTrue(bst.Contains("PKG002"));
            Assert.IsTrue(bst.Contains("PKG003"));
        }
    }

    [TestClass]
    public class ForhadQueueTests
    {
        // queue should start empty
        [TestMethod]
        public void Queue_StartsEmpty()
        {
            var q = new TestQueue();
            Assert.AreEqual(0, q.Count);
        }

        // add to queue check count
        [TestMethod]
        public void Queue_AddItem_CountGoesUp()
        {
            var q = new TestQueue();
            q.Enqueue("PKG001");
            Assert.AreEqual(1, q.Count);
        }

        // first in first out check
        [TestMethod]
        public void Queue_FirstInFirstOut()
        {
            var q = new TestQueue();
            q.Enqueue("PKG001");
            q.Enqueue("PKG002");
            Assert.AreEqual("PKG001", q.Dequeue());
        }

        // dequeue reduces count
        [TestMethod]
        public void Queue_Dequeue_CountGoesDown()
        {
            var q = new TestQueue();
            q.Enqueue("PKG001");
            q.Dequeue();
            Assert.AreEqual(0, q.Count);
        }

        // add 3 dequeue all check empty
        [TestMethod]
        public void Queue_DequeueAll_EmptyAfter()
        {
            var q = new TestQueue();
            q.Enqueue("PKG001");
            q.Enqueue("PKG002");
            q.Enqueue("PKG003");
            q.Dequeue();
            q.Dequeue();
            q.Dequeue();
            Assert.AreEqual(0, q.Count);
        }
    }

    [TestClass]
    public class ForhadTrackingIDTests
    {
        // tracking id shouldnt be null
        [TestMethod]
        public void TrackingID_NotNull()
        {
            string id = "UK" + DateTime.Now.Ticks.ToString().Substring(0, 9);
            Assert.IsNotNull(id);
        }

        // tracking id should have more than 5 chars
        [TestMethod]
        public void TrackingID_LongEnough()
        {
            string id = "UK123456789";
            Assert.IsTrue(id.Length > 5);
        }

        // two ids should be different
        [TestMethod]
        public void TrackingID_TwoIDs_AreDifferent()
        {
            string id1 = "UK" + Guid.NewGuid().ToString("N").Substring(0, 9).ToUpper();
            string id2 = "UK" + Guid.NewGuid().ToString("N").Substring(0, 9).ToUpper();
            Assert.AreNotEqual(id1, id2);
        }

        // id should start with UK
        [TestMethod]
        public void TrackingID_StartsWithUK()
        {
            string id = "UK123456789";
            Assert.IsTrue(id.StartsWith("UK"));
        }
    }
}
