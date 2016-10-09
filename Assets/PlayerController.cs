using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class PlayerController : MonoBehaviour {

    Rigidbody player;
    RectTransform mouseHelp;
    Image drawMoves;
    Camera mainCam;
    float moveX;
    float moveY;
    public float speed = 2f;
    public LayerMask floorMask;
    RaycastHit floorPos;
    Vector3 newPos;
    LinkedListNode<Vector3> nextPos;
    LinkedList<Vector3> path;
    List<RectTransform> movesUI = new List<RectTransform>();
    Ray camRay;
    bool playerMoving = false;
    bool playerReadyToMove = false;
    LineRenderer myLine;
    public const int playerMoveLimit = 3;

    //Hack-y code to make sure that Vector3s match up in closedSet
    //was having trouble with the HashCodes not matching up. So now
    //all Vector3s have the same hashcode (0) and are evaluated for
    //equality based on values alone.
    private class Vector3Comparer : EqualityComparer<Vector3> {

        public override bool Equals(Vector3 a, Vector3 b) {
            return a == b;
        }

        //hack-y way to make sure they have same hashcodes
        public override int GetHashCode(Vector3 a) {
            return 0;
        }

    }
    
    // Use this for initialization
    void Start () {
        player = this.GetComponent<Rigidbody>();
        myLine = player.gameObject.GetComponent<LineRenderer>();
        myLine.startColor = Color.white;
        myLine.endColor = Color.white;
        myLine.startWidth = 0.1f;
        myLine.endWidth = 0.1f;
        myLine.enabled = false;
        mouseHelp = GameObject.Find("MouseHelper").GetComponent<RectTransform>();
        mainCam = FindObjectOfType<Camera>();
        //mainCam = GameObject.FindGameObjectsWithTag("MainCamera");
        print(mainCam);
        //mouseHelp.gameObject.SetActive(false);
        drawMoves = GameObject.Find("PossibleMoves").GetComponentInChildren<Image>();
        Color myColor = drawMoves.color;
        myColor.a = 0.5f;
        drawMoves.color = myColor;
        drawMoves.enabled = false;
    }

    // Update is called once per frame
    void Update()
    { 
        camRay = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Input.GetKeyDown(KeyCode.M) && !playerReadyToMove)
        {
            DrawPossibleMoves();
            playerReadyToMove = true;
        }
        //If it hits the floor
        if (Physics.Raycast(camRay, out floorPos, floorMask))
        {
            //mouseHelp.gameObject.SetActive(true);
            Vector3 mousePos1 = new Vector3(ConvertToFloorUnits(floorPos.point.x), 0.0001f, ConvertToFloorUnits(floorPos.point.z));
            mouseHelp.transform.position = mousePos1;
            List<Vector3> movesList = movesUI.Select(l => l.transform.position).ToList();
            if (movesList.Contains(mouseHelp.transform.position, new Vector3Comparer()) && playerReadyToMove) { //&& (path != null && path.Last.Value != mouseHelp.transform.position)) {
                print("does it detect mouse?");
                //draw the path
                Vector3 playerPosToMouse = new Vector3(player.transform.position.x, mouseHelp.transform.position.y, player.transform.position.z);
                path = FindShortestPath(playerPosToMouse, mousePos1);
                DrawPath();
                if (Input.GetMouseButtonDown(0) && playerReadyToMove)
                {
                    if (!playerMoving)
                    {
                        newPos = mousePos1;
                        newPos.y = player.transform.position.y;
                        playerMoving = true;
                        //if last element of current path does not equal mousehelp then find a new one
                        if (path == null)
                        {
                            path = FindShortestPath(player.transform.position, newPos);
                        }
                        else
                        {
                            path = new LinkedList<Vector3>(path.Select(l => new Vector3(l.x, player.transform.position.y, l.z)));
                            if (path.Last.Value != newPos)
                            {
                                path = FindShortestPath(player.transform.position, newPos);
                            }
                        }
                        nextPos = path.First;
                        //foreach (Vector3 v in path) print(v);
                    }
                }
            }
            
            if (playerMoving)
            {
                RemovePossibleMoves();
                if (player.transform.position != nextPos.Value)
                {
                    player.transform.position = Vector3.MoveTowards(player.transform.position, nextPos.Value, Time.deltaTime * 3f);
                }
                if (player.transform.position == nextPos.Value)
                {
                    nextPos = nextPos.Next;
                }
                if (nextPos == null)
                {
                    playerReadyToMove = false;
                    playerMoving = false;
                    myLine.enabled = false;
                }
            }
        }
    }

    /*HashSet<Vector3> CalculateAllShortestPaths(Vector3 current, Dictionary<Vector3, LinkedList<Vector3>> visited, int limit = playerMoveLimit)
    {
        //end of path - set current as key
        if (limit == 0)
        {
            visited.Add(current);
            return visited;
        }
        if (visited.ContainsKey(current) && (playerMoveLimit - limit < visited[current].Count)) {
            visited[current].Clear();
            visited[current] = new LinkedList<Vector3>
        }
        //set current as key - path is visited?
        //how to get the ordered path of nodes??
        visited.Add(current);
        limit -= 1;
        //if right in visited - don't add else rightVect
        //Vector3 rightVect = new Vector3(current.x + 1, current.y, current.z);
        Vector3 rightVect = current + Vector3.right;
        HashSet<Vector3> rightVisited = new HashSet<Vector3> { };
        rightVisited.UnionWith(visited);
        if (!(visited.Contains(rightVect)))
        {
            rightVisited = CalculateShortestPaths(rightVect, rightVisited, limit);
        }
        //if left in visited - don't add else leftVect
        //Vector3 leftVect = new Vector3(current.x, current.y, current.z + 1);
        Vector3 leftVect = current + Vector3.left;
        HashSet<Vector3> leftVisited = new HashSet<Vector3> { };
        leftVisited.UnionWith(visited);
        if (!(visited.Contains(leftVect)))
        {
            leftVisited = CalculateShortestPaths(leftVect, leftVisited, limit);
        }
        //if up in visited - don't add else upVect
        //Vector3 upVect = new Vector3(current.x + 1, current.y, current.z + 1);
        Vector3 upVect = current + Vector3.forward;
        HashSet<Vector3> upVisited = new HashSet<Vector3> { };
        upVisited.UnionWith(visited);
        if (!(visited.Contains(upVect)))
        {
            upVisited = CalculateShortestPaths(upVect, upVisited, limit);
        }
        //if down in visited - don't add else downVect
        //Vector3 downVect = new Vector3(current.x - 1, current.y, current.z - 1);
        Vector3 downVect = current + Vector3.back;
        HashSet<Vector3> downVisited = new HashSet<Vector3> { };
        downVisited.UnionWith(visited);
        if (!(visited.Contains(downVect)))
        {
            downVisited = CalculateShortestPaths(downVect, downVisited, limit);
        }
        visited.UnionWith(upVisited);
        visited.UnionWith(downVisited);
        visited.UnionWith(rightVisited);
        visited.UnionWith(leftVisited);
        return visited;
    }*/

    //Do we want it to prefer a diagonal path?
    //If we add tiles with different movement costs, then the Heuristic will have to be changed.
    LinkedList<Vector3> FindShortestPath(Vector3 start, Vector3 goal) {
        //print("start: " + start + " " + "goal: " + goal);
        HashSet<Vector3> closedSet = new HashSet<Vector3>(new Vector3Comparer()) { };
        PriorityQueue openSet = new PriorityQueue(new List<Vector3>() {start});
        Vector3[] neighbors = {Vector3.right, Vector3.forward, Vector3.left, Vector3.back};
        //Node -> the node that it can most efficiently be reached from
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3> { };
        //Node -> "cost" from getting from start to this node
        //default value of infinity
        Dictionary<Vector3, float> gScore = new Dictionary<Vector3, float> { };
        //gScore.Add(start, 0);
        gScore[start] = 0;
        //Node -> the "score" of getting from start node to goal through this node
        //default value of infinity
        Dictionary<Vector3, float> fScore = new Dictionary<Vector3, float> { };
        fScore[start] = ManhattanHeuristic(start, goal);
        //while(i < 4) {
        while (openSet.Count != 0) {
            //Aggregate - like python map? - was so proud of this but it turned out to be useless
            //Vector3 current = 
            // fScore.Aggregate((key, nextKey) => key.Value < nextKey.Value ? key : nextKey).Key;
            Vector3 current = openSet.First.Value;
            //print("current: " + current);
            if (current == goal)
            {
                //print("ever exit?");
                return ReconstructPath(cameFrom, current);
            }
            openSet.Remove(current);
            closedSet.Add(current);
            //how to get neighbor of current
            string closedString = "";
            foreach (Vector3 v in closedSet) {
                closedString = closedString + " " + v;
            }
            //print(closedString);
            Vector3[] currentNeighbors = neighbors.Select(l => l + current).ToArray<Vector3>();
            foreach(Vector3 neighbor in currentNeighbors) {
                if (closedSet.Contains(neighbor)) {
                    //print("already in closed set:" + neighbor);
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
                else if (tentativeGScore >= gScore[neighbor]) {
                    continue;
                }
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + ManhattanHeuristic(neighbor, goal);
                //print(neighbor + "fScore: " + fScore[neighbor]);
            }
        }
        return null;
    }

    LinkedList<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> previousNode, Vector3 current) {
        LinkedList<Vector3> totalPath = new LinkedList<Vector3>();
        totalPath.AddFirst(current);
        while (previousNode.Select((l, r) => l.Key).Contains(current)) {
            current = previousNode[current];
            totalPath.AddFirst(current);
        }
        return totalPath;
    }

    float ManhattanHeuristic(Vector3 node, Vector3 goal) {
        float dx = Mathf.Abs(node.x - goal.x);
        float dy = Mathf.Abs(node.z - goal.z);
        return 1 * (dx + dy);
    }

    //Use this to paint all available moves.
    //Can we pre-bake this into the coming A* algorithm to make sure that it only selects from available moves?
    HashSet<Vector3> CalculateMoveLimits(Vector3 current, HashSet<Vector3> visited, int limit = playerMoveLimit) {
        if (limit == 0) {
            visited.Add(current);
            return visited;
        }
        visited.Add(current);
        limit -= 1;
        //if right in visited - don't add else rightVect
        //Vector3 rightVect = new Vector3(current.x + 1, current.y, current.z);
        Vector3 rightVect = current + Vector3.right;
        HashSet<Vector3> rightVisited = new HashSet<Vector3>() {};
        rightVisited.UnionWith(visited);
        if (!(visited.Contains(rightVect))) {
            rightVisited = CalculateMoveLimits(rightVect, rightVisited, limit);
        }
        //if left in visited - don't add else leftVect
        //Vector3 leftVect = new Vector3(current.x, current.y, current.z + 1);
        Vector3 leftVect = current + Vector3.left;
        HashSet<Vector3> leftVisited = new HashSet<Vector3>() { };
        leftVisited.UnionWith(visited);
        if (!(visited.Contains(leftVect)))
        {
            leftVisited = CalculateMoveLimits(leftVect, leftVisited, limit);
        }
        //if up in visited - don't add else upVect
        //Vector3 upVect = new Vector3(current.x + 1, current.y, current.z + 1);
        Vector3 upVect = current + Vector3.forward;
        HashSet<Vector3> upVisited = new HashSet<Vector3>() { };
        upVisited.UnionWith(visited);
        if (!(visited.Contains(upVect)))
        {
            upVisited = CalculateMoveLimits(upVect, upVisited, limit);
        }
        //if down in visited - don't add else downVect
        //Vector3 downVect = new Vector3(current.x - 1, current.y, current.z - 1);
        Vector3 downVect = current + Vector3.back;
        HashSet<Vector3> downVisited = new HashSet<Vector3>() { };
        downVisited.UnionWith(visited);
        if (!(visited.Contains(downVect)))
        {
            downVisited = CalculateMoveLimits(downVect, downVisited, limit);
        }
        visited.UnionWith(upVisited);
        visited.UnionWith(downVisited);
        visited.UnionWith(rightVisited);
        visited.UnionWith(leftVisited);
        return visited;
    }

    void DrawPossibleMoves() {
        drawMoves.enabled = true;
        RectTransform imagePos = drawMoves.canvas.GetComponent<RectTransform>();
        HashSet<Vector3> returnList = CalculateMoveLimits(player.transform.position, new HashSet<Vector3>(new Vector3Comparer()) { });
        IEnumerator<Vector3> myEnumerator = returnList.GetEnumerator();
        //(returnList.Count);
        //((RectTransform)Object.Instantiate(mouseHelp)).transform.position = new Vector3(myEnumerator.Current.x, 0.0001f, myEnumerator.Current.z);
        //instantiate all possible moves and then go through list and enable
        while (myEnumerator.MoveNext())
        {
            //print(returnList[i]);
            RectTransform moveSquare = ((RectTransform)UnityEngine.Object.Instantiate(imagePos));
            moveSquare.transform.position = new Vector3(myEnumerator.Current.x, 0.0001f, myEnumerator.Current.z);
            movesUI.Add(moveSquare);
        }
    }

    void RemovePossibleMoves() {
        foreach (RectTransform r in movesUI) {
            Destroy(r.gameObject);
        }
        movesUI.Clear();
    }

    void DrawPath() {
        myLine.enabled = true;
        myLine.numPositions = path.Count;
        myLine.SetPositions(path.Select(l => new Vector3(l.x, 0.001f, l.z)).ToArray());
    }

    float ConvertToFloorUnits(float x) {
        if (x < 0) return (int)x - 0.5f;
        else return (int)x + 0.5f;
    }

}
