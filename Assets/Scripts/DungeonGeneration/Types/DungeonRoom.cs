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

	public DungeonRoom(Vector2Int mapSize, Vector2Int spawnRoomDims)
	{
		spawnRoomDims = spawnRoomDims + 2 * Vector2Int.one;
		int leftPointX = UnityEngine.Random.Range(-1, mapSize.x - spawnRoomDims.x + 1);
		int leftPointY = UnityEngine.Random.Range(-1, mapSize.y - spawnRoomDims.y + 1);
		Vector2Int randomLowerLeftPoint = new (leftPointX, leftPointY);
		
		rect = new RectInt(randomLowerLeftPoint, spawnRoomDims);
		isSpawnRoom = true;
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

	public Vector2Int RandomPointInside()
	{
		return new(UnityEngine.Random.Range(rect.xMin + 1, rect.xMax), UnityEngine.Random.Range(rect.yMin + 1, rect.yMax));
	}

	public void PopulateSpawnRoom()
	{
		if (!isSpawnRoom)
			Debug.LogWarning("Populating non-spawn room with spawn room items");

		// TODO
	}

};

} // namespace DungeonGeneration

