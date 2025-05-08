using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

namespace DungeonGeneration
{

[RequireComponent(typeof(DungeonRenderer))]
[RequireComponent(typeof(CameraFade))]
public class DungeonGenerator : MonoBehaviour
{
	public List<FloorSO> floorsList;
	public FloorSO MaxFloor;
	private FloorSO currentFloor;

	private DungeonRenderer dungeonRenderer;
	public float tileScaleFactor = 2.0f;
	public Tilemap collisionTilemap;
	public Tilemap displayTilemap;
	public ShopTile shopTilePrefab;
	public ShopTile bonusTimeShopTile;
	public ShopTile singleUseShopTile;
	public int attemptsForRandomBossPlacement = 100;

	private GameObject floorEntityHolder;

	public int floorNumber {get; private set; }

	// It is required that rooms[0] is the boss room, and rooms[1] is the Spawn Room
	List<DungeonRoom> rooms;
	public DungeonTerrainType[,] terrains;

	void Awake()
	{
		dungeonRenderer = GetComponent<DungeonRenderer>();
		dungeonRenderer.SetTilemaps(collisionTilemap, displayTilemap);
		transform.localScale = new Vector3(tileScaleFactor, tileScaleFactor, 1.0f);
	}

	void Start()
	{
		GenerateFloor(false);
	}

	public void GenerateFloor(bool incrementFloor)
	{
		SendMessage("FadeOut");
		WipeMap();
		if (incrementFloor)
		{
			Destroy(floorEntityHolder);
			dungeonRenderer.ClearTilemaps();
			++floorNumber;
				foreach (Enemy enemy in Resources.LoadAll<Enemy>("Data/Enemies"))
				{
					for (int i = 0; i < PlayerPrefs.GetInt("", 0) + 1; i++)
					{
						enemy.UpgradeEnemy();
					}
				}
			}
		floorEntityHolder = new GameObject("FloorEntityHolder");
		currentFloor = (floorNumber >= floorsList.Count)
				? MaxFloor
				: floorsList[floorNumber];
		PrepareForGeneration();
		FloodWithRock();
		GenerateRooms();
		currentFloor.corridorGenerationStrategy.GenerateCorridors(rooms, ref terrains);
		dungeonRenderer.Draw(currentFloor.mapSize, terrains);
		PlaceEnemies();
		PlaceShops();
		PopulateSpawnRoom();
		PlacePlayer();

		SendMessage("FadeIn");
	}


	private void WipeMap()
	{
        foreach (GameObject gameObject in Object.FindObjectsByType(typeof(GameObject), FindObjectsSortMode.None))
        {
			if (gameObject.CompareTag("Coin") || gameObject.CompareTag("Weapon") || gameObject.CompareTag("PlayerBullet") || gameObject.CompareTag("EnemyBullet") || gameObject.CompareTag("ShopTile")) Destroy(gameObject);
        }
    }

	private void PrepareForGeneration()
	{
		terrains = new DungeonTerrainType[currentFloor.mapSize.x,currentFloor.mapSize.y];
		rooms = new List<DungeonRoom>();
	}

	private void PlaceFloorEntity(GameObject obj, Vector2Int cellPosition)
	{
		Vector3Int roomCoords = (Vector3Int)cellPosition;
		Instantiate(obj, collisionTilemap.GetCellCenterWorld(roomCoords), Quaternion.identity, floorEntityHolder.transform);
	}

	private void PlaceFloorEntity(GameObject obj, Vector3Int roomCoords)
	{
		Instantiate(obj, collisionTilemap.GetCellCenterWorld(roomCoords), Quaternion.identity, floorEntityHolder.transform);
	}

	private void PlaceFloorEntity(GameObject obj, Vector3 worldCoords)
	{
		Instantiate(obj, worldCoords, Quaternion.identity, floorEntityHolder.transform);
	}

	private void PopulateSpawnRoom()
	{
		Vector2Int[] shopPos = SpawnRoom().SpawnRoomShopPositions();

		Vector3Int itemShopPos = new(shopPos[0].x, shopPos[0].y);
		Vector3Int timeShopPos = new(shopPos[1].x, shopPos[1].y);

		PlaceFloorEntity(shopTilePrefab.gameObject, collisionTilemap.GetCellCenterWorld(itemShopPos) + .7f * Vector3.down);
		PlaceFloorEntity(bonusTimeShopTile.gameObject, collisionTilemap.GetCellCenterWorld(timeShopPos) + .7f * Vector3.down);
	}

	private void PlacePlayer()
	{
		PlayerController player = PlayerController.playerController;
		if (player == null)
		{
			Debug.LogWarning("Player not in scene");
		}
		player.transform.position = collisionTilemap.GetCellCenterWorld(SpawnRoom().CenterCoords());
	}

	private void FloodWithRock()
	{
		for (int i = 0; i < currentFloor.mapSize.x; ++i)
		{
			for (int j = 0; j < currentFloor.mapSize.y; ++j)
			{
				terrains[i,j] = DungeonTerrainType.Rock;
			}
		}
	}

	private void GenerateStartingRooms()
	{
		// Attempt to randomly place rooms
		rooms.Add(new DungeonRoom(currentFloor.mapSize, currentFloor.bossRoomDims, false));
		DungeonRoom spawnCandidate = new DungeonRoom(currentFloor.mapSize, currentFloor.spawnRoomDims, true);
		
		for (int i = 0; i < attemptsForRandomBossPlacement; ++i)
		{
			if (!spawnCandidate.rect.Overlaps(rooms[0].rect))
			{
				Vector2 distanceVector = rooms[0].rect.center - spawnCandidate.rect.center;
				if (distanceVector.magnitude >= currentFloor.minimumBossSpawnDistance)
				{
					rooms.Add(spawnCandidate);
					return;
				}
			}
			spawnCandidate = new DungeonRoom(currentFloor.mapSize, currentFloor.bossRoomDims, true);
		}

		// Failed randomly placing, manual in the corners
		rooms = new List<DungeonRoom>();
		rooms.Add(new DungeonRoom(currentFloor.mapSize, currentFloor.bossRoomDims, false, false));
		rooms.Add(new DungeonRoom(currentFloor.mapSize, currentFloor.spawnRoomDims, true, false));
	}

	private void GenerateRooms()
	{
		GenerateStartingRooms();
		for (int i = 0; i < currentFloor.roomAttempts; ++i)
		{
			DungeonRoom potenchRoom = new DungeonRoom(currentFloor.mapSize
					, currentFloor.roomMinWidth
					, currentFloor.roomMaxWidth
					, currentFloor.roomMinHeight
					, currentFloor.roomMaxHeight);
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
			for (int i = room.rect.xMin + 1; i < Mathf.Min(currentFloor.mapSize.x, room.rect.xMax); ++i)
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
					if (i >= 0 && i < currentFloor.mapSize.y)
						terrains[room.rect.xMin, i] = DungeonTerrainType.RoomOutline;
				}
			}
			if (room.rect.xMax < currentFloor.mapSize.x)
			{
				for (int i = room.rect.yMin; i <= room.rect.yMax; ++i)
				{
					if (i >= 0 && i < currentFloor.mapSize.y)
						terrains[room.rect.xMax, i] = DungeonTerrainType.RoomOutline;
				}
			}
			if (room.rect.yMin > -1)
			{
				for (int i = room.rect.xMin; i <= room.rect.xMax; ++i)
				{
					if (i >= 0 && i < currentFloor.mapSize.x)
						terrains[i, room.rect.yMin] = DungeonTerrainType.RoomOutline;
				}
			}
			if (room.rect.yMax < currentFloor.mapSize.y)
			{
				for (int i = room.rect.xMin; i <= room.rect.xMax; ++i)
				{
					if (i >= 0 && i < currentFloor.mapSize.x)
						terrains[i, room.rect.yMax] = DungeonTerrainType.RoomOutline;
				}
			}
		}
	}

	private void PlaceEnemies()
	{
		PlaceFloorEntity(currentFloor.bossSpawner.gameObject, rooms[0].RandomPointInside());
		if (rooms.Count == 2)
		{
			return;
		}
		for (int i = 0; i < currentFloor.numEnemies; ++i)
		{
			PlaceFloorEntity(currentFloor.enemySpawner.gameObject, RandomNonSpecialRoom().RandomPointInside());
		}
	}

	private void PlaceShops()
	{
		if (rooms.Count == 2)
		{
			return;
		}
		// They look bad when they dont have enough space. easiest way is restrict 1 to the non special rooms
		// Can change to allow multiple in each room but i dont think this harms the experience
		List<DungeonRoom> nonSpecialRooms = new();
		for (int i = 2; i < rooms.Count; ++i)
			nonSpecialRooms.Add(rooms[i]);
		for (int i = 0; i < currentFloor.maxNumRandomShops && nonSpecialRooms.Count > 0; ++i)
		{
			var randomRoom = nonSpecialRooms[UnityEngine.Random.Range(0, nonSpecialRooms.Count)];
			PlaceFloorEntity(singleUseShopTile.gameObject, randomRoom.RandomShopTilePosition());
			nonSpecialRooms.Remove(randomRoom);
		}

	}

#nullable enable
	public DungeonRoom? RandomNonSpecialRoom()
	{
		if (rooms.Count <= 2)
		{
			return null;
		}
		return rooms[UnityEngine.Random.Range(2,rooms.Count)];
	}
#nullable disable

	public DungeonRoom SpawnRoom()
	{
		return rooms[1];
	}

	public void BossDefeated()
	{
		PlaceFloorEntity(singleUseShopTile.gameObject, rooms[0].RandomShopTilePosition());
	}
}

} // namespace DungeonGeneration

