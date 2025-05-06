using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

using System.Collections.Generic;

namespace DungeonGeneration
{

public class DungeonRoom
{
	public RectInt rect { get; private set; }

	// Generates a random room given params
	public DungeonRoom(Vector2Int mapSize, RoomSizeParameters roomSizeParameters)
	{
		Vector2Int randomLowerLeftPoint = new (UnityEngine.Random.Range(0, mapSize.x - roomSizeParameters.minWidth)
				, UnityEngine.Random.Range(0, mapSize.y - roomSizeParameters.minHeight));
		Vector2Int randomSize = new (UnityEngine.Random.Range(roomSizeParameters.minWidth, Mathf.Min(roomSizeParameters.maxWidth, mapSize.x - randomLowerLeftPoint.x + 1))
				, UnityEngine.Random.Range(roomSizeParameters.minWidth, Mathf.Min(roomSizeParameters.maxHeight, mapSize.y - randomLowerLeftPoint.y + 1)));

		rect = new RectInt(randomLowerLeftPoint, randomSize);
	}

	// TODO: Make Base versions of each accent and randomly select which one, biasing main tile
	TileBase GetTileForPosition(Vector3Int pos, MapTileList mapTileList)
	{
		if (pos.x == 0 || pos.x == rect.size.x - 1)
		{
			return (pos.y == rect.size.y - 1 || pos.y == 0)
				? mapTileList.cornerIn
				: mapTileList.walls[0];
		}
		if (pos.y == 0 || pos.y == rect.size.y - 1)
		{
			return mapTileList.walls[0];
		}
		return mapTileList.floors[0];
	}

	// TODO: make the Rotation matrices a static array
	Matrix4x4 GetTransformForPosition(Vector3Int pos)
	{
		if (pos.x == 0)
		{
			if (pos.y == 0)
				return Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180));
			return Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90));
		}
		if (pos.x == rect.size.x - 1)
		{
			if (pos.y < rect.size.y - 1)
				return Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270));
		}
		if (pos.y == 0)
			return Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180));
		return Matrix4x4.Rotate(Quaternion.identity);
	}
	
	// Requires this to be valid in the map (not overlapping with any other rooms)
	public void Draw(Tilemap map, MapTileList mapTileList)
	{
		List<TileChangeData> tileChangeDataArray = new List<TileChangeData>();
		for (int i = 0; i < rect.size.x; ++i)
		{
			for (int j = 0; j < rect.size.y; ++j)
			{
				Vector3Int relative_pos = new (i, j);
				Vector3Int absolute_pos = new (i + rect.x, j + rect.y);
				TileBase tile = GetTileForPosition(relative_pos, mapTileList);
				tileChangeDataArray.Add(new TileChangeData{
						position = absolute_pos,
						tile = tile,
						color = (tile == mapTileList.floors[0])
							? Color.black
							: Color.white,
						transform = GetTransformForPosition(relative_pos)});
			}
		}
		map.SetTiles(tileChangeDataArray.ToArray(), true);
	}
};

} // namespace DungeonGeneration

