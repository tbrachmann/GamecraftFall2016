using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class FloorTile : Tile
{
	/*public FloorTile() : base(Tile.TileType.Floor)
	{}*/

	public FloorTile(TileCoords coords) : base(coords, Tile.TileType.Floor)
	{}

}