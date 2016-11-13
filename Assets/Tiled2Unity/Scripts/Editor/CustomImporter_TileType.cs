using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Tiled2Unity.CustomTiledImporter]
public class CustomImporter_TileType : Tiled2Unity.ICustomTiledImporter {

	public void HandleCustomProperties(GameObject gameObject,
        IDictionary<string, string> customProperties)
    {
        //Tiled2Unity.TiledMap myTiledMap = (Tiled2Unity.TiledMap) gameObject.transform.root.gameObject.AddComponent <Tiled2Unity.TiledMap>();
        //gameObject.transform.root.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        if (gameObject.transform.root.gameObject.GetComponent<TileHolder>() == null){
            gameObject.transform.root.gameObject.AddComponent<TileHolder>();
        }
        TileHolder myTiles = (TileHolder) gameObject.transform.root.gameObject.GetComponent<TileHolder>();
        if (myTiles == null) {
            myTiles.myTiles = new Dictionary<TileCoords, Tile>();
        }
        for(int i = 0; i < gameObject.transform.childCount; i++) {
            GameObject thisTile = gameObject.transform.GetChild(i).gameObject;
            if (customProperties.ContainsKey("Type")) {
                //MyTileMap myTileMap = (MyTileMap) gameObject.transform.root.gameObject.GetComponent("MyTileMap");
                //Tiled2Unity.TiledMap myMap = 
                    //(Tiled2Unity.TiledMap) gameObject.transform.root.gameObject.GetComponent<Tiled2Unity.TiledMap>();
                string stringType = customProperties["Type"];
                Vector3 myPos = thisTile.transform.position;
                TileCoords coords = new TileCoords(Mathf.FloorToInt(myPos.x), Mathf.FloorToInt(myPos.y));
                Tile.TileType myType;
                Debug.Log("pos: " + myPos + " coords: " + coords);
                /*if(stringType == "Floor") {
                    myType = Tile.TileType.Floor;
                } else {
                    myType = Tile.TileType.Obstacle;
                }*/
                switch(stringType){
                    case "Obstacle":
                        myType = Tile.TileType.Obstacle;
                        myTiles.addTile(new Obstacle(coords));
                        break;
                    default:
                        myType = Tile.TileType.Floor;
                        myTiles.addTile(new FloorTile(coords));
                        break;
                }
                /*if(myType == Tile.TileType.Floor) {
                    //myTileMap.addTile(new FloorTile(coords));
                } else {
                    //myTileMap.addTile(new Obstacle(coords));
                }*/
                //GameObject.DestroyImmediate(gameObject);
                //Debug.Log("added a tile with type: " + stringType);
                //myCounter.myCounter++;
                
                // Add the terrain tile game object
                /*StrategyTile tile = gameObject.AddComponent&amp;lt;StrategyTile&amp;gt;();
                tile.TileType = customProperties["Terrain"];
                tile.TileNote = customProperties["Note"];*/
            }
        }
        /*for(int i = 0; i < gameObject.transform.childCount; i++) {
            //GameObject thisTile = gameObject.transform.GetChild(i).gameObject;
            Object.DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
        }*/
        Debug.Log(myTiles.getNumTiles());
        
    }
 
    public void CustomizePrefab(GameObject prefab)
    {
        //GameObject floorTile = prefab.transform.GetChild(2).gameObject;
        //GameObject obstacleTile = prefab.transform.GetChild(3).gameObject;
        //Object.DestroyImmediate(floorTile);
        //Object.DestroyImmediate(obstacleTile);
        //UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(prefab, "Assets/Tiled2Unity/Scripts/Editor/CustomImporter_TileType.cs (39,9)", "MyTileMap.cs");
        //prefab.AddComponent<TileCounter>();
    }

}
