using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, Combatable {

    Rigidbody player;
    RectTransform cursor;
    PlayerState myState;
    public Tile playerCurrentTile;
    //public GameObject TileMapObj;
    //set these to be children of the player
    GameObject drawMovesHelper;
    RectTransform drawMoves;
    List<RectTransform> drawMovesChildImages = new List<RectTransform>();
    RectTransform drawPath;
    List<RectTransform> drawPathChildImages = new List<RectTransform>();
    RectTransform targetingReticle;
    RectTransform targetingReticleImage;
    //Flag for a finished state if it doesn't end on input.
    bool stateFinished = false;
    //public bool myTurn = false;
    public float health = 100;
    int myActionPoints = 1;

    Camera mainCam;
    public float speed = 2f;
    public LayerMask floorMask;
    RaycastHit floorPos;
    LinkedList<Tile> path;
    Ray camRay;
    //bool cursorOnMap;
    TileMap tileMap;
    public const int playerMoveLimit = 1;

    static Dictionary<string, Attack> myAttacks = new Dictionary<string, Attack>()
    {
        {"Ferocious Bite", new Attack(20, 1, "Ferocious Bite") },
        {"Tennis Ball Throw", new Attack(15, 3, "Tennis Ball Throw") },
    };

    /*static Dictionary<string, Ability> myAbilities = new Dictionary<string, Ability>()
    {

    };*/

    static Dictionary<string, Stance> myStances = new Dictionary<string, Stance>()
    {
        {"Relaxed", new Stance(0, 0, "Relaxed") },
        {"Alert", new Stance(-0.15f, -0.1f, "Alert") },
        {"Threatened", new Stance(0.1f, 0.15f, "Threatened") }
    };


    void Awake() {
        //get cursor, myLine, drawMoves stuff in here
    }

    // Use this for initialization
    void Start () {
        //drawMovesHelper = GameObject.Find("DrawMovesHelper");
        //drawMoves = drawMovesHelper.transform.GetChild(0).gameObject;
        //drawMovesImage = drawMoves.transform.GetComponentInChildren<Image>();
        drawMoves = (RectTransform) GameObject.Find("DrawMoves").transform;
        //drawMovesChildImages = drawMoves.gameObject.GetComponentsInChildren<RectTransform>(true);
        foreach (Transform child in drawMoves.transform) {
            drawMovesChildImages.Add((RectTransform)child.transform);
        }
        drawPath = (RectTransform) GameObject.Find("DrawPath").transform;
        //drawPathChildImages = drawPath.gameObject.GetComponentsInChildren<RectTransform>(true);
        foreach (Transform child in drawPath.transform) {
            drawPathChildImages.Add((RectTransform)child.transform);
        }
        targetingReticle = (RectTransform)GameObject.Find("TargetingReticle").transform;
        targetingReticleImage = (RectTransform)targetingReticle.GetChild(0).transform;
        player = gameObject.GetComponent<Rigidbody>();
        cursor = (RectTransform) GameObject.Find("Cursor").transform;
        mainCam = FindObjectOfType<Camera>();
        //try to get all references here so that you don't have to 
        tileMap = GameManager.instance.getTileMap();
        //Same with drawMoves
        //Color myColor = drawMovesImage.color;
        //myColor.a = 0.5f;
        //drawMovesImage.color = myColor;
        //drawMoves.gameObject.SetActive(false);
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
            myActionPoints = 1;
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

    public void dealDamage(Combatable target, float damage) {
        //float modifiedDamage = damage + (damage * myStances.getDamageGivenModifier());
        target.takeDamage(damage);
    }

    public void takeDamage(float damage) {
        Debug.Log("Player took damage of " + damage + "!");
        health -= damage;
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
        RectTransform drawMoves;
        Image drawMovesImage;
        Vector3 playerPos;
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
                    foreach (RectTransform r in controller.drawPathChildImages)
                    {
                        r.gameObject.SetActive(false);
                    }
                } 
            }
        }

        public void Exit() {
            controller.path = preparePath;
            //Share the line with player so that the next state can access it.
            //Turn off possible movement grid and path line.
            RemovePossibleMoves();
        }

        public PlayerState HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.M)) {
                return new WaitingForInput(controller);
            }
            if (Input.GetMouseButtonDown(0) && tileMap.getTile(controller.floorPos.point) == preparePath.Last.Value) {
                return new Moving(controller);
            }
            return null;
        }

        void DrawPath()
        {
            foreach (RectTransform r in controller.drawPathChildImages) {
                r.gameObject.SetActive(false);
            }
            RectTransform prevLine = null;
            IEnumerator<RectTransform> lineEnumerator = controller.drawPathChildImages.GetEnumerator();
            IEnumerator<Tile> pathEnumerator = preparePath.GetEnumerator();
            pathEnumerator.MoveNext();
            Vector3 prevCoord = pathEnumerator.Current.coordsToVector3();
            while (lineEnumerator.MoveNext() && pathEnumerator.MoveNext()) {
                Vector3 nextCoord = pathEnumerator.Current.coordsToVector3();
                RectTransform currLine = lineEnumerator.Current;
                if (prevLine == null) prevLine = currLine;
                currLine.gameObject.SetActive(true);
                Vector3 rootPoint = new Vector3(prevCoord.x + .5f, 0, prevCoord.z + .5f);
                currLine.transform.position = rootPoint;
                Vector3 differenceVector = nextCoord - prevCoord;
                float angle = Mathf.Atan2(differenceVector.z, differenceVector.x) * Mathf.Rad2Deg;
                //its hacky but it works!!
                currLine.rotation = Quaternion.AngleAxis(angle, Vector3.down);
                currLine.Rotate(90, 0, 0);
                if (angle != 0) {
                    prevLine.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1050);
                }
                prevCoord = nextCoord;
                prevLine = currLine;
            }
        }

        private void DrawPossibleMoves()
        {
            //drawMoves.gameObject.SetActive(true);
            //RectTransform imagePos = drawMoves.canvas.GetComponent<RectTransform>();
            possibleMoves = CalculateMoveLimits(playerTile, new HashSet<Tile>()).ToList();
            IEnumerator<Tile> tileEnumerator = possibleMoves.GetEnumerator();
            IEnumerator<RectTransform> imageEnumerator = controller.drawMovesChildImages.GetEnumerator();
            //instantiate all possible moves and then go through list and enable
            while (tileEnumerator.MoveNext() && imageEnumerator.MoveNext())
            {
                //print(returnList[i]);
                TileCoords currentCoords = tileEnumerator.Current.getCoords();
                //GameObject moveSquare = GameObject.Instantiate(drawMoves.gameObject, drawMovesHelper.transform);
                RectTransform moveSquare = imageEnumerator.Current;
                //y = 0.0001f so that its just off the floor
                moveSquare.transform.position = new Vector3(currentCoords.x, 0, currentCoords.z);
                moveSquare.gameObject.SetActive(true);
                //movesUI.Add(moveSquare);
            }
            //drawMoves.gameObject.SetActive(false);
        }

        void RemovePossibleMoves()
        {
            foreach (RectTransform r in controller.drawMovesChildImages)
            {
                //Destroy(r);
                r.gameObject.SetActive(false);
            }
            //movesUI.Clear();
            possibleMoves.Clear();
            //drawMoves.enabled = false;
        }
    }

    private class Moving : PlayerState
    {
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
            myPath = controller.path;
            nextTile = myPath.First;
            controller.myActionPoints -= 1;
        }

        public void Exit()
        {
            foreach (RectTransform r in controller.drawPathChildImages)
            {
                r.gameObject.SetActive(false);
            }
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
            /*if (nextTilePos == null) {
                controller.stateFinished = true;
            }*/
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

    private class ReadyToAttack : PlayerState
    {

        PlayerController controller;
        //Question marks make it nullable. Doesn't look nice though.
        //I guess by default structs are non-nullable in C#.
        Attack newAttack;
        Ability newAbility;
        Stance newStance;


        public ReadyToAttack(PlayerController controller) {
            this.controller = controller;
        }

        public void Enter() {

        }

        public void Update() {

        }

        public PlayerState HandleInput() {
            //In the end, the input string will be the attack or ability name.
            //Configured with onscreen buttons/hotkeys.
            switch (Input.inputString)
            {
                case "q":
                    newAttack = myAttacks["Ferocious Bite"];
                    return new Targeting(controller, newAttack);
                case "w":
                    newAttack = myAttacks["Tennis Ball Throw"];
                    return new Targeting(controller, newAttack);
                case "r":
                    //The ability is not implemented yet.
                    newStance = myStances["Relaxed"];
                    return null;
                case "t":
                    newStance = myStances["Alert"];
                    return null;
                case "y":
                    newStance = myStances["Threatened"];
                    return null;
                default:
                    return null;
            }
        }

        public void Exit() {

        }

    }

    private class Targeting : PlayerState
    {

        PlayerController controller;
        //need to access tileMap to make check if enemies are in range
        int range;
        Enemy target;
        Attack myAttack;
        Vector3 targetedPoint;
        RectTransform targetingReticle;


        public Targeting(PlayerController controller, Attack attack)
        {
            this.controller = controller;
            this.targetingReticle = controller.targetingReticleImage;
            this.myAttack = attack;
            this.range = attack.getRange();
        }

        public void Enter()
        {
            target = GameObject.Find("Enemy").GetComponent<Enemy>();
            //Activate targeting reticle.
        }

        public void Update()
        {
            //do all the GUI stuff here
            //reticle, etc.
            Vector3 playerPos = controller.gameObject.transform.position;
            playerPos.y = 0.01f;
            Vector3 mousePos = controller.floorPos.point;
            mousePos.y = 0.01f;
            Vector3 delta3 = mousePos - playerPos;
            if (delta3.magnitude > myAttack.getRange()) {
                delta3 = delta3.normalized * myAttack.getRange();
            }
            targetedPoint = playerPos + delta3;
            targetingReticle.gameObject.SetActive(true);
            targetingReticle.position = playerPos;
            float angle = Mathf.Atan2(delta3.z, delta3.x) * Mathf.Rad2Deg;
            //its hacky but it works!!
            targetingReticle.rotation = Quaternion.AngleAxis(angle, Vector3.down);
            targetingReticle.Rotate(90, 0, 0);
            targetingReticle.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, delta3.magnitude * 1000f);
        }

        public PlayerState HandleInput()
        {
            //TODO: check if target is inRange and valid
            if (Input.GetMouseButtonDown(0))
            {
                //TileCoords playerCoords = controller.playerCurrentTile.getCoords();
                Tile targetTile = controller.tileMap.getTile(targetedPoint);
                Enemy target = GameManager.instance.getEnemyOnTile(targetTile);
                if (target != null) {
                    controller.dealDamage(target, myAttack.getDamage());
                    controller.myActionPoints -= 1;
                    return new WaitingForInput(controller);
                }
            }
            else {
                return null;
            }
            if (Input.GetKeyDown(KeyCode.Escape)) {
                return new WaitingForInput(controller);
            }
            return null;
        }

        public void Exit()
        {
            //Deactivate targeting reticle.
        }

    }

    /*private class Attacking : PlayerState
    {

        PlayerController controller;
        //need to access tileMap to make check if enemies are in range
        TileMap tileMap;

        public Attacking(PlayerController controller)
        {
            this.controller = controller;
            this.tileMap = controller.tileMap;
        }

        public void Enter()
        {

        }

        public void Update()
        {

        }

        public PlayerState HandleInput()
        {
            return null;
        }

        public void Exit()
        {

        }

    }*/

    private class WaitingForInput : PlayerState
    {
        PlayerController controller;

        public WaitingForInput(PlayerController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            /*Since each state returns to WaitingForInput when its finished,
            we'll just check if the turn is over and end it in here. */
            if (controller.myActionPoints == 0) {
                GameManager.instance.playerTurn = false;
            }
        }

        public void Exit()
        {
        }

        public PlayerState HandleInput()
        {
            //Define a new combat state, it will be accessible from here.
            //We need move limits but also "action points" to determine when 
            //to end the turn. 
            //Tentative attack points: 3
            //Two moves and 1 attack
            //Or 1 move and 2 attacks
            //How to deal with stances?
            //Just have reference to current stance, and call
            //stance.takeDamage(attackBaseDamage);
            //stance.dealDamage(attackBaseDamage);
            //dealing damage done through GameManager?
            //dealDamage(amount, enemy); ? 
            //basically, does attacking enemy get to call player.takeDamage()
            //or should it go through an intermediary? Either the game manager or
            //a combat handler
            if (Input.GetKeyDown(KeyCode.M))
            {
                return new ReadyToMove(controller.tileMap, controller);
            }
            if (Input.GetKeyDown(KeyCode.A)) {
                Debug.Log("does it get here?");
                return new ReadyToAttack(controller);
            }
            return null;
        }

        public void Update()
        {
        }
    }


}