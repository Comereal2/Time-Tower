using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonGeneration
{

[CreateAssetMenu(fileName = "FloorSO", menuName = "Dungeon Generation/FloorSO")]
public class FloorSO : ScriptableObject
{
	[Header("Settings")]
	public Vector2Int mapSize;
	[SerializeField] Vector2Int spawnRoomDims;
	[SerializeField] int roomAttempts;
	[SerializeField] int numEnemies;
	[SerializeField] RoomSizeParameters roomSizeParameters;
	[SerializeField] ICorridorGenerationStrategy corridorGenerationStrategy;

	RoomSizeParametersWithPadding rspPadded;
	List<DungeonRoom> rooms;
	public DungeonTerrainType[,] terrains;

	void OnValidate()
	{
		int x = Mathf.Max(spawnRoomDims.x, 3);
		int y = Mathf.Max(spawnRoomDims.y, 3);
		spawnRoomDims = new Vector2Int(x, y);
		rspPadded.UpdateParams(roomSizeParameters);
	}

	private void FloodWithRock()
	{
		terrains = new DungeonTerrainType[mapSize.x,mapSize.y];
		for (int i = 0; i < mapSize.x; ++i)
		{
			for (int j = 0; j < mapSize.y; ++j)
			{
				terrains[i,j] = DungeonTerrainType.Rock;
			}
		}
	}

	private void GenerateRooms()
	{
		rooms = new List<DungeonRoom>();

		// Spawn room
		rooms.Add(new DungeonRoom(mapSize, spawnRoomDims));

		for (int i = 0; i < roomAttempts; ++i)
		{
			DungeonRoom potenchRoom = new DungeonRoom(mapSize, rspPadded);
			bool roomValid = true;
			foreach (var room in rooms)
			{
				if (potenchRoom.rect.Overlaps(room.rect))
				{
					roomValid = false;
					break;
				}
			}
			if (roomValid)
			{
				rooms.Add(potenchRoom);
			}
		}
		UpdateTerrain();
	}

	private void UpdateTerrain()
	{
		foreach (var room in rooms)
		{
			for (int i = room.rect.xMin + 1; i < Mathf.Min(mapSize.x, room.rect.xMax); ++i)
			{
				for (int j = room.rect.yMin + 1; j < room.rect.yMax; ++j)
				{
					terrains[i,j] = DungeonTerrainType.RoomFloor;
				}
			}
			if (room.rect.xMin > -1)
			{
				for (int i = room.rect.yMin; i <= room.rect.yMax; ++i)
				{
					if (i >= 0 && i < mapSize.y)
						terrains[room.rect.xMin, i] = DungeonTerrainType.RoomOutline;
				}
			}
			if (room.rect.xMax < mapSize.x)
			{
				for (int i = room.rect.yMin; i <= room.rect.yMax; ++i)
				{
					if (i >= 0 && i < mapSize.y)
						terrains[room.rect.xMax, i] = DungeonTerrainType.RoomOutline;
				}
			}
			if (room.rect.yMin > -1)
			{
				for (int i = room.rect.xMin; i <= room.rect.xMax; ++i)
				{
					if (i >= 0 && i < mapSize.x)
						terrains[i, room.rect.yMin] = DungeonTerrainType.RoomOutline;
				}
			}
			if (room.rect.yMax < mapSize.y)
			{
				for (int i = room.rect.xMin; i <= room.rect.xMax; ++i)
				{
					if (i >= 0 && i < mapSize.x)
						terrains[i, room.rect.yMax] = DungeonTerrainType.RoomOutline;
				}
			}
		}
	}

	public void GenerateDungeon()
	{
		FloodWithRock();
		GenerateRooms();
		corridorGenerationStrategy.GenerateCorridors(rooms, ref terrains);
	}

	public DungeonRoom RandomNonSpawnRoom()
	{
		return rooms[UnityEngine.Random.Range(1,rooms.Count)];
	}

	public DungeonRoom SpawnRoom()
	{
		return rooms[0];
	}
}

} // namespace DungeonGeneration

