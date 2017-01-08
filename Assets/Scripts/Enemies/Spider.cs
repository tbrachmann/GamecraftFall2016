using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : Enemy
{
    public override void dealDamage(Combatable target, float damage)
    {
        bool trait = false;
        if (trait)
        {
            damage = 40;
        }
        target.takeDamage(damage);
    }

    public override void takeDamage(float damage)
    {
        health -= damage;
    }

    //Roshan: implement AI here.
    protected override PlayerState getBestMove()
    {
        TileCoords playerCoords = player.playerCurrentTile;
        TileCoords targetCoords = myCurrentTile;
        float dx = Mathf.Abs(playerCoords.x - targetCoords.x);
        float dy = Mathf.Abs(playerCoords.z - targetCoords.z);
        int manhattanDistance = Mathf.FloorToInt(dx + dy);
        if (manhattanDistance > 1 && manhattanDistance < 5)
        {
            return new EnemyMoving(tileMap, this);
        }
        else if (manhattanDistance >= 5)
        {
            return new Attacking(player, myAttack, this);
            //TODO: Redesign player trap. The enemy should not be able to freeze-spam.
        }
        else
        {
            return new Attacking(player, myAttack, this);
        }
    }

    // Use this for initialization
    protected override void Start () {
        //TODO: Implement player trap or equivalent
        this.myAttack = new Attack(36, 1, "Venomous Bite");
        this.health = 30;
        this.moveLimit = 2;
        this.actionPoints = 2;
        base.Start();
    }
	
}
