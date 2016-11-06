using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TileMap : MonoBehaviour {

	public int size_x = 10;
	public int size_z = 5;
	public float tileSize = 1.0f;
    int tileResolution = 512;
    public Texture2D myTexture;
    public Texture2D myNormalMap;
    Color[] tileTexture;
    Color[] tileNormalMap;
	Dictionary<TileCoords, Tile> myTiles = new Dictionary<TileCoords, Tile>();

	// Use this for initialization
	void Start () {
        //print("started!");
        tileTexture = myTexture.GetPixels(0, 0, tileResolution, tileResolution);
        tileNormalMap = myNormalMap.GetPixels(0, 0, tileResolution, tileResolution);
		BuildMesh();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void BuildMesh() {
		int numTiles = size_x * size_z;
		//2 triangles go into each rectangular tile
		int numTriangles = numTiles * 2;
		
		int vsize_x = size_x + 1;
		int vsize_z = size_z + 1;
		int numVertices = vsize_x * vsize_z;

		Vector3[] vertices = new Vector3[numVertices];
		Vector3[] normals = new Vector3[numVertices];
		Vector2[] uv = new Vector2[numVertices];
		int[] triangles = new int[numTriangles * 3];

		//assign vertices
        //iterate by vertex
		int x, z;
		for(z=0; z < vsize_z; z++) {
			for(x=0; x < vsize_x; x++) {
				vertices[ z * vsize_x + x ] = new Vector3( x*tileSize, 0, z*tileSize );
				normals[ z * vsize_x + x ] = Vector3.up;
				uv[ z * vsize_x + x ] = new Vector2( (float)x / vsize_x, (float)z / vsize_z );
			}
		}
        //Debug.Log ("Done Verts!");
        //iterate by tile
        int texWidth = size_x * tileResolution;
        int texHeight = size_z * tileResolution;
        Texture2D texture = new Texture2D(texWidth, texHeight);
        Texture2D normalMap = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);

        for (z=0; z < size_z; z++) {
			for(x=0; x < size_x; x++) {
				int squareIndex = z * size_x + x;
				int triOffset = squareIndex * 6;
				triangles[triOffset + 0] = z * vsize_x + x + 		   0;
				triangles[triOffset + 1] = z * vsize_x + x + vsize_x + 0;
				triangles[triOffset + 2] = z * vsize_x + x + vsize_x + 1;
				
				triangles[triOffset + 3] = z * vsize_x + x + 		   0;
				triangles[triOffset + 4] = z * vsize_x + x + vsize_x + 1;
				triangles[triOffset + 5] = z * vsize_x + x + 		   1;
				TileCoords newCoords = new TileCoords(x, z);
				myTiles.Add(newCoords, new FloorTile(newCoords));
                texture.SetPixels(x * tileResolution, z * tileResolution, tileResolution, tileResolution, tileTexture);
                normalMap.SetPixels(x * tileResolution, z * tileResolution, tileResolution, tileResolution, tileNormalMap);
            }
		}
        //Go over normal map a second time
        /*Color theColor = new Color();
        for (x = 0; x < normalMap.width; x++) {
            for (int y = 0; y < normalMap.height; y++) {
                theColor.r = normalMap.GetPixel(x, y).g;
                theColor.g = theColor.r;
                theColor.b = theColor.r;
                theColor.a = normalMap.GetPixel(x, y).r;
                normalMap.SetPixel(x, y, theColor);
            }
        }*/

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uv;

		MeshFilter mesh_filter = GetComponent<MeshFilter>();
		MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
		MeshCollider mesh_collider = GetComponent<MeshCollider>();

		mesh_filter.mesh = mesh;
		mesh_collider.sharedMesh = mesh;

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        normalMap.Apply();
        mesh_renderer.sharedMaterials[0].mainTexture = texture;
        mesh_renderer.sharedMaterials[0].EnableKeyword("_NORMALMAP");
        mesh_renderer.sharedMaterials[0].SetTexture("_BumpMap", normalMap);
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
        if(!myTiles.ContainsKey(coords)){
        	return null;
        }
		return myTiles[coords];
	}

}
