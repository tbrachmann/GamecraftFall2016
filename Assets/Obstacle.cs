using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class Obstacle : Tile
{
	/*public FloorTile() : base(Tile.TileType.Floor)
	{}*/

	public Obstacle(TileCoords coords) : base(coords, Tile.TileType.Obstacle)
	{
		this.traversable = false; 
	}

}