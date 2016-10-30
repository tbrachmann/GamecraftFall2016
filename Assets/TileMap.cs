using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TileMap : MonoBehaviour {

	public int size_x = 10;
	public int size_z = 5;
	public float tileSize = 1.0f;
	Dictionary<TileCoords, Tile> myTiles = new Dictionary<TileCoords, Tile>();

	// Use this for initialization
	void Start () {
		//print("started!");
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
		int x, z;
		for(z=0; z < vsize_z; z++) {
			for(x=0; x < vsize_x; x++) {
				vertices[ z * vsize_x + x ] = new Vector3( x*tileSize, 0, z*tileSize );
				normals[ z * vsize_x + x ] = Vector3.up;
				uv[ z * vsize_x + x ] = new Vector2( (float)x / vsize_x, (float)z / vsize_z );
			}
		}
		//Debug.Log ("Done Verts!");
		
		for(z=0; z < size_z; z++) {
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
				if(x % 2 != 0) {
					myTiles.Add(newCoords, new Obstacle(newCoords));
				} else {
					myTiles.Add(newCoords, new FloorTile(newCoords));
				}
			}
		}
		/*for(x=0; x < vsize_x; x++){
			for(z=0; z < vsize_z; z++){
				vertices[x * vsize_z + z] = new Vector3(x*tileSize, 0, z*tileSize);
				normals[x * vsize_z + z] = Vector3.up;
				uv[x * vsize_z + z] = new Vector2((float) x / vsize_x, (float) z / vsize_z);
			}
		}

		//assign triangles
		for(x=0; x < size_x; x++){
			for(z=0; z < size_z; z++){
				int squareIndex = x * size_z + z;
				int triOffset = squareIndex * 6;
				triangles[triOffset + 0] = x * vsize_z + z + 0;
				triangles[triOffset + 1] = x * vsize_z + z + vsize_z + 0;
				triangles[triOffset + 2] = x * vsize_z + z + 1;

				triangles[triOffset + 3] = triangles[triOffset + 1];
				triangles[triOffset + 4] = x * vsize_z + z + vsize_z + 1;
				triangles[triOffset + 5] = triangles[triOffset + 2];
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
		return myTiles[coords];
	}

}
