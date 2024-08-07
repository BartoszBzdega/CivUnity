using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Priority_Queue
{

    public sealed class FastPriorityQueue<T> : IFixedSizePriorityQueue<T, float>
        where T : FastPriorityQueueNode
    {
        private int _numNodes;
        private T[] _nodes;


        public FastPriorityQueue(int maxNodes)
        {
#if DEBUG
            if (maxNodes <= 0)
            {
                throw new InvalidOperationException("rozmiar nowej kolejki nie moze byc mniejszy niz 1");
            }
#endif

            _numNodes = 0;
            _nodes = new T[maxNodes + 1];
        }


        public int Count
        {
            get
            {
                return _numNodes;
            }
        }


        public int MaxSize
        {
            get
            {
                return _nodes.Length - 1;
            }
        }


#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Clear()
        {
            Array.Clear(_nodes, 1, _numNodes);
            _numNodes = 0;
        }


#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(T node)
        {
#if DEBUG
            if(node == null)
            {
                throw new ArgumentNullException("wezel");
            }
            if(node.QueueIndex < 0 || node.QueueIndex >= _nodes.Length)
            {
                throw new InvalidOperationException("Indeks kolejki jest bledny");
            }
#endif

            return (_nodes[node.QueueIndex] == node);
        }


#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Enqueue(T node, float priority)
        {
#if DEBUG
            if(node == null)
            {
                throw new ArgumentNullException("wezel");
            }
            if(_numNodes >= _nodes.Length - 1)
            {
                throw new InvalidOperationException("kolejka jest pelna: " + node);
            }
            if(Contains(node))
            {
                throw new InvalidOperationException("wezel jest juz w kolejce: " + node);
            }
#endif

            node.Priority = priority;
            _numNodes++;
            _nodes[_numNodes] = node;
            node.QueueIndex = _numNodes;
            CascadeUp(node);
        }


        private void CascadeUp(T node)
        {

            int parent;
            if(node.QueueIndex > 1)
            {
                parent = node.QueueIndex >> 1;
                T parentNode = _nodes[parent];
                if(HasHigherOrEqualPriority(parentNode, node))
                    return;


                _nodes[node.QueueIndex] = parentNode;
                parentNode.QueueIndex = node.QueueIndex;

                node.QueueIndex = parent;
            }
            else
            {
                return;
            }
            while(parent > 1)
            {
                parent >>= 1;
                T parentNode = _nodes[parent];
                if(HasHigherOrEqualPriority(parentNode, node))
                    break;


                _nodes[node.QueueIndex] = parentNode;
                parentNode.QueueIndex = node.QueueIndex;

                node.QueueIndex = parent;
            }
            _nodes[node.QueueIndex] = node;
        }

#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void CascadeDown(T node)
        {
            //aka Heapify-down
            int finalQueueIndex = node.QueueIndex;
            int childLeftIndex = 2 * finalQueueIndex;


            if(childLeftIndex > _numNodes)
            {
                return;
            }


            int childRightIndex = childLeftIndex + 1;
            T childLeft = _nodes[childLeftIndex];
            if(HasHigherPriority(childLeft, node))
            {

                if(childRightIndex > _numNodes)
                {
                    node.QueueIndex = childLeftIndex;
                    childLeft.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childLeft;
                    _nodes[childLeftIndex] = node;
                    return;
                }

                T childRight = _nodes[childRightIndex];
                if(HasHigherPriority(childLeft, childRight))
                {

                    childLeft.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childLeft;
                    finalQueueIndex = childLeftIndex;
                }
                else
                {

                    childRight.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
            }

            else if(childRightIndex > _numNodes)
            {
                return;
            }
            else
            {

                T childRight = _nodes[childRightIndex];
                if(HasHigherPriority(childRight, node))
                {
                    childRight.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }

                else
                {
                    return;
                }
            }

            while(true)
            {
                childLeftIndex = 2 * finalQueueIndex;


                if(childLeftIndex > _numNodes)
                {
                    node.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = node;
                    break;
                }


                childRightIndex = childLeftIndex + 1;
                childLeft = _nodes[childLeftIndex];
                if(HasHigherPriority(childLeft, node))
                {

                    if(childRightIndex > _numNodes)
                    {
                        node.QueueIndex = childLeftIndex;
                        childLeft.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = childLeft;
                        _nodes[childLeftIndex] = node;
                        break;
                    }

                    T childRight = _nodes[childRightIndex];
                    if(HasHigherPriority(childLeft, childRight))
                    {

                        childLeft.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = childLeft;
                        finalQueueIndex = childLeftIndex;
                    }
                    else
                    {

                        childRight.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                }

                else if(childRightIndex > _numNodes)
                {
                    node.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = node;
                    break;
                }
                else
                {

                    T childRight = _nodes[childRightIndex];
                    if(HasHigherPriority(childRight, node))
                    {
                        childRight.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }

                    else
                    {
                        node.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = node;
                        break;
                    }
                }
            }
        }


#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool HasHigherPriority(T higher, T lower)
        {
            return (higher.Priority < lower.Priority);
        }


#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool HasHigherOrEqualPriority(T higher, T lower)
        {
            return (higher.Priority <= lower.Priority);
        }

#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public T Dequeue()
        {
#if DEBUG
            if(_numNodes <= 0)
            {
                throw new InvalidOperationException("nie mozna usunac nic z pustej kolejki");
            }

            if(!IsValidQueue())
            {
                throw new InvalidOperationException("Cos z kolejka jest nie tak");
            }
#endif

            T returnMe = _nodes[1];

            if(_numNodes == 1)
            {
                _nodes[1] = null;
                _numNodes = 0;
                return returnMe;
            }

            T formerLastNode = _nodes[_numNodes];
            _nodes[1] = formerLastNode;
            formerLastNode.QueueIndex = 1;
            _nodes[_numNodes] = null;
            _numNodes--;


            CascadeDown(formerLastNode);
            return returnMe;
        }


        public void Resize(int maxNodes)
        {
#if DEBUG
            if (maxNodes <= 0)
            {
                throw new InvalidOperationException("rozmiar kolejki nie moze byc mniejszy niz 1");
            }

            if (maxNodes < _numNodes)
            {
                throw new InvalidOperationException("Probowano zmienic rozmiar(" + maxNodes + "), obecna kolejka zawiera " + _numNodes + " wezlow");
            }
#endif

            T[] newArray = new T[maxNodes + 1];
            int highestIndexToCopy = Math.Min(maxNodes, _numNodes);
            Array.Copy(_nodes, newArray, highestIndexToCopy + 1);
            _nodes = newArray;
        }


        public T First
        {
            get
            {
#if DEBUG
                if(_numNodes <= 0)
                {
                    throw new InvalidOperationException("nie mozna tego uzyc na pustej kolejce");
                }
#endif

                return _nodes[1];
            }
        }


#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void UpdatePriority(T node, float priority)
        {
#if DEBUG
            if(node == null)
            {
                throw new ArgumentNullException("wezel");
            }
            if(!Contains(node))
            {
                throw new InvalidOperationException("nie mozna zupdatowac wezla ktorego nie ma w kolejce: " + node);
            }
#endif

            node.Priority = priority;
            OnNodeUpdated(node);
        }

#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void OnNodeUpdated(T node)
        {

            int parentIndex = node.QueueIndex >> 1;

            if(parentIndex > 0 && HasHigherPriority(node, _nodes[parentIndex]))
            {
                CascadeUp(node);
            }
            else
            {

                CascadeDown(node);
            }
        }


#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Remove(T node)
        {
#if DEBUG
            if(node == null)
            {
                throw new ArgumentNullException("wezel");
            }
            if(!Contains(node))
            {
                throw new InvalidOperationException("nie mozna usunac wezla ktory nie jest w kolejce: " + node);
            }
#endif

            if(node.QueueIndex == _numNodes)
            {
                _nodes[_numNodes] = null;
                _numNodes--;
                return;
            }


            T formerLastNode = _nodes[_numNodes];
            _nodes[node.QueueIndex] = formerLastNode;
            formerLastNode.QueueIndex = node.QueueIndex;
            _nodes[_numNodes] = null;
            _numNodes--;

            OnNodeUpdated(formerLastNode);
        }

        public IEnumerator<T> GetEnumerator()
        {
#if NET_VERSION_4_5
            IEnumerable<T> e = new ArraySegment<T>(_nodes, 1, _numNodes);
            return e.GetEnumerator();
#else
            for(int i = 1; i <= _numNodes; i++)
                yield return _nodes[i];
#endif
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public bool IsValidQueue()
        {
            for(int i = 1; i < _nodes.Length; i++)
            {
                if(_nodes[i] != null)
                {
                    int childLeftIndex = 2 * i;
                    if(childLeftIndex < _nodes.Length && _nodes[childLeftIndex] != null && HasHigherPriority(_nodes[childLeftIndex], _nodes[i]))
                        return false;

                    int childRightIndex = childLeftIndex + 1;
                    if(childRightIndex < _nodes.Length && _nodes[childRightIndex] != null && HasHigherPriority(_nodes[childRightIndex], _nodes[i]))
                        return false;
                }
            }
            return true;
        }
    }
}