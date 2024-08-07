using System;
using System.Collections;
using System.Collections.Generic;

namespace Priority_Queue
{

    public class SimplePriorityQueue<TItem, TPriority> : IPriorityQueue<TItem, TPriority>
        where TPriority : IComparable<TPriority>
    {
        private class SimpleNode : GenericPriorityQueueNode<TPriority>
        {
            public TItem Data { get; private set; }

            public SimpleNode(TItem data)
            {
                Data = data;
            }
        }

        private const int INITIAL_QUEUE_SIZE = 10;
        private readonly GenericPriorityQueue<SimpleNode, TPriority> _queue;
        private readonly Dictionary<TItem, IList<SimpleNode>> _itemToNodesCache;
        private readonly IList<SimpleNode> _nullNodesCache;


        public SimplePriorityQueue() : this(Comparer<TPriority>.Default) { }


        public SimplePriorityQueue(IComparer<TPriority> comparer) : this(comparer.Compare) { }


        public SimplePriorityQueue(Comparison<TPriority> comparer)
        {
            _queue = new GenericPriorityQueue<SimpleNode, TPriority>(INITIAL_QUEUE_SIZE, comparer);
            _itemToNodesCache = new Dictionary<TItem, IList<SimpleNode>>();
            _nullNodesCache = new List<SimpleNode>();
        }


        private SimpleNode GetExistingNode(TItem item)
        {
            if (item == null)
            {
                return _nullNodesCache.Count > 0 ? _nullNodesCache[0] : null;
            }

            IList<SimpleNode> nodes;
            if (!_itemToNodesCache.TryGetValue(item, out nodes))
            {
                return null;
            }
            return nodes[0];
        }

        private void AddToNodeCache(SimpleNode node)
        {
            if (node.Data == null)
            {
                _nullNodesCache.Add(node);
                return;
            }

            IList<SimpleNode> nodes;
            if (!_itemToNodesCache.TryGetValue(node.Data, out nodes))
            {
                nodes = new List<SimpleNode>();
                _itemToNodesCache[node.Data] = nodes;
            }
            nodes.Add(node);
        }

        private void RemoveFromNodeCache(SimpleNode node)
        {
            if (node.Data == null)
            {
                _nullNodesCache.Remove(node);
                return;
            }

            IList<SimpleNode> nodes;
            if (!_itemToNodesCache.TryGetValue(node.Data, out nodes))
            {
                return;
            }
            nodes.Remove(node);
            if (nodes.Count == 0)
            {
                _itemToNodesCache.Remove(node.Data);
            }
        }


        public int Count
        {
            get
            {
                lock(_queue)
                {
                    return _queue.Count;
                }
            }
        }



        public TItem First
        {
            get
            {
                lock(_queue)
                {
                    if(_queue.Count <= 0)
                    {
                        throw new InvalidOperationException("Cannot call .First on an empty queue");
                    }

                    return _queue.First.Data;
                }
            }
        }


        public void Clear()
        {
            lock(_queue)
            {
                _queue.Clear();
                _itemToNodesCache.Clear();
                _nullNodesCache.Clear();
            }
        }


        public bool Contains(TItem item)
        {
            lock(_queue)
            {
                if (item == null)
                {
                    return _nullNodesCache.Count > 0;
                }
                return _itemToNodesCache.ContainsKey(item);
            }
        }


        public TItem Dequeue()
        {
            lock(_queue)
            {
                if(_queue.Count <= 0)
                {
                    throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
                }

                SimpleNode node =_queue.Dequeue();
                RemoveFromNodeCache(node);
                return node.Data;
            }
        }


        private SimpleNode EnqueueNoLockOrCache(TItem item, TPriority priority)
        {
            SimpleNode node = new SimpleNode(item);
            if (_queue.Count == _queue.MaxSize)
            {
                _queue.Resize(_queue.MaxSize * 2 + 1);
            }
            _queue.Enqueue(node, priority);
            return node;
        }


        public void Enqueue(TItem item, TPriority priority)
        {
            lock(_queue)
            {
                IList<SimpleNode> nodes;
                if (item == null)
                {
                    nodes = _nullNodesCache;
                }
                else if (!_itemToNodesCache.TryGetValue(item, out nodes))
                {
                    nodes = new List<SimpleNode>();
                    _itemToNodesCache[item] = nodes;
                }
                SimpleNode node = EnqueueNoLockOrCache(item, priority);
                nodes.Add(node);
            }
        }


        public bool EnqueueWithoutDuplicates(TItem item, TPriority priority)
        {
            lock(_queue)
            {
                IList<SimpleNode> nodes;
                if (item == null)
                {
                    if (_nullNodesCache.Count > 0)
                    {
                        return false;
                    }
                    nodes = _nullNodesCache;
                }
                else if (_itemToNodesCache.ContainsKey(item))
                {
                    return false;
                }
                else
                {
                    nodes = new List<SimpleNode>();
                    _itemToNodesCache[item] = nodes;
                }
                SimpleNode node = EnqueueNoLockOrCache(item, priority);
                nodes.Add(node);
                return true;
            }
        }


        public void Remove(TItem item)
        {
            lock(_queue)
            {
                SimpleNode removeMe;
                IList<SimpleNode> nodes;
                if (item == null)
                {
                    if (_nullNodesCache.Count == 0)
                    {
                        throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + item);
                    }
                    removeMe = _nullNodesCache[0];
                    nodes = _nullNodesCache;
                }
                else
                {
                    if (!_itemToNodesCache.TryGetValue(item, out nodes))
                    {
                        throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + item);
                    }
                    removeMe = nodes[0];
                    if (nodes.Count == 1)
                    {
                        _itemToNodesCache.Remove(item);
                    }
                }
                _queue.Remove(removeMe);
                nodes.Remove(removeMe);
            }
        }


        public void UpdatePriority(TItem item, TPriority priority)
        {
            lock (_queue)
            {
                SimpleNode updateMe = GetExistingNode(item);
                if (updateMe == null)
                {
                    throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " + item);
                }
                _queue.UpdatePriority(updateMe, priority);
            }
        }


        public TPriority GetPriority(TItem item)
        {
            lock (_queue)
            {
                SimpleNode findMe = GetExistingNode(item);
                if(findMe == null)
                {
                    throw new InvalidOperationException("Cannot call GetPriority() on a node which is not enqueued: " + item);
                }
                return findMe.Priority;
            }
        }

        #region Try* methods for multithreading
        /// Get the head of the queue, without removing it (use TryDequeue() for that).
        /// Useful for multi-threading, where the queue may become empty between calls to Contains() and First
        /// Returns true if successful, false otherwise
        /// O(1)
        public bool TryFirst(out TItem first)
        {
            lock(_queue)
            {
                if(_queue.Count <= 0)
                {
                    first = default(TItem);
                    return false;
                }

                first = _queue.First.Data;
                return true;
            }
        }

        /// <summary>
        /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and sets it to first.
        /// Useful for multi-threading, where the queue may become empty between calls to Contains() and Dequeue()
        /// Returns true if successful; false if queue was empty
        /// O(log n)
        /// </summary>
        public bool TryDequeue(out TItem first)
        {
            lock(_queue)
            {
                if(_queue.Count <= 0)
                {
                    first = default(TItem);
                    return false;
                }

                SimpleNode node = _queue.Dequeue();
                first = node.Data;
                RemoveFromNodeCache(node);
                return true;
            }
        }

        /// <summary>
        /// Attempts to remove an item from the queue.  The item does not need to be the head of the queue.  
        /// Useful for multi-threading, where the queue may become empty between calls to Contains() and Remove()
        /// Returns true if the item was successfully removed, false if it wasn't in the queue.
        /// If multiple copies of the item are enqueued, only the first one is removed. 
        /// O(log n)
        /// </summary>
        public bool TryRemove(TItem item)
        {
            lock(_queue)
            {
                SimpleNode removeMe;
                IList<SimpleNode> nodes;
                if (item == null)
                {
                    if (_nullNodesCache.Count == 0)
                    {
                        return false;
                    }
                    removeMe = _nullNodesCache[0];
                    nodes = _nullNodesCache;
                }
                else
                {
                    if (!_itemToNodesCache.TryGetValue(item, out nodes))
                    {
                        return false;
                    }
                    removeMe = nodes[0];
                    if (nodes.Count == 1)
                    {
                        _itemToNodesCache.Remove(item);
                    }
                }
                _queue.Remove(removeMe);
                nodes.Remove(removeMe);
                return true;
            }
        }

        /// <summary>
        /// Call this method to change the priority of an item.
        /// Useful for multi-threading, where the queue may become empty between calls to Contains() and UpdatePriority()
        /// If the item is enqueued multiple times, only the first one will be updated.
        /// (If your requirements are complex enough that you need to enqueue the same item multiple times <i>and</i> be able
        /// to update all of them, please wrap your items in a wrapper class so they can be distinguished).
        /// Returns true if the item priority was updated, false otherwise.
        /// O(log n)
        /// </summary>
        public bool TryUpdatePriority(TItem item, TPriority priority)
        {
            lock(_queue)
            {
                SimpleNode updateMe = GetExistingNode(item);
                if(updateMe == null)
                {
                    return false;
                }
                _queue.UpdatePriority(updateMe, priority);
                return true;
            }
        }

        /// <summary>
        /// Attempt to get the priority of the given item.
        /// Useful for multi-threading, where the queue may become empty between calls to Contains() and GetPriority()
        /// If the item is enqueued multiple times, only the priority of the first will be returned.
        /// (If your requirements are complex enough that you need to enqueue the same item multiple times <i>and</i> be able
        /// to query all their priorities, please wrap your items in a wrapper class so they can be distinguished).
        /// Returns true if the item was found in the queue, false otherwise
        /// O(1)
        /// </summary>
        public bool TryGetPriority(TItem item, out TPriority priority)
        {
            lock(_queue)
            {
                SimpleNode findMe = GetExistingNode(item);
                if(findMe == null)
                {
                    priority = default(TPriority);
                    return false;
                }
                priority = findMe.Priority;
                return true;
            }
        }
        #endregion

        public IEnumerator<TItem> GetEnumerator()
        {
            List<TItem> queueData = new List<TItem>();
            lock (_queue)
            {
                //Copy to a separate list because we don't want to 'yield return' inside a lock
                foreach(var node in _queue)
                {
                    queueData.Add(node.Data);
                }
            }

            return queueData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsValidQueue()
        {
            lock(_queue)
            {

                foreach (IList<SimpleNode> nodes in _itemToNodesCache.Values)
                {
                    foreach (SimpleNode node in nodes)
                    {
                        if (!_queue.Contains(node))
                        {
                            return false;
                        }
                    }
                }


                foreach (SimpleNode node in _queue)
                {
                    if (GetExistingNode(node.Data) == null)
                    {
                        return false;
                    }
                }


                return _queue.IsValidQueue();
            }
        }
    }


    public class SimplePriorityQueue<TItem> : SimplePriorityQueue<TItem, float>
    {

        public SimplePriorityQueue() { }


        public SimplePriorityQueue(IComparer<float> comparer) : base(comparer) { }


        public SimplePriorityQueue(Comparison<float> comparer) : base(comparer) { }
    }
}