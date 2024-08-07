using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Pathing.Path;

namespace Pathing
{
    public class Astar<T> where T:Tile
    {
        public Astar(WorldPath world, PathableUnit unit, T StartTile, T EndTile, CostEstimateDelegate costEstimateFunc)
        {
           this.world = world;
           this.unit = unit;
           this.StartTile = StartTile;
           this.EndTile = EndTile;
           this.costEstimateFunc= costEstimateFunc;
        }
        WorldPath world;
        PathableUnit unit;
        T StartTile;
        T EndTile;
        CostEstimateDelegate costEstimateFunc;
        Queue<T> path;
        public void DoWork()
        {
            path = new Queue<T>();
            HashSet<T> ClosedSet = new HashSet<T>();
            PathfindingPriorityQueue<T> OpenSet = new PathfindingPriorityQueue<T>();
            OpenSet.Enqueue(StartTile, 0);
            Dictionary<T,T> cameFrom = new Dictionary<T, T>();
            Dictionary<T,float> g_score = new Dictionary<T,float>(); //prawdziwy koszt przejscia od a do b
            g_score[StartTile] = 0;
            Dictionary<T,float> f_score = new Dictionary<T,float>();//przewidywany koszt przejscia od a do b
            f_score[StartTile] = costEstimateFunc(StartTile, EndTile);

            while (OpenSet.Count > 0)
            {
                T current = OpenSet.Dequeue();
                if (System.Object.ReferenceEquals(current,EndTile))
                {
                    Reconstruct_path(cameFrom, current);
                    return;
                }
                ClosedSet.Add(current);
                foreach(T edge_neighbuor in current.GetNeighbours())
                {
                    T neighbour = edge_neighbuor;
                    if (ClosedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    float total_pathfinding_cost_to_neighbour =neighbour.AggregateCostToEnter(g_score[current], current, unit);

                    if (total_pathfinding_cost_to_neighbour < 0)
                    {
                        continue;
                    }

                    float tentative_g_score = total_pathfinding_cost_to_neighbour;
                    if (OpenSet.Contains(neighbour) && tentative_g_score >= g_score[neighbour])
                    {
                        continue;
                    }
                    cameFrom[neighbour] = current;
                    g_score[neighbour]=tentative_g_score;
                    f_score[neighbour] = g_score[neighbour] + costEstimateFunc(neighbour,EndTile);
                    OpenSet.EnqueueOrUpdate(neighbour, f_score[neighbour]);
                }
            }


        }

        private void Reconstruct_path(Dictionary<T,T> came_from, T current)
        {
            Queue<T> Total_path = new Queue<T>();
            Total_path.Enqueue(current);

            while (came_from.ContainsKey(current))
            {
                current = came_from[current];
                Total_path.Enqueue(current);
            }
            path = new Queue<T>(Total_path.Reverse());
        }

        float costEstimate(T source, T destination) 
        {
            return 0;
        }

        public T[] GetList()
        {
            return path.ToArray();
        }
    }
}
