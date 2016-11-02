using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, Turn {

    Rigidbody player;
    Transform cursor;
    PlayerState myState;
    Tile playerCurrentTile;
    public GameObject TileMapObj;
    public GameObject drawMovesHelper;
    public GameObject drawMoves;
    public Image drawMovesImage;
    public LineRenderer myLine;
    TileMap myTileMap;
    //Flag for a finished state if it doesn't end on input.
    bool stateFinished = false;

    Camera mainCam;
    public float speed = 2f;
    public LayerMask floorMask;
    RaycastHit floorPos;
    LinkedList<Tile> path;
    Ray camRay;
    //bool cursorOnMap;
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
        myTileMap = (TileMap) TileMapObj.GetComponent("TileMap");
        //Instantiate myLine and disable it - now only State ReadyToMove
        //will deal with it.
        LineRenderer myLine = this.gameObject.GetComponent<LineRenderer>();
        myLine.startColor = Color.white;
        myLine.endColor = Color.white;
        myLine.startWidth = 0.1f;
        myLine.endWidth = 0.1f;
        myLine.enabled = false;
        //Same with drawMoves
        Color myColor = drawMovesImage.color;
        myColor.a = 0.5f;
        drawMovesImage.color = myColor;
        drawMoves.SetActive(false);
        //The state to start out in. Waiting for input!
        myState = new WaitingForInput(this);
        player = this.GetComponent<Rigidbody>();
        cursor = GameObject.Find("CursorHelper").GetComponent<Transform>();
        mainCam = FindObjectOfType<Camera>();
        playerCurrentTile = myTileMap.getTile(player.transform.position);
        
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
            cursor.gameObject.SetActive(true);
            //cursorOnMap = true;
            cursor.position = myTileMap.getTile(TileMapObj.transform.InverseTransformPoint(floorPos.point)).coordsToVector3();
        } else {
            //cursorOnMap = false; 
            cursor.gameObject.SetActive(false);
        }
        playerCurrentTile = myTileMap.getTile(player.transform.position);
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
        List<GameObject> movesUI = new List<GameObject>();
        //The tile to instantiate when drawing moves.
        GameObject drawMovesHelper;
        GameObject drawMoves;
        Image drawMovesImage;
        Vector3 playerPos;
        LineRenderer myLine;
        LinkedList<Tile> preparePath;
        List<Tile> possibleMoves;
        Tile playerTile;
        //Tile cursorTile;
        TileMap myTileMap;

        public ReadyToMove(PlayerController controller) : base(controller)
        {
            drawMovesHelper = controller.drawMovesHelper;
            drawMoves = controller.drawMoves;
            myLine = controller.myLine;
        }

        public override void Enter() {
            myTileMap = controller.myTileMap;
            playerTile = myTileMap.getTile(controller.gameObject.GetComponent<Rigidbody>().transform.position);
            //This needs to be updated every time we enter this state.
            //playerPos = controller.gameObject.GetComponent<Rigidbody>().transform.position;
            //Get the tile that draws the moves (DrawPossibleMoves 
            //will be instantiating this for how many moves there
            //are).
            //drawMoves.SetActive(false);
            //drawMoves.SetActive(false);
            //Draw the moves.
            DrawPossibleMoves();
        }
        
        public override void Update() {
            //Position of the cursor. Set by player controller.
            //Vector3 cursorPos = controller.cursor.transform.position;
            if(controller.cursor.gameObject.activeSelf){
               Tile cursorTile = myTileMap.getTile(controller.cursor.position);    
            //this isn't really efficient though - instantiating and deleting game objects
            //we'll leave it as it is right now though, because its well-controlled
            //possibleMoves = new List<Tile>(movesUI.Select(l => l.position));
            //Check that the cursor is currently in possible moves.
            //Is this really necessary? We could draw a white line in possible moves
            //and then a red line outside.
                if (possibleMoves.Contains(cursorTile)) {
                    //Keep the line with Y = 0.0001f so that we can easily draw it.
                    //Vector3 playerPosToMouse = new Vector3(playerPos.x, cursorPos.y, playerPos.z);
                    preparePath = FindShortestPath(controller.playerCurrentTile, cursorTile);
                    DrawPath();
                } else {
                    myLine.enabled = false;
                } 
            }
        }

        public override void Exit() {
            //put path into player Y units?
            //controller.path = new LinkedList<Vector3>(preparePath.Select(l => new Vector3(l.x, playerPos.y, l.z)));
            controller.path = preparePath;
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
            if (Input.GetMouseButtonDown(0) && myLine.enabled) {
                return new Moving(controller);
            }
            return null;
        }

        void DrawPath()
        {
            myLine.enabled = true;
            myLine.numPositions = preparePath.Count;
            myLine.SetPositions(preparePath.Select(l => new Vector3(l.getCoords().x + 0.5f, 0.01f, l.getCoords().z + 0.5f)).ToArray());
        }

        private void DrawPossibleMoves()
        {
            drawMoves.SetActive(true);
            //RectTransform imagePos = drawMoves.canvas.GetComponent<RectTransform>();
            possibleMoves = CalculateMoveLimits(playerTile, new HashSet<Tile>()).ToList();
            IEnumerator<Tile> myEnumerator = possibleMoves.GetEnumerator();
            //(returnList.Count);
            //((RectTransform)Object.Instantiate(mouseHelp)).transform.position = new Vector3(myEnumerator.Current.x, 0.0001f, myEnumerator.Current.z);
            //instantiate all possible moves and then go through list and enable
            while (myEnumerator.MoveNext())
            {
                //print(returnList[i]);
                TileCoords currentCoords = myEnumerator.Current.getCoords();
                GameObject moveSquare = GameObject.Instantiate(drawMoves, drawMovesHelper.transform);
                //RectTransform moveSquare = ((RectTransform)UnityEngine.Object.Instantiate(imagePos));
                //drawMovesHelper.transform.InverseTransformPoint(new Vector3(currentCoords.x, 0.0001f, currentCoords.z);)
                moveSquare.transform.position = new Vector3(currentCoords.x + 0.5f, 0.0001f, currentCoords.z + 0.5f);
                movesUI.Add(moveSquare);
            }
            drawMoves.SetActive(false);
        }

        void RemovePossibleMoves()
        {
            foreach (GameObject r in movesUI)
            {
                Destroy(r);
            }
            movesUI.Clear();
            possibleMoves.Clear();
            //drawMoves.enabled = false;
        }

        //Find the shortest path to follow to goal.
        LinkedList<Tile> FindShortestPath(Tile start, Tile goal)
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
                    //print("ever exit?");
                    return ReconstructPath(cameFrom, current);
                }
                openSet.Remove(current);
                closedSet.Add(current);
                //how to get neighbor of current
                string closedString = "";
                /*foreach (Tile t in closedSet)
                {
                    closedString = closedString + " " + v;
                }*/
                //print(closedString);
                //When implementing obstacles, just need to check if each neighbor is valid:
                //if its traversable or not
                Tile[] currentNeighbors = neighbors.Select(l => myTileMap.getTile(l + current.getCoords())).ToArray<Tile>();
                foreach (Tile neighbor in currentNeighbors)
                {
                    if (closedSet.Contains(neighbor))
                    {
                        //print("already in closed set:" + neighbor);
                        continue;
                    } else if(neighbor == null){
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
        LinkedList<Tile> ReconstructPath(Dictionary<Tile, Tile> previousNode, Tile current)
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
        float ManhattanHeuristic(Tile node, Tile goal)
        {
            TileCoords nodeCoords = node.getCoords();
            TileCoords goalCoords = node.getCoords();
            float dx = Mathf.Abs(nodeCoords.x - goalCoords.x);
            float dy = Mathf.Abs(nodeCoords.z - goalCoords.z);
            return 1 * (dx + dy);
        }

        /*Use this to find all available moves, taking movelimits into account. Can we pre-bake this into the coming A* 
         * algorithm to make sure that it only selects from available moves?*/
        HashSet<Tile> CalculateMoveLimits(Tile current, HashSet<Tile> visited, int limit = playerMoveLimit)
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
            Tile rightTile = myTileMap.getTile(current.getCoords() + TileCoords.right);
            if(rightTile != null && rightTile.isTraversable()){
                rightVisited = new HashSet<Tile>() { };
                rightVisited.UnionWith(visited);
                if (!(visited.Contains(rightTile)))
                {
                    rightVisited = CalculateMoveLimits(rightTile, rightVisited, limit);
                }
                
            }
            //if left in visited - don't add else leftVect
            //Tile leftVect = new Tile(current.x, current.y, current.z + 1);
            Tile leftTile = myTileMap.getTile(current.getCoords() + TileCoords.left);
            if(leftTile != null && leftTile.isTraversable()) {
                leftVisited = new HashSet<Tile>() { };
                leftVisited.UnionWith(visited);
                if (!(visited.Contains(leftTile)))
                {
                    leftVisited = CalculateMoveLimits(leftTile, leftVisited, limit);
                }
                
            }
            //if up in visited - don't add else upVect
            //Tile upVect = new Tile(current.x + 1, current.y, current.z + 1);
            Tile upTile = myTileMap.getTile(current.getCoords() + TileCoords.forward);
            if(upTile != null && upTile.isTraversable()){
                upVisited = new HashSet<Tile>() { };
                upVisited.UnionWith(visited);
                if (!(visited.Contains(upTile)))
                {
                    upVisited = CalculateMoveLimits(upTile, upVisited, limit);
                }
                
            }
            //if down in visited - don't add else downVect
            //Tile downVect = new Tile(current.x - 1, current.y, current.z - 1);
            Tile downTile = myTileMap.getTile(current.getCoords() + TileCoords.back);
            if(downTile != null && downTile.isTraversable()) {
                downVisited = new HashSet<Tile>() { };
                downVisited.UnionWith(visited);
                if (!(visited.Contains(downTile)))
                {
                    downVisited = CalculateMoveLimits(downTile, downVisited, limit);
                } 
                
            } 
            if(rightVisited != null) visited.UnionWith(rightVisited);
            if(leftVisited != null) visited.UnionWith(leftVisited);
            if(upVisited != null) visited.UnionWith(upVisited);
            if(downVisited != null) visited.UnionWith(downVisited);
            return visited;
        }
    }

    private class Moving : PlayerState
    {
        LineRenderer myLine;
        LinkedList<Tile> myPath;
        LinkedListNode<Tile> nextTile;
        Transform player;

        public Moving(PlayerController controller) : base(controller)
        {   
        }

        public override void Enter()
        {
            player = controller.gameObject.transform.parent;
            myLine = controller.gameObject.GetComponent<LineRenderer>();
            myLine.enabled = true;
            myPath = controller.path;
            nextTile = myPath.First;
        }

        public override void Exit()
        {
            myLine.enabled = false;
        }

        public override void Update()
        {   
            Vector3 nextTilePos = nextTile.Value.coordsToVector3(); 
            if(player.position != nextTilePos){
                //print(player.position + " " + nextTile.Value.coordsToVector3());
                player.position = Vector3.MoveTowards(player.position, nextTilePos, Time.deltaTime * 3f);
            }
            if(player.position == nextTilePos){
                nextTile = nextTile.Next;
            }
            if(nextTile == null){
                controller.stateFinished = true;
            }
            /*if (player.position != nextPos.Value) {
                player.position = Vector3.MoveTowards(player.position, nextPos.Value, Time.deltaTime * 3f);
            }
            if (player.position == nextPos.Value) {
                nextPos = nextPos.Next;
            }
            if (nextPos == null) {
                controller.stateFinished = true;
            }*/
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
