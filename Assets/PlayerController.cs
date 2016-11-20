using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, Turn {

    Rigidbody player;
    Transform cursor;
    PlayerState myState;
    Tile playerCurrentTile;
    //public GameObject TileMapObj;
    //set these to be children of the player
    GameObject drawMovesHelper;
    GameObject drawMoves;
    Image drawMovesImage;
    LineRenderer myLine;
    //Flag for a finished state if it doesn't end on input.
    bool stateFinished = false;

    Camera mainCam;
    public float speed = 2f;
    public LayerMask floorMask;
    RaycastHit floorPos;
    LinkedList<Tile> path;
    Ray camRay;
    //bool cursorOnMap;
    TileMap tileMap;
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

    void Awake() {
        //get cursor, myLine, drawMoves stuff in here
    }

    // Use this for initialization
    void Start () {
        drawMovesHelper = GameObject.Find("DrawMovesHelper");
        drawMoves = drawMovesHelper.transform.GetChild(0).gameObject;
        drawMovesImage = drawMoves.transform.GetComponentInChildren<Image>();
        myLine = gameObject.GetComponent<LineRenderer>();
        player = gameObject.GetComponent<Rigidbody>();
        cursor = GameObject.Find("CursorHelper").GetComponent<Transform>();
        mainCam = FindObjectOfType<Camera>();
        //try to get all references here so that you don't have to 
        tileMap = GameManager.instance.getTileMap();
        //Instantiate myLine and disable it - now only State ReadyToMove
        //will deal with it.
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
        playerCurrentTile = tileMap.getTile(player.transform.position);
    }

    public void StartTurn() { }
    public void EndTurn() { }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.playerTurn) {
            return;
        }
        camRay = mainCam.ScreenPointToRay(Input.mousePosition);
        //If it hits the floor
        if (Physics.Raycast(camRay, out floorPos, floorMask))
        {
            cursor.gameObject.SetActive(true);
            cursor.position = tileMap.getTile(floorPos.point).coordsToVector3();
        } else {
            //cursorOnMap = false; 
            cursor.gameObject.SetActive(false);
        }
        playerCurrentTile = tileMap.getTile(player.transform.position);
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
    private class ReadyToMove : Movable, PlayerState
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
        PlayerController controller;

        public ReadyToMove(TileMap tileMap, PlayerController controller) : base(tileMap)
        {
            this.controller = controller;
            drawMovesHelper = controller.drawMovesHelper;
            drawMoves = controller.drawMoves;
            myLine = controller.myLine;
        }

        public void Enter() {
            tileMap = controller.tileMap;
            //This needs to be updated every time we enter this state.
            //OR does it get it from player controller update?
            playerTile = tileMap.getTile(controller.gameObject.GetComponent<Rigidbody>().transform.position);
            //Get the tile that draws the moves (DrawPossibleMoves 
            //will be instantiating this for how many moves there
            //are).
            //Draw the moves.
            DrawPossibleMoves();
        }
        
        public void Update() {
            //Position of the cursor. Set by player controller.
            //Vector3 cursorPos = controller.cursor.transform.position;
            if(controller.cursor.gameObject.activeSelf){
               Tile cursorTile = tileMap.getTile(controller.cursor.position);    
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

        public void Exit() {
            controller.path = preparePath;
            //Share the line with player so that the next state can access it.
            //Turn off possible movement grid and path line.
            RemovePossibleMoves();
            myLine.enabled = false;
        }

        public PlayerState HandleInput()
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
            //instantiate all possible moves and then go through list and enable
            while (myEnumerator.MoveNext())
            {
                //print(returnList[i]);
                TileCoords currentCoords = myEnumerator.Current.getCoords();
                GameObject moveSquare = GameObject.Instantiate(drawMoves, drawMovesHelper.transform);
                //y = 0.0001f so that its just off the floor
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
    }

    private class Moving : PlayerState
    {
        LineRenderer myLine;
        LinkedList<Tile> myPath;
        LinkedListNode<Tile> nextTile;
        Transform player;
        PlayerController controller;

        public Moving(PlayerController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            player = controller.gameObject.transform.parent;
            myLine = controller.gameObject.GetComponent<LineRenderer>();
            myLine.enabled = true;
            myPath = controller.path;
            nextTile = myPath.First;
        }

        public void Exit()
        {
            myLine.enabled = false;
            GameManager.instance.playerTurn = false;
        }

        public void Update()
        {
            Vector3 nextTilePos = nextTile.Value.coordsToVector3();
            Quaternion rotation = Quaternion.LookRotation(nextTilePos - player.position);
            //Debug.Log(rotation.eulerAngles);
            //Vector3 eulerAngleVelocity = new Vector3(0, 3, 0);
            //Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * (Time.deltaTime * 3f));
            //player.GetComponentInChildren<Rigidbody>().MoveRotation(rotation * deltaRotation);
            player.GetComponentInChildren<Rigidbody>().rotation = rotation;
            if (nextTilePos == null) {
                controller.stateFinished = true;
            }
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
        }

        public PlayerState HandleInput() {
            if(!(Input.anyKeyDown)){
                return new WaitingForInput(controller);    
            } else {
                return null;
            }

        }

    }

    private class WaitingForInput : PlayerState
    {
        PlayerController controller;

        public WaitingForInput(PlayerController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        public PlayerState HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                return new ReadyToMove(controller.tileMap, controller);
            }
            return null;
        }

        public void Update()
        {
        }
    }


}