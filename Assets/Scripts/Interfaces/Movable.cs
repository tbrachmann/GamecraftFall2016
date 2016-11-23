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

    protected LinkedList<Tile> FindShortestPath(Tile start, Tile goal)
    {
        //print("start: " + start + " " + "goal: " + goal);
        HashSet<Tile> closedSet = new HashSet<Tile>();
        PriorityQueue openSet = new PriorityQueue(new List<Tile>() { start });
        TileCoords[] neighbors = { TileCoords.right, TileCoords.forward, TileCoords.left, TileCoords.back };
        //Node -> the node that it can most efficiently be reached from
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile> { };
        //Node -> "cost" from getting from start to this node
        //default value of infinity
        Dictionary<Tile, float> gScore = new Dictionary<Tile, float> { };
        //gScore.Add(start, 0);
        gScore[start] = 0;
        //Node -> the "score" of getting from start node to goal through this node
        //default value of infinity
        Dictionary<Tile, float> fScore = new Dictionary<Tile, float> { };
        fScore[start] = ManhattanHeuristic(start, goal);
        while (openSet.Count != 0)
        {
            //Aggregate - like python map? - was so proud of this but it turned out to be useless
            //Vector3 current = 
            // fScore.Aggregate((key, nextKey) => key.Value < nextKey.Value ? key : nextKey).Key;
            Tile current = openSet.First.Value;
            //print("current: " + current);
            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }
            openSet.Remove(current);
            closedSet.Add(current);
            //When implementing obstacles, just need to check if each neighbor is valid:
            //if its traversable or not
            Tile[] currentNeighbors = neighbors.Select(l => tileMap.getTile(l + current.getCoords())).ToArray<Tile>();
            foreach (Tile neighbor in currentNeighbors)
            {
                if (closedSet.Contains(neighbor))
                {
                    //print("already in closed set:" + neighbor);
                    continue;
                }
                else if (neighbor == null)
                {
                    continue;
                }
                else if (!neighbor.isTraversable())
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
    protected LinkedList<Tile> ReconstructPath(Dictionary<Tile, Tile> previousNode, Tile current)
    {
        LinkedList<Tile> totalPath = new LinkedList<Tile>();
        totalPath.AddFirst(current);
        while (previousNode.Select((l, r) => l.Key).Contains(current))
        {
            current = previousNode[current];
            totalPath.AddFirst(current);
        }
        return totalPath;
    }

    //Heuristic method for FindShortestPath.
    protected float ManhattanHeuristic(Tile node, Tile goal)
    {
        TileCoords nodeCoords = node.getCoords();
        TileCoords goalCoords = node.getCoords();
        float dx = Mathf.Abs(nodeCoords.x - goalCoords.x);
        float dy = Mathf.Abs(nodeCoords.z - goalCoords.z);
        return 1 * (dx + dy);
    }

    /*Use this to find all available moves, taking movelimits into account. Can we pre-bake this into the coming A* 
     * algorithm to make sure that it only selects from available moves?*/
    protected HashSet<Tile> CalculateMoveLimits(Tile current, HashSet<Tile> visited, int limit = 3)
    {
        if (limit == 0)
        {
            visited.Add(current);
            return visited;
        }
        visited.Add(current);
        limit -= 1;
        //if right in visited - don't add else rightVect
        //Tile rightVect = new Tile(current.x + 1, current.y, current.z);
        HashSet<Tile> rightVisited = null;
        HashSet<Tile> leftVisited = null;
        HashSet<Tile> upVisited = null;
        HashSet<Tile> downVisited = null;
        //also check if each new tile is traversable or not
        Tile rightTile = tileMap.getTile(current.getCoords() + TileCoords.right);
        if (rightTile != null && rightTile.isTraversable())
        {
            rightVisited = new HashSet<Tile>() { };
            rightVisited.UnionWith(visited);
            if (!(visited.Contains(rightTile)))
            {
                rightVisited = CalculateMoveLimits(rightTile, rightVisited, limit);
            }

        }
        //if left in visited - don't add else leftVect
        //Tile leftVect = new Tile(current.x, current.y, current.z + 1);
        Tile leftTile = tileMap.getTile(current.getCoords() + TileCoords.left);
        if (leftTile != null && leftTile.isTraversable())
        {
            leftVisited = new HashSet<Tile>() { };
            leftVisited.UnionWith(visited);
            if (!(visited.Contains(leftTile)))
            {
                leftVisited = CalculateMoveLimits(leftTile, leftVisited, limit);
            }

        }
        //if up in visited - don't add else upVect
        //Tile upVect = new Tile(current.x + 1, current.y, current.z + 1);
        Tile upTile = tileMap.getTile(current.getCoords() + TileCoords.forward);
        if (upTile != null && upTile.isTraversable())
        {
            upVisited = new HashSet<Tile>() { };
            upVisited.UnionWith(visited);
            if (!(visited.Contains(upTile)))
            {
                upVisited = CalculateMoveLimits(upTile, upVisited, limit);
            }

        }
        //if down in visited - don't add else downVect
        //Tile downVect = new Tile(current.x - 1, current.y, current.z - 1);
        Tile downTile = tileMap.getTile(current.getCoords() + TileCoords.back);
        if (downTile != null && downTile.isTraversable())
        {
            downVisited = new HashSet<Tile>() { };
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
