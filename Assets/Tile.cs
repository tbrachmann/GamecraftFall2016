using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;


public abstract class Tile
{
    //enum my tile type
    //maybe enums should go into the same file?
    public enum TileType {Sand, Rock, Grass, Water};
    protected TileType myType;

    protected Tile(TileType type) {
        myType = type;
    }

    //my center coords
    //my movement cost
    //IF you can save neighbors, that means that you must generate tile map in script beforehand
    //my neighbors - left right forward backwards
}
