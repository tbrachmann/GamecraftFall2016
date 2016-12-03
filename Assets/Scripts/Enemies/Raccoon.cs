using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raccoon : Enemy
{
    public override void dealDamage(Combatable target, float damage)
    {
        
        target.takeDamage(damage);
    }

    public override void takeDamage(float damage)
    {
        health -= damage;
    }

    //@Roshan: implement AI here.
    protected override PlayerState getBestMove()
    {
        TileCoords playerCoords = player.playerCurrentTile.getCoords();
        TileCoords targetCoords = myCurrentTile.getCoords();
        float dx = Mathf.Abs(playerCoords.x - targetCoords.x);
        float dy = Mathf.Abs(playerCoords.z - targetCoords.z);
        int manhattanDistance = Mathf.FloorToInt(dx + dy);
        bool trait = false;
        GameObject[] neighborRaccoon = GameObject.FindGameObjectsWithTag("Raccoon");
        if (neighborRaccoon.Length > 0)
        {
            foreach(GameObject raccoon in neighborRaccoon)
            {
                TileCoords raccoonCoords = tileMap.getTile(raccoon.transform.position).getCoords();
                float neighborDx = Mathf.Abs(raccoonCoords.x - targetCoords.x);
                float neighborDy = Mathf.Abs(raccoonCoords.z - targetCoords.z);
                int traitManhattanDistance = Mathf.FloorToInt(dx + dy);
                if (traitManhattanDistance <= 4)
                {
                    trait = true;
                }
            }
        }
        if (trait)
        {
            health += 10;
        }
        if (manhattanDistance > 1)
        {
            return new EnemyMoving(tileMap, this);
        }
        else
        {
            return new Attacking(player, myAttack, this);
        }
    }

    // Use this for initialization
    protected override void Start () {
        this.myAttack = new Attack(36, 1, "Bite");
        this.health = 40;
        this.moveLimit = 2;
        this.actionPoints = 2;
        base.Start();
    }
	
}
