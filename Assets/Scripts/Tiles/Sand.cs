using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class Sand : Tile
{
    public Sand() {
        this.traversable = true;
        this.healthCost = 0;
        this.movementCost = 2;
    }
}
