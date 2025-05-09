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
	public Vector2Int spawnRoomDims;
	public Vector2Int bossRoomDims;
	public float minimumBossSpawnDistance; 
	public int roomAttempts;
	public int maxNumRandomShops;
	public int numEnemies;
	public EnemySpawner enemySpawner;
	public EnemySpawner bossSpawner;
	public ICorridorGenerationStrategy corridorGenerationStrategy;
	public bool checkParameters;

	[Header("Room Size Parameters")]
	public int roomMinWidth;
	public int roomMaxWidth;
	public int roomMinHeight;
	public int roomMaxHeight;

	private void LogError(string s)
	{
		Debug.LogError($"{base.name} - {s}");
	}

	void OnValidate()
	{
		if (!checkParameters)
		{
			Debug.LogWarning("Remember to turn Check Parameters on when you're done");
			return;
		}

		bool failedChecks = false;
		int x = Mathf.Max(spawnRoomDims.x, 4);
		int y = Mathf.Max(spawnRoomDims.y, 3);
		var newSpawnRoomDims = new Vector2Int(x, y);
		if (newSpawnRoomDims != spawnRoomDims)
		{
			spawnRoomDims = newSpawnRoomDims;
			LogError("Spawn Room must be at least 4 wide and 3 tall");
			failedChecks = true;
		}
		int bossx = Mathf.Max(bossRoomDims.x, 4);
		int bossy = Mathf.Max(bossRoomDims.y, 4);
		var newBossRoomDims = new Vector2Int(bossx, bossy);
		if (newBossRoomDims != bossRoomDims)
		{
			bossRoomDims = newBossRoomDims;
			LogError("Boss Room must be at least 4 on both sides");
			failedChecks = true;
		}
		if (maxNumRandomShops > roomAttempts)
		{
			maxNumRandomShops = roomAttempts;
			LogError("Each room can have max 1 random shop, updating");
			failedChecks = true;
		}

		// Max here since we use center for distance calculations.
		float minDistance = (float) Mathf.Max(bossRoomDims.x, bossRoomDims.y, spawnRoomDims.x, spawnRoomDims.y);
		if (minimumBossSpawnDistance < minDistance)
		{
			minimumBossSpawnDistance = minDistance;
		}
		// Not necessarily an error but it would be nice to notify
		if (minimumBossSpawnDistance < 2 * minDistance)
		{
			Debug.LogWarning("Min Boss Distance is close, the boss room might spawn too close to the spawn room");
		}

		// MapSize must be big enough to hold the Spawn and Boss Rooms across from each other)
		int minMapSizeX = x + bossx;
		int minMapSizeY = y + bossy;
		int updatedMapSizeX = Mathf.Max(minMapSizeX, mapSize.x);
		int updatedMapSizeY = Mathf.Max(minMapSizeY, mapSize.y);
		var updatedMapSize =  new Vector2Int(updatedMapSizeX, updatedMapSizeY);
		if (updatedMapSize != mapSize)
		{
			mapSize = updatedMapSize;
			LogError("Map must be big enough to hold both the Spawn and Boss rooms at opposite ends of the map");
			failedChecks = true;
		}

		// Force Rooms to be at least 2x2 tiles large
		if (roomMinHeight < 4)
		{
			LogError("min dimensions must be 4 or larger to account for padding");
			roomMinHeight = 4;
			failedChecks = true;
		}
		if (roomMinWidth < 4)
		{
			LogError("min dimensions must be 4 or larger to account for padding");
			roomMinWidth = 4;
			failedChecks = true;
		}
		if (roomMinWidth >= roomMaxWidth)
		{
			LogError("Width dims are [inclusive, exclusive)");
			roomMaxWidth = roomMinWidth + 1;
			failedChecks = true;
		}
		if (roomMinHeight >= roomMaxHeight)
		{
			LogError("Height dims are [inclusive, exclusive)");
			roomMaxHeight = roomMinHeight + 1;
			failedChecks = true;
		}
		if (enemySpawner == null || bossSpawner == null || corridorGenerationStrategy == null)
		{
			LogError("All SOs must be instantiated");
			failedChecks = true;
		}
		
		if (failedChecks)
		{
			Debug.Log("Turn off Check Parameters if you're currently editing and I'm getting in your way :(");
			Debug.LogWarning("Dont forget to save your changes!");
		}
	}
}

} // namespace DungeonGeneration

