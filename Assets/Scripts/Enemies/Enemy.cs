using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, Combatable {

    protected PlayerController player;
    protected TileMap tileMap;
    protected PlayerState myState = null;
    protected Tile myCurrentTile;
    public bool myTurn = false;
    protected bool stateFinished = false;
    protected float health;
    protected int moveLimit;
    protected int actionPoints;
    protected Attack myAttack;
    protected int biteDuration;
    protected bool damageOverTime;

    //also need to add my attacks

    // Use this for initialization
    protected virtual void Start () {
        this.player = GameManager.instance.player;
        this.tileMap = GameManager.instance.tileMap;
    }
	
	// Update is called once per frame
	protected void Update () {
        myCurrentTile = tileMap.getTile(gameObject.transform.position);
        if (GameManager.instance.playerTurn)
        {
            actionPoints = 2;
            return;
        }
        else if (myState == null)
        {
            myState = getBestMove();
            myState.Enter();
        }
        myState.Update();
        if (stateFinished) {
            //myState.Exit();
            myState = null;
            if (actionPoints == 0) {
                //Debug.Log("My action points:" + actionPoints);
                GameManager.instance.playerTurn = true;
            }
            stateFinished = false;
        }
	}

    //Implemented by each class to do the basic AI.
    protected abstract PlayerState getBestMove();

    public abstract void dealDamage(Combatable target, float damage);

    public abstract void takeDamage(float damage);

    protected class EnemyMoving : Movable, PlayerState
    {
        Enemy controller;
        Transform enemy;
        Transform player;
        LinkedList<Tile> path;
        LinkedListNode<Tile> nextTile;
        int moveLimit;

        public EnemyMoving(TileMap tileMap, Enemy controller) : base(tileMap)
        {
            this.moveLimit = controller.moveLimit;
            this.controller = controller;
            this.player = controller.player.transform;
            this.enemy = controller.transform.parent;
        }

        public void Enter()
        {
            Tile playerTile = tileMap.getTile(player.position);
            path = FindShortestPath(controller.myCurrentTile, playerTile);
            /*foreach (Tile t in path) {
                Debug.Log(t.getCoords());
            }*/
            nextTile = path.First;
            controller.actionPoints -= 1;
        }

        public void Exit()
        {
            
        }

        public PlayerState HandleInput()
        {
            return null;
        }

        public void Update()
        {
            Vector3 nextTilePos = nextTile.Value.coordsToVector3();
            if (nextTilePos == tileMap.getTile(player.position).coordsToVector3()) {
                controller.stateFinished = true;
                nextTile = null;
            }
            if (enemy.position != nextTilePos)
            {
                enemy.position = Vector3.MoveTowards(enemy.position, nextTilePos, Time.deltaTime * 3f);
            }
            if (enemy.position == nextTilePos)
            {
                if (moveLimit == 0)
                {
                    controller.stateFinished = true;
                    //TODO: check if somebody is already on this tile
                }
                moveLimit--;
                nextTile = nextTile.Next;
            }
            if (nextTile == null)
            {
                controller.stateFinished = true;
            }
        }
    }

    //TODO: implement this. needs to carry out the attack.
    /* Having an entire state just to carry out this seems 
     * sorta dumb though. */
    protected class Attacking : PlayerState
    {
        Combatable myTarget;
        Attack myAttack;
        Enemy controller;

        public Attacking(Combatable target, Attack attack, Enemy controller) {
            this.myTarget = target;
            this.myAttack = attack;
            this.controller = controller;
        }

        public void Enter()
        {
            controller.dealDamage(myTarget, myAttack.getDamage());
            controller.stateFinished = true;
            controller.actionPoints -= 1;
        }

        public void Exit()
        {

        }

        public PlayerState HandleInput()
        {
            return null;
        }

        public void Update()
        {

        }
    }

}
