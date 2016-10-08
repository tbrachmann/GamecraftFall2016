using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour {

    Rigidbody player;
    RectTransform mouseHelp;
    Camera mainCam;
    float moveX;
    float moveY;
    public float speed = 2f;
    public LayerMask floorMask;
    RaycastHit floorPos;
    Ray camRay;
    bool playerMoving = false;
    Vector3 newPos;
    LineRenderer myLine;
    public const int playerMoveLimit = 3;

    // Use this for initialization
    void Start () {
        player = this.GetComponent<Rigidbody>();
        myLine = player.gameObject.AddComponent<LineRenderer>();
        mouseHelp = GameObject.FindGameObjectWithTag("WorldUI").GetComponent<RectTransform>();
        mainCam = FindObjectOfType<Camera>();
        //mouseHelp.gameObject.SetActive(false);
        HashSet<Vector3> returnList = CalculateMoveLimits(player.transform.position, new HashSet<Vector3>{ });
        IEnumerator<Vector3> myEnumerator = returnList.GetEnumerator();
        print(returnList.Count);
        //((RectTransform)Object.Instantiate(mouseHelp)).transform.position = new Vector3(myEnumerator.Current.x, 0.0001f, myEnumerator.Current.z);
        while (myEnumerator.MoveNext())
        {
            //print(returnList[i]);
            ((RectTransform)Object.Instantiate(mouseHelp)).transform.position = new Vector3(myEnumerator.Current.x, 0.0001f, myEnumerator.Current.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        camRay = mainCam.ScreenPointToRay(Input.mousePosition);
        List<Vector3> path;
        //If it hits the floor
        if (Physics.Raycast(camRay, out floorPos, floorMask))
        {
            //mouseHelp.gameObject.SetActive(true);
            Vector3 mousePos1 = new Vector3(ConvertToFloorUnits(floorPos.point.x), 0.0001f, ConvertToFloorUnits(floorPos.point.z));
            //print("MousePos: " + floorPos.point.x + ", " + floorPos.point.z + "; MouseHelp: " + mousePos1.x + ", " + mousePos1.z);
            mouseHelp.transform.position = mousePos1;
            if (Input.GetMouseButtonDown(0) && !playerMoving)
            {
                newPos = mousePos1;
                newPos.y = player.transform.position.y;
                playerMoving = true;
                path = FindShortestPath(player.transform.position, mousePos1);
            }
            if(playerMoving)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, newPos, Time.deltaTime * 3f);
                if (player.transform.position == newPos) playerMoving = false;
                //Draw the pathfinding line.
                //print(ManhattanHeuristic(player.transform.position, mousePos1)); 
                
                
                //myLine.numPositions = path.Count;
                //myLine.SetPositions(path.ToArray());
            }
        }
    }

    /*HashSet<Vector3> CalculateShortestPaths(Vector3 current, Dictionary<Vector3, LinkedList<Vector3>> visited, int limit = playerMoveLimit)
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

    List<Vector3> FindShortestPath(Vector3 start, Vector3 goal) {
        HashSet<Vector3> closedSet = new HashSet<Vector3> {};
        HashSet<Vector3> openSet = new HashSet<Vector3> {start};
        Vector3[] neighbors = {Vector3.right, Vector3.left, Vector3.forward, Vector3.back};
        //Node -> the node that it can most efficiently be reached from
        Dictionary<Vector3, Vector3> previousNode = new Dictionary<Vector3, Vector3> { };
        //Node -> its distance from start
        Dictionary<Vector3, float> gScore = new Dictionary<Vector3, float> { };
        //gScore.Add(start, 0);
        gScore[start] = 0;
        //Node -> the "score" of getting from start node to goal through this node
        Dictionary<Vector3, float> fScore = new Dictionary<Vector3, float> { };
        fScore[start] = ManhattanHeuristic(start, goal);
        while (openSet.Count != 0) {
            //Aggregate - like python map?
            //first run, this should be start
            Vector3 current = 
                fScore.Aggregate((key, nextKey) => key.Value < nextKey.Value ? key : nextKey).Key;
            if (current == goal)
            {
                print("ever exit?");
                return ReconstructPath(previousNode, current);
            }
            openSet.Remove(current);
            closedSet.Add(current);
            //how to get neighbor of current
            Vector3[] currentNeighbors = neighbors.Select(l => l + current).ToArray<Vector3>();
            foreach(Vector3 neighbor in currentNeighbors) {
                if (closedSet.Contains(neighbor)) {
                    continue;
                }
                /*float neighborGScore;
                gScore.TryGetValue(neighbor, out neighborGScore);
                gScore[neighbor] = neighborGScore == 0f ? 10000 : neighborGScore;*/ 
                float tentativeGScore = gScore[current] + 1;
                if (!(openSet.Contains(neighbor)))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= gScore[neighbor]) {
                    continue;
                }
                previousNode[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + ManhattanHeuristic(neighbor, goal);
            }
        }
        return null;
    }

    List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> previousNode, Vector3 current) {
        List<Vector3> totalPath = new List<Vector3> { current };
        while (previousNode.Select((l, r) => l.Key).Contains(current)) {
            current = previousNode[current];
            totalPath.Add(current);
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
        HashSet<Vector3> rightVisited = new HashSet<Vector3> { };
        rightVisited.UnionWith(visited);
        if (!(visited.Contains(rightVect))) {
            rightVisited = CalculateMoveLimits(rightVect, rightVisited, limit);
        }
        //if left in visited - don't add else leftVect
        //Vector3 leftVect = new Vector3(current.x, current.y, current.z + 1);
        Vector3 leftVect = current + Vector3.left;
        HashSet<Vector3> leftVisited = new HashSet<Vector3> { };
        leftVisited.UnionWith(visited);
        if (!(visited.Contains(leftVect)))
        {
            leftVisited = CalculateMoveLimits(leftVect, leftVisited, limit);
        }
        //if up in visited - don't add else upVect
        //Vector3 upVect = new Vector3(current.x + 1, current.y, current.z + 1);
        Vector3 upVect = current + Vector3.forward;
        HashSet<Vector3> upVisited = new HashSet<Vector3> { };
        upVisited.UnionWith(visited);
        if (!(visited.Contains(upVect)))
        {
            upVisited = CalculateMoveLimits(upVect, upVisited, limit);
        }
        //if down in visited - don't add else downVect
        //Vector3 downVect = new Vector3(current.x - 1, current.y, current.z - 1);
        Vector3 downVect = current + Vector3.back;
        HashSet<Vector3> downVisited = new HashSet<Vector3> { };
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

    float ConvertToFloorUnits(float x) {
        if (x < 0) return (int)x - 0.5f;
        else return (int)x + 0.5f;
    }

}
