using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Enemy {

    //Overrided to initialize the variables.
    protected override void Start()
    {
        this.myAttack = new Attack(10, 1, "Bite");
        this.health = 50;
        this.moveLimit = 2;
        this.actionPoints = 2;
        base.Start();
    }

    //@Roshan: implement AI here.
    protected override PlayerState getBestMove()
    {
        TileCoords playerCoords = player.playerCurrentTile.getCoords();
        TileCoords targetCoords = myCurrentTile.getCoords();
        float dx = Mathf.Abs(playerCoords.x - targetCoords.x);
        float dy = Mathf.Abs(playerCoords.z - targetCoords.z);
        int manhattanDistance = Mathf.FloorToInt(dx + dy);
        /* If player is not within one square then move towards enemy until
         * you can attack. */
        if (manhattanDistance > 1)
        {
            return new EnemyMoving(tileMap, this);
        }
        else {
            return new Attacking(player, myAttack, this);
        }
    }

    public override void dealDamage(Combatable target, float damage)
    {
        Debug.Log("this gets called");
        target.takeDamage(damage);
    }

    public override void takeDamage(float damage)
    {
        health -= damage;
    }
}
