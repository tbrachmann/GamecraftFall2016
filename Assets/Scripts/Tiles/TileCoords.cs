using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public struct TileCoords : IEquatable<TileCoords>
{
    public TileCoords(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
    public int x;
    public int z;

    public TileCoords(Vector3 pos) {
        this.x = Mathf.FloorToInt(pos.x);
        this.z = Mathf.FloorToInt(pos.z);
    }

    public void update(Vector3 pos) {
        this.x = Mathf.FloorToInt(pos.x);
        this.z = Mathf.FloorToInt(pos.z);
    }

    public static TileCoords operator +(TileCoords a, TileCoords b)
    {
        return new TileCoords(a.x + b.x, a.z + b.z);
    }

    public static TileCoords operator -(TileCoords a, TileCoords b)
    {
        return new TileCoords(a.x - b.x, a.z - b.z);
    }

    public static bool operator ==(TileCoords a, TileCoords b) {
        return a.Equals(b);
    }

    public static bool operator !=(TileCoords a, TileCoords b) {
        return !(a.Equals(b));
    }

    public static TileCoords right = new TileCoords(1, 0);
    public static TileCoords left = new TileCoords(-1, 0);
    public static TileCoords forward = new TileCoords(0, 1);
    public static TileCoords back = new TileCoords(0, -1);

    //add a getHashCode()
    public override string ToString() {
        return "(" + this.x + ", " + this.z + ")";
    }

    public bool Equals(TileCoords other)
    {
        return this.x == other.x && this.z == other.z;
    }

    public override bool Equals(System.Object obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (obj.GetType().IsInstanceOfType(this))
        {
            return Equals((TileCoords)obj);
        }
        else {
            return false;
        }
    }

    public override int GetHashCode()
    {
        int result = this.x;
        result = 31 * result + this.z;
        return result;
    }


}
