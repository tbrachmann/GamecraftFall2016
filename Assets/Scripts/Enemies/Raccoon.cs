using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raccoon : Enemy
{
    public override void dealDamage(Combatable target, float damage)
    {
        throw new NotImplementedException();
    }

    public override void takeDamage(float damage)
    {
        throw new NotImplementedException();
    }

    //@Roshan: implement AI here.
    protected override PlayerState getBestMove()
    {
        throw new NotImplementedException();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
