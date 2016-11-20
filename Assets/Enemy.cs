using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    GameObject player;
    TileMap tileMap;
    PlayerState myState = null;
    Tile myCurrentTile;
    bool stateFinished = false;

	// Use this for initialization
	void Start () {
        player = GameManager.instance.player;
        tileMap = GameManager.instance.tileMap;
    }
	
	// Update is called once per frame
	void Update () {
        myCurrentTile = tileMap.getTile(gameObject.transform.position);
        if (GameManager.instance.playerTurn)
        {
            return;
        }
        else if (myState == null)
        {
            myState = new EnemyMoving(tileMap, this);
            myState.Enter();
        }
        myState.Update();
        if (stateFinished) {
            //myState.Exit();
            myState = null;
            GameManager.instance.playerTurn = true;
            stateFinished = false;
        }
	}

    private class EnemyMoving : Movable, PlayerState
    {
        Enemy controller;
        Transform enemy;
        Transform player;
        LinkedList<Tile> path;
        LinkedListNode<Tile> nextTile;
        int moveLimit;

        public EnemyMoving(TileMap tileMap, Enemy controller) : base(tileMap)
        {
            moveLimit = 3;
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
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public PlayerState HandleInput()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            Vector3 nextTilePos = nextTile.Value.coordsToVector3();
            if (enemy.position != nextTilePos)
            {
                enemy.position = Vector3.MoveTowards(enemy.position, nextTilePos, Time.deltaTime * 3f);
            }
            if (enemy.position == nextTilePos)
            {
                if (moveLimit == 0) {
                    controller.stateFinished = true;
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

}
