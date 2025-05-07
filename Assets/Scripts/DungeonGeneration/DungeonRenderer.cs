using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace DungeonGeneration
{
public class DungeonRenderer : MonoBehaviour
{
	[SerializeField] MapTileList mapTileList;

	Vector2Int mapSize;
	DungeonTerrainType[,] terrains;
	public void Draw(DungeonTerrainType[,] terrains_, Tilemap collisionMap, Tilemap displayMap)
	{
		// Padding
		mapSize = new Vector2Int(terrains_.GetLength(0) + 2, terrains_.GetLength(1) + 2);
		terrains = new DungeonTerrainType[mapSize.x, mapSize.y];
		for (int i = 0; i < terrains_.GetLength(0); ++i)
		{
			for (int j = 0; j < terrains_.GetLength(1); ++j)
			{
				terrains[i+1,j+1] = terrains_[i,j];
			}
		}
		for (int i = 0; i < mapSize.x; ++i)
		{
			terrains[i,0] = DungeonTerrainType.Rock;
			terrains[i,mapSize.y-1] = DungeonTerrainType.Rock;
		}
		for (int i = 0; i < mapSize.y; ++i)
		{
			terrains[0,i] = DungeonTerrainType.Rock;
			terrains[mapSize.x-1,i] = DungeonTerrainType.Rock;
		}

		DrawCollisionTiles(collisionMap);
		DrawDisplayTiles(displayMap);
	}

	private void DrawCollisionTiles(Tilemap collisionMap)
	{
		List<Vector3Int> positions = new List<Vector3Int>();
		List<Tile> invisTiles = new List<Tile>();

		for (int i = 0; i < mapSize.x; ++i)
		{
			for (int j = 0; j < mapSize.y; ++j)
			{
				if (TerrainShouldHaveCollider(terrains[i,j]))
				{
					positions.Add(new (i - 1, j - 1, 0));
					invisTiles.Add(mapTileList.invisible);
				}
			}
		}

		collisionMap.SetTiles(positions.ToArray(), invisTiles.ToArray());
	}

	private bool TerrainShouldHaveCollider(DungeonTerrainType t)
	{
		return t == DungeonTerrainType.Rock
				|| t == DungeonTerrainType.RoomOutline;
	}

#nullable enable
	private Tile? GetTile(int x, int y)
	{
		int count = 0;
		if (TerrainShouldHaveCollider(terrains[x,y])) ++count;
		if (TerrainShouldHaveCollider(terrains[x,y+1])) ++count;
		if (TerrainShouldHaveCollider(terrains[x+1,y])) ++count;
		if (TerrainShouldHaveCollider(terrains[x+1,y+1])) ++count;

		if (count == 0)
		{
			return mapTileList.PickFloor();
		}
		if (count == 1)
		{
			return mapTileList.PickCornerOut();
		}
		if (count == 2)
		{
			return mapTileList.PickWall();
		}
		if (count == 3)
		{
			return mapTileList.PickCornerIn();
		}
		return null;
	}
#nullable disable

	private Matrix4x4 CornerOutTransform(int x, int y)
	{
			if (TerrainShouldHaveCollider(terrains[x,y]))
				return RotationMatrices.Identity;
			if (TerrainShouldHaveCollider(terrains[x+1,y]))
				return RotationMatrices.Rotate90;
			if (TerrainShouldHaveCollider(terrains[x+1,y+1]))
				return RotationMatrices.Rotate180;
			return RotationMatrices.Rotate270;
	}
	private Matrix4x4 CornerInTransform(int x, int y)
	{
			if (!TerrainShouldHaveCollider(terrains[x,y]))
				return RotationMatrices.Identity;
			if (!TerrainShouldHaveCollider(terrains[x+1,y]))
				return RotationMatrices.Rotate90;
			if (!TerrainShouldHaveCollider(terrains[x+1,y+1]))
				return RotationMatrices.Rotate180;
			return RotationMatrices.Rotate270;
	}
	private Matrix4x4 WallTransform(int x, int y)
	{
		if (TerrainShouldHaveCollider(terrains[x,y]))
		{
			if (TerrainShouldHaveCollider(terrains[x+1,y]))
				return RotationMatrices.Rotate180;
			return RotationMatrices.Rotate90;
		}
		if (!TerrainShouldHaveCollider(terrains[x+1,y]))
			return RotationMatrices.Identity;
		return RotationMatrices.Rotate270;
	}

	private Matrix4x4 GetTransform(int x, int y, Tile t)
	{
		if (mapTileList.cornerOuts.Contains(t))
			return CornerOutTransform(x, y);
		else if (mapTileList.cornerIns.Contains(t))
			return CornerInTransform(x, y);
		else if (mapTileList.walls.Contains(t))
			return WallTransform(x, y);

		return RotationMatrices.Identity;
	}

	private TileChangeData GetTileDataAtIndex(int x, int y)
	{
		Tile t = GetTile(x, y);
		return new TileChangeData{tile = t,
			position = new Vector3Int(x-1, y-1, 0),
			color = ((t == mapTileList.floors[0]) ? Color.black : Color.white),
			transform = GetTransform(x, y, t)};
	}

	private void DrawDisplayTiles(Tilemap displayMap)
	{
		List<TileChangeData> tcd = new();
		for (int i = 0; i < mapSize.x - 1; ++i)
		{
			for (int j = 0; j < mapSize.y - 1; ++j)
			{
				tcd.Add(GetTileDataAtIndex(i, j));
			}
		}
		displayMap.SetTiles(tcd.ToArray(), true);
	}


}

} // namespace DungeonGeneration

