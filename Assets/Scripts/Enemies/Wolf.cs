using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Enemy {

    //Overrided to initialize the variables.
    protected override void Start()
    {
        this.myAttack = new Attack(20, 1, "Ferocious Bite");
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
        bool trait = false;
        TileCoords targetCoords = myCurrentTile.getCoords();
        GameObject[] neighborWolf = GameObject.FindGameObjectsWithTag("Wolf");
        if (neighborWolf.Length > 0)
        {
            foreach(GameObject wolf in neighborWolf)
            {
                TileCoords wolfCoords = tileMap.getTile(wolf.transform.position).getCoords();
                float dx = Mathf.Abs(wolfCoords.x - targetCoords.x);
                float dy = Mathf.Abs(wolfCoords.z - targetCoords.z);
                int manhattanDistance = Mathf.FloorToInt(dx + dy);
                if (manhattanDistance <= 4)
                {
                    trait = true;
                }
            }
        }
        if (trait)
        {
            damage = damage * 1.2f;
        }
        target.takeDamage(damage);
    }

    public override void takeDamage(float damage)
    {
        Debug.Log("Enemy took damage of " + damage + "!");
        health -= damage;
    }
}
