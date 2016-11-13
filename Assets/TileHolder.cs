using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHolder : MonoBehaviour {

	public int size_x = 5;
	public int size_z = 10;
	public float tileSize = 1.0f;
    //Tiled2Unity.TiledMap otherTileMap;
    public Dictionary<TileCoords, Tile> myTiles;
    //public int tileCount;

    public void Start() {
        //Debug.Log("when does start get called");
        //myTiles 
        //tileCount = myTiles.Count;
    }

    public int getNumTiles() {
        return myTiles.Count;
    }

	public void addTile(Tile tile) {
		Debug.Log("trying to add tile");
		if(!myTiles.ContainsKey(tile.getCoords())) {
			Debug.Log("added tile at: " + tile.coordsToVector3());
			//tileCount++;
			myTiles.Add(tile.getCoords(), tile);
		}
	}

	public Tile getTile(TileCoords coords) {
        if(!myTiles.ContainsKey(coords)){
        	return null;
        }
        return myTiles[coords];
    }

    //tileMap can only interface with TileCoords?
    //outside static method to convert to tileCoords?
	public Tile getTile(Vector3 vector){
		TileCoords coords = new TileCoords(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.z));
        //print(coords.x + ", " +  coords.z);
        Debug.Log(myTiles);
        if(!myTiles.ContainsKey(coords)){
        	return null;
        }
		return myTiles[coords];
	}

}