using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class PlayerController : MonoBehaviour, Turn {

    Rigidbody player;
    RectTransform cursor;
    PlayerState myState;
    //Flag for a finished state if it doesn't end on input.
    bool stateFinished = false;

    Camera mainCam;
    public float speed = 2f;
    public LayerMask floorMask;
    RaycastHit floorPos;
    LinkedList<Vector3> path;
    Ray camRay;
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
        //Instantiate myLine and disable it - now only State ReadyToMove
        //will deal with it.
        LineRenderer myLine = this.gameObject.GetComponent<LineRenderer>();
        myLine.startColor = Color.white;
        myLine.endColor = Color.white;
        myLine.startWidth = 0.1f;
        myLine.endWidth = 0.1f;
        myLine.enabled = false;
        //Same with drawMoves
        Image drawMoves = GameObject.Find("PossibleMoves").GetComponentInChildren<Image>();
        Color myColor = drawMoves.color;
        myColor.a = 0.5f;
        drawMoves.color = myColor;
        drawMoves.enabled = false;
        //The state to start out in. Waiting for input!
        myState = new WaitingForInput(this);
        player = this.GetComponent<Rigidbody>();
        cursor = GameObject.Find("Cursor").GetComponent<RectTransform>();
        mainCam = FindObjectOfType<Camera>();
        //mainCam = GameObject.FindGameObjectsWithTag("MainCamera");
        //print(mainCam);
        //mouseHelp.gameObject.SetActive(false);
        
    }

    public void StartTurn() { }
    public void EndTurn() { }

    // Update is called once per frame
    void Update()
    { 
        camRay = mainCam.ScreenPointToRay(Input.mousePosition);
        //If it hits the floor
        if (Physics.Raycast(camRay, out floorPos, floorMask))
        {
            //mouseHelp.gameObject.SetActive(true);
            cursor.transform.position = new Vector3(ConvertToFloorUnits(floorPos.point.x), 0.0001f, ConvertToFloorUnits(floorPos.point.z));
        }
        myState.Update();
        if (Input.anyKeyDown || stateFinished) {
            PlayerState nextState = myState.HandleInput();
            if (nextState != null) {
                stateFinished = false;
                myState.Exit();
                myState = nextState;
                myState.Enter();
            }
        }
    }

    public float ConvertToFloorUnits(float x) {
        if (x < 0) return (int)x - 0.5f;
        else return (int)x + 0.5f;
    }

    //The state at the start of the turn.
    //Forget this state, just have player implement a TURN interface
    //that will also be implemented by enemies
    //methods: startTurn
    //endTurn\
    //that will be called by the game manager!
    //private so that it can only be accessed by the player
    private class ReadyToMove : PlayerState
    {
        //The list of tiles drawn on the board to show possible moves.
        //Y is 0.0001f
        List<RectTransform> movesUI = new List<RectTransform>();
        //The tile to instantiate when drawing moves.
        Image drawMoves;
        Vector3 playerPos;
        LineRenderer myLine;
        LinkedList<Vector3> preparePath;
        List<Vector3> possibleMoves;

        public ReadyToMove(PlayerController controller) : base(controller)
        {
        }

        public override void Enter() {
            //This needs to be updated every time we enter this state.
            playerPos = controller.gameObject.GetComponent<Rigidbody>().transform.position;
            //Get the tile that draws the moves (DrawPossibleMoves 
            //will be instantiating this for how many moves there
            //are).
            drawMoves = GameObject.Find("PossibleMoves").GetComponentInChildren<Image>();
            myLine = controller.gameObject.GetComponent<LineRenderer>();
            Color myColor = drawMoves.color;
            myColor.a = 0.5f;
            drawMoves.color = myColor;
            drawMoves.enabled = false;
            //Draw the moves.
            DrawPossibleMoves();
        }
        
        public override void Update() {
            //Position of the cursor. Set by player controller.
            Vector3 cursorPos = controller.cursor.transform.position;
            possibleMoves = new List<Vector3>(movesUI.Select(l => l.position));
            //Check that the cursor is currently in possible moves.
            //Is this really necessary? We could draw a white line in possible moves
            //and then a red line outside.
            if (possibleMoves.Contains(cursorPos, new Vector3Comparer())) {
                //Keep the line with Y = 0.0001f so that we can easily draw it.
                Vector3 playerPosToMouse = new Vector3(playerPos.x, cursorPos.y, playerPos.z);
                preparePath = FindShortestPath(playerPosToMouse, cursorPos);
                DrawPath();
            }
        }

        public override void Exit() {
            //put path into player Y units?
            controller.path = new LinkedList<Vector3>(preparePath.Select(l => new Vector3(l.x, playerPos.y, l.z)));
            //Share the line with player so that the next state can access it.
            //Turn off possible movement grid and path line.
            RemovePossibleMoves();
            myLine.enabled = false;
        }

        public override PlayerState HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.M)) {
                return new WaitingForInput(controller);
            }
            if (Input.GetMouseButtonDown(0)) {
                return new Moving(controller);
            }
            return null;
        }

        void DrawPath()
        {
            myLine.enabled = true;
            myLine.numPositions = preparePath.Count;
            myLine.SetPositions(preparePath.Select(l => new Vector3(l.x, 0.01f, l.z)).ToArray());
        }

        private void DrawPossibleMoves()
        {
            drawMoves.enabled = true;
            RectTransform imagePos = drawMoves.canvas.GetComponent<RectTransform>();
            HashSet<Vector3> returnList = CalculateMoveLimits(playerPos, new HashSet<Vector3>(new Vector3Comparer()) { });
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
            drawMoves.enabled = false;
        }

        void RemovePossibleMoves()
        {
            foreach (RectTransform r in movesUI)
            {
                Destroy(r.gameObject);
            }
            movesUI.Clear();
            possibleMoves.Clear();
            //drawMoves.enabled = false;
        }

        //Find the shortest path to follow to goal.
        LinkedList<Vector3> FindShortestPath(Vector3 start, Vector3 goal)
        {
            //print("start: " + start + " " + "goal: " + goal);
            HashSet<Vector3> closedSet = new HashSet<Vector3>(new Vector3Comparer()) { };
            PriorityQueue openSet = new PriorityQueue(new List<Vector3>() { start });
            Vector3[] neighbors = { Vector3.right, Vector3.forward, Vector3.left, Vector3.back };
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
            while (openSet.Count != 0)
            {
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
                foreach (Vector3 v in closedSet)
                {
                    closedString = closedString + " " + v;
                }
                //print(closedString);
                //When implementing obstacles, just need to check if each neighbor is valid:
                //if its traversable or not
                Vector3[] currentNeighbors = neighbors.Select(l => l + current).ToArray<Vector3>();
                foreach (Vector3 neighbor in currentNeighbors)
                {
                    if (closedSet.Contains(neighbor))
                    {
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
                    else if (tentativeGScore >= gScore[neighbor])
                    {
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

        //Helper method for FindShortestPath.
        LinkedList<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> previousNode, Vector3 current)
        {
            LinkedList<Vector3> totalPath = new LinkedList<Vector3>();
            totalPath.AddFirst(current);
            while (previousNode.Select((l, r) => l.Key).Contains(current))
            {
                current = previousNode[current];
                totalPath.AddFirst(current);
            }
            return totalPath;
        }

        //Heuristic method for FindShortestPath.
        float ManhattanHeuristic(Vector3 node, Vector3 goal)
        {
            float dx = Mathf.Abs(node.x - goal.x);
            float dy = Mathf.Abs(node.z - goal.z);
            return 1 * (dx + dy);
        }

        /*Use this to find all available moves, taking movelimits into account. Can we pre-bake this into the coming A* 
         * algorithm to make sure that it only selects from available moves?*/
        HashSet<Vector3> CalculateMoveLimits(Vector3 current, HashSet<Vector3> visited, int limit = playerMoveLimit)
        {
            if (limit == 0)
            {
                visited.Add(current);
                return visited;
            }
            visited.Add(current);
            limit -= 1;
            //if right in visited - don't add else rightVect
            //Vector3 rightVect = new Vector3(current.x + 1, current.y, current.z);
            Vector3 rightVect = current + Vector3.right;
            HashSet<Vector3> rightVisited = new HashSet<Vector3>() { };
            rightVisited.UnionWith(visited);
            if (!(visited.Contains(rightVect)))
            {
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
    }

    private class Moving : PlayerState
    {
        LineRenderer myLine;
        LinkedList<Vector3> myPath;
        LinkedListNode<Vector3> nextPos;
        Transform player;

        public Moving(PlayerController controller) : base(controller)
        {   
        }

        public override void Enter()
        {
            player = controller.gameObject.GetComponent<Rigidbody>().transform;
            myLine = controller.gameObject.GetComponent<LineRenderer>();
            myLine.enabled = true;
            myPath = controller.path;
            nextPos = myPath.First;
        }

        public override void Exit()
        {
            myLine.enabled = false;
        }

        public override void Update()
        {
            if (player.position != nextPos.Value) {
                player.position = Vector3.MoveTowards(player.position, nextPos.Value, Time.deltaTime * 3f);
            }
            if (player.position == nextPos.Value) {
                nextPos = nextPos.Next;
            }
            if (nextPos == null) {
                controller.stateFinished = true;
            }
        }

        public override PlayerState HandleInput() {
            return new WaitingForInput(controller);
        }

    }

    private class WaitingForInput : PlayerState
    {
        public WaitingForInput(PlayerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
        }

        public override void Exit()
        {
        }

        public override PlayerState HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                return new ReadyToMove(controller);
            }
            return null;
        }

        public override void Update()
        {
        }
    }


}
