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
	[SerializeField] Vector2Int bossRoomDims;
	[SerializeField] int roomAttempts;
	[SerializeField] int numEnemies;
	[SerializeField] EnemySpawner enemySpawner;
	[SerializeField] EnemySpawner bossSpawner;
	[SerializeField] RoomSizeParameters roomSizeParameters;
	[SerializeField] ICorridorGenerationStrategy corridorGenerationStrategy;

	RoomSizeParametersWithPadding rspPadded;
	List<DungeonRoom> rooms;
	public DungeonTerrainType[,] terrains;

	void OnValidate()
	{
		int x = Mathf.Max(spawnRoomDims.x, 3);
		int y = Mathf.Max(spawnRoomDims.y, 2);
		spawnRoomDims = new Vector2Int(x, y);
		int bossx = Mathf.Max(bossRoomDims.x, 4);
		int bossy = Mathf.Max(bossRoomDims.y, 4);
		bossRoomDims = new Vector2Int(bossx, bossy);
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

	private void GenerateStartingRooms()
	{
		rooms = new List<DungeonRoom>();
		// Boss room
		rooms.Add(new DungeonRoom(mapSize, bossRoomDims, false));
		// Spawn room
		DungeonRoom spawnCandidate = new DungeonRoom(mapSize, spawnRoomDims, true);
		for (int i = 0; i < 100; ++i)
		{
			if (!spawnCandidate.rect.Overlaps(rooms[0].rect))
			{
				rooms.Add(spawnCandidate);
				return;
			}
			spawnCandidate = new DungeonRoom(mapSize, bossRoomDims, true);
		}

		// Failed randomly placing, manual in the corners
		rooms = new List<DungeonRoom>();
		rooms.Add(new DungeonRoom(mapSize, bossRoomDims, false, false));
		rooms.Add(new DungeonRoom(mapSize, spawnRoomDims, true, false));
	}

	private void GenerateRooms()
	{
		GenerateStartingRooms();
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

	public void PlaceEnemies(Tilemap displayTilemap)
	{
		for (int i = 0; i < numEnemies; ++i)
		{
			Vector3Int roomCoords = (Vector3Int)RandomNonSpecialRoom().RandomPointInside();
			Instantiate(enemySpawner, displayTilemap.GetCellCenterWorld(roomCoords), Quaternion.identity);
		}
		Vector3Int bossCoords = (Vector3Int)rooms[0].RandomPointInside();
		Instantiate(bossSpawner, displayTilemap.GetCellCenterWorld(bossCoords), Quaternion.identity);
	}

	public DungeonRoom RandomNonSpecialRoom()
	{
		return rooms[UnityEngine.Random.Range(2,rooms.Count)];
	}

	public DungeonRoom SpawnRoom()
	{
		return rooms[1];
	}
}

} // namespace DungeonGeneration

