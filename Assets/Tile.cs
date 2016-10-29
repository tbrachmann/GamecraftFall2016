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
    public enum TileType {Sand, Rock, Grass, Water, Obstacle, Floor};
    protected TileType myType;
    protected TileCoords myCoords;

    protected Tile(TileCoords coords, TileType type) {
        myType = type;
        myCoords = coords;
    }

    /*public Tile(TileCoords coords){
        myCoords = coords;
    }*/

    public virtual TileCoords getCoords(){
        return myCoords;
    }

    public virtual Vector3 coordsToVector3(){
        return new Vector3(myCoords.x, 0, myCoords.z);
    }
    //my center coords
    //my movement cost
    //IF you can save neighbors, that means that you must generate tile map in script beforehand
    //my neighbors - left right forward backwards
}
