using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;


public class Tile
{
    //enum my tile type
    //maybe enums should go into the same file
    protected bool traversable = true;
    //my movement cost
    protected int movementCost = 1;
    protected int healthCost = 0;

    public Tile() {
        
    }

    public virtual bool isTraversable() {
        return traversable;
    }
}
