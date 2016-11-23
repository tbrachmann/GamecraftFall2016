using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public struct TileCoords
{
    public TileCoords(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
    public int x;
    public int z;

    public static TileCoords operator +(TileCoords a, TileCoords b)
    {
        return new TileCoords(a.x + b.x, a.z + b.z);
    }

    public static TileCoords operator -(TileCoords a, TileCoords b)
    {
        return new TileCoords(a.x - b.x, a.z - b.z);
    }

    public static TileCoords right = new TileCoords(1, 0);
    public static TileCoords left = new TileCoords(-1, 0);
    public static TileCoords forward = new TileCoords(0, 1);
    public static TileCoords back = new TileCoords(0, -1);

    //add a getHashCode()
    public override string ToString() {
        return "(" + this.x + ", " + this.z + ")";
    }
}
