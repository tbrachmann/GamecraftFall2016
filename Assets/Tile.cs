using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

[Serializable]
public abstract class Tile : IEquatable<Tile>
{
    //enum my tile type
    //maybe enums should go into the same file?
    public enum TileType {Sand, Rock, Grass, Water, Obstacle, Floor};
    protected TileType myType;
    protected TileCoords myCoords;
    protected bool traversable = true;

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

    public virtual Tile.TileType getType(){
        return myType;
    }

    public virtual bool isTraversable() {
        return traversable;
    }

    public virtual Vector3 coordsToVector3(){
        return new Vector3(myCoords.x, 0, myCoords.z);
    }

    public bool Equals(Tile other){
        if(other == null){
            return false;
        }
        Tile.TileType otherType = other.getType();
        TileCoords otherCoords = other.getCoords();
        return myCoords.x == otherCoords.x && myCoords.z == otherCoords.z && myType == otherType;
    }

    public override bool Equals(System.Object obj){
        if(obj == null) {
            return false;
        }
        Tile tileObj = obj as Tile;
        if(tileObj == null){
            return false;
        } else {
            return Equals(tileObj);
        }
    }

    public override int GetHashCode(){
        int result = myCoords.x;
        result = 31 * result + myCoords.z;
        return result;
    }


    //my center coords
    //my movement cost
    //IF you can save neighbors, that means that you must generate tile map in script beforehand
    //my neighbors - left right forward backwards
}
