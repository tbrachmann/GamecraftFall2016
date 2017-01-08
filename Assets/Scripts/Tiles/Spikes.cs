using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class Spikes : Tile
{
    public Spikes()
    {
        this.traversable = true;
        this.healthCost = 15;
        this.movementCost = 1;
    }
}
