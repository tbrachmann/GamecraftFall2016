using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class Movable
{

    protected TileMap tileMap;

    protected Movable(TileMap tileMap)
    {
        this.tileMap = tileMap;
    }

    protected LinkedList<TileCoords> FindShortestPath(TileCoords start, TileCoords goal)
    {
        //print("start: " + start + " " + "goal: " + goal);
        HashSet<TileCoords> closedSet = new HashSet<TileCoords>();
        PriorityQueue openSet = new PriorityQueue(new List<TileCoords>() { start });
        TileCoords[] neighbors = { TileCoords.right, TileCoords.forward, TileCoords.left, TileCoords.back };
        //Node -> the node that it can most efficiently be reached from
        Dictionary<TileCoords, TileCoords> cameFrom = new Dictionary<TileCoords, TileCoords> { };
        //Node -> "cost" from getting from start to this node
        //default value of infinity
        Dictionary<TileCoords, float> gScore = new Dictionary<TileCoords, float> { };
        //gScore.Add(start, 0);
        gScore[start] = 0;
        //Node -> the "score" of getting from start node to goal through this node
        //default value of infinity
        Dictionary<TileCoords, float> fScore = new Dictionary<TileCoords, float> { };
        fScore[start] = ManhattanHeuristic(start, goal);
        while (openSet.Count != 0)
        {
            //Aggregate - like python map? - was so proud of this but it turned out to be useless
            //Vector3 current = 
            // fScore.Aggregate((key, nextKey) => key.Value < nextKey.Value ? key : nextKey).Key;
            TileCoords current = openSet.First.Value;
            //print("current: " + current);
            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }
            openSet.Remove(current);
            closedSet.Add(current);
            //When implementing obstacles, just need to check if each neighbor is valid:
            //if its traversable or not
            TileCoords[] currentNeighbors = neighbors.Select(l => l + current).ToArray<TileCoords>();
            foreach (TileCoords neighbor in currentNeighbors)
            {
                Tile neighborTile = tileMap.getTile(neighbor);
                if (closedSet.Contains(neighbor))
                {
                    //print("already in closed set:" + neighbor);
                    continue;
                }
                else if (neighborTile == null) {
                    continue;
                }
                else if (!neighborTile.isTraversable())
                {
                    continue;
                }
                //trying to give gScore of key a default value
                float neighborGScore;
                gScore.TryGetValue(neighbor, out neighborGScore);
                gScore[neighbor] = neighborGScore == 0f ? 10000 : neighborGScore;
                //print(neighbor + "gScore: " + gScore[neighbor]);
                //(the cost of moving from start to neighbor) + (default value of moving between tiles)
                //for tiles with different movement costs, this will have to change
                float tentativeGScore = gScore[current] + 1;
                if (!(openSet.Contains(neighbor)))
                {
                    float neighborFScore;
                    fScore.TryGetValue(neighbor, out neighborFScore);
                    fScore[neighbor] = neighborFScore == 0f ? 10000 : neighborFScore;
                    openSet.AddToQueue(neighbor, fScore);
                }
                //Here: gScore[neighbor] shouldn't have a value yet
                else if (tentativeGScore >= gScore[neighbor])
                {
                    continue;
                }
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + ManhattanHeuristic(neighbor, goal);
            }
        }
        return null;
    }

    //Helper method for FindShortestPath.
    protected LinkedList<TileCoords> ReconstructPath(Dictionary<TileCoords, TileCoords> previousNode, TileCoords current)
    {
        LinkedList<TileCoords> totalPath = new LinkedList<TileCoords>();
        totalPath.AddFirst(current);
        while (previousNode.Select((l, r) => l.Key).Contains(current))
        {
            current = previousNode[current];
            totalPath.AddFirst(current);
        }
        return totalPath;
    }

    //Heuristic method for FindShortestPath.
    protected float ManhattanHeuristic(TileCoords node, TileCoords goal)
    {
        float dx = Mathf.Abs(node.x - goal.x);
        float dy = Mathf.Abs(node.z - goal.z);
        return 1 * (dx + dy);
    }

    /*Use this to find all available moves, taking movelimits into account. Can we pre-bake this into the coming A* 
     * algorithm to make sure that it only selects from available moves?*/
    protected HashSet<TileCoords> CalculateMoveLimits(TileCoords current, HashSet<TileCoords> visited, int limit = 3)
    {
        if (limit == 0)
        {
            visited.Add(current);
            return visited;
        }
        visited.Add(current);
        limit -= 1;
        //if right in visited - don't add else rightVect
        //TileCoords rightVect = new TileCoords(current.x + 1, current.y, current.z);
        HashSet<TileCoords> rightVisited = null;
        HashSet<TileCoords> leftVisited = null;
        HashSet<TileCoords> upVisited = null;
        HashSet<TileCoords> downVisited = null;
        //also check if each new TileCoords is traversable or not
        TileCoords rightTile = current + TileCoords.right;
        Tile rightTileCheck = tileMap.getTile(rightTile);
        if (rightTileCheck != null && rightTileCheck.isTraversable())
        {
            rightVisited = new HashSet<TileCoords>() { };
            rightVisited.UnionWith(visited);
            if (!(visited.Contains(rightTile)))
            {
                rightVisited = CalculateMoveLimits(rightTile, rightVisited, limit);
            }

        }
        //if left in visited - don't add else leftVect
        //TileCoords leftVect = new TileCoords(current.x, current.y, current.z + 1);
        TileCoords leftTile = current + TileCoords.left;
        Tile leftTileCheck = tileMap.getTile(leftTile);
        if (leftTileCheck != null && leftTileCheck.isTraversable())
        {
            leftVisited = new HashSet<TileCoords>() { };
            leftVisited.UnionWith(visited);
            if (!(visited.Contains(leftTile)))
            {
                leftVisited = CalculateMoveLimits(leftTile, leftVisited, limit);
            }

        }
        //if up in visited - don't add else upVect
        //TileCoords upVect = new TileCoords(current.x + 1, current.y, current.z + 1);
        TileCoords upTile = current + TileCoords.forward;
        Tile upTileCheck = tileMap.getTile(upTile);
        if (upTileCheck != null && upTileCheck.isTraversable())
        {
            upVisited = new HashSet<TileCoords>() { };
            upVisited.UnionWith(visited);
            if (!(visited.Contains(upTile)))
            {
                upVisited = CalculateMoveLimits(upTile, upVisited, limit);
            }

        }
        //if down in visited - don't add else downVect
        //TileCoords downVect = new TileCoords(current.x - 1, current.y, current.z - 1);
        TileCoords downTile = current + TileCoords.back;
        Tile downTileCheck = tileMap.getTile(downTile);
        if (downTileCheck != null && downTileCheck.isTraversable())
        {
            downVisited = new HashSet<TileCoords>() { };
            downVisited.UnionWith(visited);
            if (!(visited.Contains(downTile)))
            {
                downVisited = CalculateMoveLimits(downTile, downVisited, limit);
            }

        }
        if (rightVisited != null) visited.UnionWith(rightVisited);
        if (leftVisited != null) visited.UnionWith(leftVisited);
        if (upVisited != null) visited.UnionWith(upVisited);
        if (downVisited != null) visited.UnionWith(downVisited);
        return visited;

    }

}
