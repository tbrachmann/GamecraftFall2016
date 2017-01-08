using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TileMap : MonoBehaviour {

	public int size_x;
	public int size_z;
	public float tileSize = 1.0f;
    public List<string> values = new List<string>();
    public List<Vector3> keys = new List<Vector3>();
    public Vector3 playerStart;
    //Tiled2Unity.TiledMap otherTileMap;
    Dictionary<TileCoords, Tile> myTiles = new Dictionary<TileCoords, Tile>();
    //public int tileCount;

    void Awake() {
        //build the tileMap
        TileCoords coords;
        Tile floor = new Tile();
        Tile sand = new Sand();
        Tile water = new Water();
        Tile spikes = new Spikes();
        Tile obstacle = new Obstacle();
        for (int i = 0; i < values.Count; i++) {
            coords = new TileCoords(Mathf.FloorToInt(keys[i].x), Mathf.FloorToInt(keys[i].z));
            switch (values[i]) {
                case "Obstacle":
                    addTile(coords, obstacle);
                    break;
                case "Spikes":
                    addTile(coords, spikes);
                    break;
                case "Water":
                    addTile(coords, water);
                    break;
                case "Sand":
                    addTile(coords, sand);
                    break;
                default:
                    addTile(coords, floor);
                    break;
            }
        }
        //Debug.Log(myTiles.Count);
    }

    public int getNumTiles() {
        return myTiles.Count;
    }

	public void addTile(TileCoords coords, Tile tile) {
        //Debug.Log("trying to add tile");
		if(!myTiles.ContainsKey(coords)) {
			//Debug.Log("added tile at: " + tile.coordsToVector3());
			myTiles.Add(coords, tile);
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
	/*public  Tile getTile(Vector3 vector){
		TileCoords coords = new TileCoords(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.z));
        //print(coords.x + ", " +  coords.z);
        //Debug.Log(myTiles);
        if(!myTiles.ContainsKey(coords)){
        	return null;
        }
		return myTiles[coords];
	}*/

    //return false if unable to move Transform to Tile
    //else true
    public void setTransformToTile(Transform transform, TileCoords tile) {
        //if (!getTile(tile).isTraversable()) return false;
        transform.position = new Vector3(tile.x, 0, tile.z);
    }

    public bool moveTransformTowardsTile(Transform transform, TileCoords tile) {
        Vector3 goalVector = new Vector3(tile.x, 0, tile.z);
        if (transform.position == goalVector)
        {
            return true;
        }
        else {
            transform.position = Vector3.MoveTowards(transform.position, goalVector, Time.deltaTime * 3f);
        }
        return false;
    }

}