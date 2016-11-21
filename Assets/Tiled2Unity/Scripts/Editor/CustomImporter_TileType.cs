using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Tiled2Unity.CustomTiledImporter]
public class CustomImporter_TileType : Tiled2Unity.ICustomTiledImporter {

    static Dictionary<Vector3, string> myDict = new Dictionary<Vector3, string>();
    static List<GameObject> toDelete = new List<GameObject>();
    static int numTiles_x;
    static int numTiles_z;

	public void HandleCustomProperties(GameObject gameObject,
        IDictionary<string, string> customProperties)
    {
        toDelete.Add(gameObject);
        Tiled2Unity.TiledMap tileMap = (Tiled2Unity.TiledMap) gameObject.transform.root.gameObject.GetComponent<Tiled2Unity.TiledMap>();
        numTiles_x = tileMap.NumTilesHigh;
        numTiles_z = tileMap.NumTilesWide;
        for(int i = 0; i < gameObject.transform.childCount; i++) {
            GameObject thisTile = gameObject.transform.GetChild(i).gameObject;
            if (customProperties.ContainsKey("Type")) {
                string stringType = customProperties["Type"];
                Vector3 myPos = thisTile.transform.position;
                Vector3 coords = new Vector3(Mathf.Abs(Mathf.Abs(Mathf.FloorToInt(myPos.y))-(numTiles_x-1)), 0, Mathf.Abs(Mathf.Abs(Mathf.FloorToInt(myPos.x))-(numTiles_z-1)));
                Tile.TileType myType;
                if (!myDict.ContainsKey(coords))
                {
                    myDict.Add(coords, stringType);
                }
            }
        }
    }
 
    public void CustomizePrefab(GameObject prefab)
    {
        //prefab.layer = 8;
        MeshFilter myMesh = prefab.GetComponentInChildren<MeshFilter>();
        myMesh.gameObject.AddComponent<MeshCollider>().sharedMesh = myMesh.sharedMesh;
        Transform childTransform = prefab.transform.GetChild(0);
        Quaternion myQuat = new Quaternion(0, 0, 0, 0);
        myQuat.eulerAngles = new Vector3(90, 90, 0);
        childTransform.localRotation = myQuat;
        childTransform.localPosition = new Vector3(numTiles_x, 0, numTiles_z);
        for (int i = 0; i < toDelete.Count; i++) {
            Object.DestroyImmediate(toDelete[i]);
        }
        GameObject board = GameObject.Find("Board");
        TileMap tileMap = board.GetComponent<TileMap>();
        if (tileMap == null) {
            tileMap = board.AddComponent<TileMap>();
        }
        tileMap.size_x = numTiles_x;
        tileMap.size_z = numTiles_z;
        tileMap.values =  new List<string>(myDict.Values);
        tileMap.keys = new List<Vector3>(myDict.Keys);
    }

}
