using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

using System.Collections.Generic;

namespace DungeonGeneration
{

// Room is defined by a rect that has a ring of padding
// The Point we pass to the RectInt constructor is the bottom left point of the padding
// The Size is roomSize.x + 2, roomSize.y + 2
public class DungeonRoom
{
	public RectInt rect { get; private set; }

	// TODO: VisualStudio comment for these
	public DungeonRoom(Vector2Int mapSize, Vector2Int fixedRoomDims, bool isSpawn, bool placeRandom = true)
	{
		int leftPointX = (isSpawn) ? 0 : mapSize.x - fixedRoomDims.x;
		int leftPointY = (isSpawn) ? 0 : mapSize.y - fixedRoomDims.y;
		if (placeRandom)
		{
			leftPointX = UnityEngine.Random.Range(0, mapSize.x - fixedRoomDims.x);
			leftPointY = UnityEngine.Random.Range(0, mapSize.y - fixedRoomDims.y);
		}
		Vector2Int randomLowerLeftPoint = new (leftPointX, leftPointY);
		
		rect = new RectInt(randomLowerLeftPoint, fixedRoomDims);
	}

	public DungeonRoom(Vector2Int mapSize, int minWidth, int maxWidth, int minHeight, int maxHeight)
	{
		int leftPointX = UnityEngine.Random.Range(0, mapSize.x - minWidth);
		int leftPointY = UnityEngine.Random.Range(0, mapSize.y - minHeight);
		Vector2Int randomLowerLeftPoint = new (leftPointX, leftPointY);

		int maxPossibleSizeX = Mathf.Min(maxWidth, mapSize.x - leftPointX);
		int maxPossibleSizeY = Mathf.Min(maxHeight, mapSize.y - leftPointY);
		int sizeX = UnityEngine.Random.Range(minWidth, maxPossibleSizeX);
		int sizeY = UnityEngine.Random.Range(minWidth, maxPossibleSizeY);
		Vector2Int randomSize = new (sizeX, sizeY);

		rect = new RectInt(randomLowerLeftPoint, randomSize);
	}

	public Vector2Int[] SpawnRoomShopPositions()
	{
		// The 1s are here due to the rect having an implicit ring of padding
		int smallestXInRoom = rect.xMin + 1;
		int largestXInRoom = rect.xMax - 1;
		int largestYInRoom = rect.yMax - 1;

		Vector2Int[] positions = new Vector2Int[2];
		positions[0] = new Vector2Int(smallestXInRoom, largestYInRoom);
		positions[1] = new Vector2Int(largestXInRoom, largestYInRoom);
		return positions;
	}

	public Vector2Int RandomShopTilePosition()
	{
		int x = RandomXInsideRoom();
		int largestYInRoom = rect.yMax - 1;
		return new(x, largestYInRoom);
	}

	public Vector2Int RandomPointInside()
	{
		int x = RandomXInsideRoom();
		int y = RandomYInsideRoom();
		return new(x, y);
	}

	private int RandomXInsideRoom()
	{
		return UnityEngine.Random.Range(rect.xMin + 1, rect.xMax - 1);
	}
	private int RandomYInsideRoom()
	{
		return UnityEngine.Random.Range(rect.yMin + 1, rect.yMax - 1);
	}
};

} // namespace DungeonGeneration

