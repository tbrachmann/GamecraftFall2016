using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TileHolder : MonoBehaviour {

	public int size_x;
	public int size_z;
	public float tileSize = 1.0f;
    public List<string> values = new List<string>();
    public List<Vector3> keys = new List<Vector3>();
    //Tiled2Unity.TiledMap otherTileMap;
    Dictionary<TileCoords, Tile> myTiles = new Dictionary<TileCoords, Tile>();
    //public int tileCount;

    void Start() {
        TileCoords coords;
        for (int i = 0; i < values.Count; i++) {
            coords = new TileCoords(Mathf.FloorToInt(keys[i].x), Mathf.FloorToInt(keys[i].z));
            switch (values[i]) {
                case "Obstacle":
                    addTile(new Obstacle(coords));
                    break;
                default:
                    addTile(new FloorTile(coords));
                    break;
            }
        }
        //Debug.Log(myTiles.Count);
    }

    public int getNumTiles() {
        return myTiles.Count;
    }

	public void addTile(Tile tile) {
        //Debug.Log("trying to add tile");
		if(!myTiles.ContainsKey(tile.getCoords())) {
			//Debug.Log("added tile at: " + tile.coordsToVector3());
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
	public  Tile getTile(Vector3 vector){
		TileCoords coords = new TileCoords(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.z));
        //print(coords.x + ", " +  coords.z);
        //Debug.Log(myTiles);
        if(!myTiles.ContainsKey(coords)){
        	return null;
        }
		return myTiles[coords];
	}

}