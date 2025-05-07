using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

using System.Collections.Generic;

namespace DungeonGeneration
{

// Room is defined by a rect that has a ring of padding
public class DungeonRoom
{
	public RectInt rect { get; private set; }
	bool isSpawnRoom;

	public DungeonRoom(Vector2Int mapSize, Vector2Int fixedRoomDims, bool isSpawn, bool placeRandom = true)
	{
		fixedRoomDims = fixedRoomDims + 2 * Vector2Int.one;
		int leftPointX = (isSpawn) ? -1 : mapSize.x - fixedRoomDims.x + 1;
		int leftPointY = (isSpawn) ? -1 : mapSize.y - fixedRoomDims.y + 1;
		if (placeRandom)
		{
			leftPointX = UnityEngine.Random.Range(-1, mapSize.x - fixedRoomDims.x + 1);
			leftPointY = UnityEngine.Random.Range(-1, mapSize.y - fixedRoomDims.y + 1);
		}
		Vector2Int randomLowerLeftPoint = new (leftPointX, leftPointY);
		
		rect = new RectInt(randomLowerLeftPoint, fixedRoomDims);
		isSpawnRoom = isSpawn;
	}

	// Generates a random room given params, padded by a ring of walls
	public DungeonRoom(Vector2Int mapSize, RoomSizeParametersWithPadding rsp)
	{
		int leftPointX = UnityEngine.Random.Range(-1, mapSize.x - rsp.minWidth + 1);
		int leftPointY = UnityEngine.Random.Range(-1, mapSize.y - rsp.minHeight + 1);
		Vector2Int randomLowerLeftPoint = new (leftPointX, leftPointY);

		int maxPossibleSizeX = Mathf.Min(rsp.maxWidth, mapSize.x - leftPointX + 1);
		int maxPossibleSizeY = Mathf.Min(rsp.maxHeight, mapSize.y - leftPointY + 1);
		int sizeX = UnityEngine.Random.Range(rsp.minWidth, maxPossibleSizeX);
		int sizeY = UnityEngine.Random.Range(rsp.minWidth, maxPossibleSizeY);
		Vector2Int randomSize = new (sizeX, sizeY);

		rect = new RectInt(randomLowerLeftPoint, randomSize);
		isSpawnRoom = false;
	}

	public Vector2Int ShopTilePosition()
	{
		return new(UnityEngine.Random.Range(rect.xMin + 1, rect.xMax - 1), rect.yMax - 1);
	}

	public Vector2Int RandomPointInside()
	{
		return new(UnityEngine.Random.Range(rect.xMin + 1, rect.xMax - 1), UnityEngine.Random.Range(rect.yMin + 1, rect.yMax - 1));
	}

	public void PopulateBossRoom()
	{
		if (isSpawnRoom)
			Debug.LogWarning("Populating spawn room as boss room");
	}
};

} // namespace DungeonGeneration

