using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace DungeonGeneration
{

// We have 2 tilemaps here:
// On the farthest bottom layer is the collisionTilemap. This holds invisible tiles so that the TilemapCollider can be generated
// The layer farther up is the displayTilemap. This is offset by .5 up and to the right. 
// The tile and its displayed rotation are determined based on what tiles underneath it are colliders or not
public class DungeonRenderer : MonoBehaviour
{
	[SerializeField] MapTileList mapTileList;

	Vector2Int mapSize;
	Tilemap collisionTilemap;
	Tilemap displayTilemap;
	DungeonTerrainType[,] terrains;

	public void SetTilemaps(Tilemap c, Tilemap d)
	{
		collisionTilemap = c;
		displayTilemap = d;
	}

	public void Draw(DungeonTerrainType[,] terrains_)
	{
		terrains = terrains_;
		DrawCollisionTiles();
		DrawDisplayTiles();
	}

	private void DrawCollisionTiles()
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

		collisionTilemap.SetTiles(positions.ToArray(), invisTiles.ToArray());
	}

	private void DrawDisplayTiles()
	{
		List<TileChangeData> tcd = new();
		for (int i = 0; i < mapSize.x - 1; ++i)
		{
			for (int j = 0; j < mapSize.y - 1; ++j)
			{
				tcd.Add(GetTileDataAtIndex(i, j));
			}
		}
		displayTilemap.SetTiles(tcd.ToArray(), true);
	}

	private bool TerrainShouldHaveCollider(DungeonTerrainType t)
	{
		return t == DungeonTerrainType.Rock
				|| t == DungeonTerrainType.RoomOutline;
	}

#nullable enable
	private Tile? GetTile(int x, int y)
	{
		int colliderCount = 0;
		if (TerrainShouldHaveCollider(terrains[x,y])) ++colliderCount;
		if (TerrainShouldHaveCollider(terrains[x,y+1])) ++colliderCount;
		if (TerrainShouldHaveCollider(terrains[x+1,y])) ++colliderCount;
		if (TerrainShouldHaveCollider(terrains[x+1,y+1])) ++colliderCount;

		if (colliderCount == 0)
		{
			return mapTileList.PickFloor();
		}
		if (colliderCount == 1)
		{
			return mapTileList.PickCornerOut();
		}
		if (colliderCount == 2)
		{
			return mapTileList.PickWall();
		}
		if (colliderCount == 3)
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
}

} // namespace DungeonGeneration

