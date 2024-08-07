

using UnityEngine;
using System.Collections.Generic;
using Priority_Queue;


public class PathfindingPriorityQueue<T>
{

    protected FastPriorityQueue<WrappedNode> underlyingQueue;

    protected Dictionary<T, WrappedNode> mapDataToWrappedNode;

    public PathfindingPriorityQueue(int startingSize = 10)
    {
        underlyingQueue = new FastPriorityQueue<WrappedNode>(startingSize);
        mapDataToWrappedNode = new Dictionary<T, WrappedNode>();
    }

    public int Count
    {
        get
        {
            return underlyingQueue.Count;
        }
    }


    public bool Contains(T data)
    {
        return mapDataToWrappedNode.ContainsKey(data);
    }


    public void Enqueue(T data, float priority)
    {
        if (mapDataToWrappedNode.ContainsKey(data))
        {
            Debug.LogError("Priority Queue can't re-enqueue a node that's already enqueued.");
            return;
        }

        if (underlyingQueue.Count == underlyingQueue.MaxSize)
        {
            underlyingQueue.Resize((2 * underlyingQueue.MaxSize) + 1);
        }

        WrappedNode newNode = new WrappedNode(data);
        underlyingQueue.Enqueue(newNode, priority);
        mapDataToWrappedNode[data] = newNode;
    }

    public void UpdatePriority(T data, float priority)
    {
        WrappedNode node = mapDataToWrappedNode[data];
        underlyingQueue.UpdatePriority(node, priority);
    }

    public void EnqueueOrUpdate(T data, float priority)
    {
        if (mapDataToWrappedNode.ContainsKey(data))
        {
            UpdatePriority(data, priority);
        }
        else
        {
            Enqueue(data, priority);
        }
    }

    public T Dequeue()
    {
        WrappedNode popped = underlyingQueue.Dequeue();
        mapDataToWrappedNode.Remove(popped.Data);
        return popped.Data;
    }

    protected class WrappedNode : FastPriorityQueueNode
    {
        public readonly T Data;

        public WrappedNode(T data)
        {
            this.Data = data;
        }
    }
}