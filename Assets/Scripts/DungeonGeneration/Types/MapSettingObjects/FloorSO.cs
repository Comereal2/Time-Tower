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
	public int roomAttempts;
	public int numEnemies;
	public EnemySpawner enemySpawner;
	public EnemySpawner bossSpawner;
	public ICorridorGenerationStrategy corridorGenerationStrategy;

	[Header("Room Size Parameters")]
	public int roomMinWidth;
	public int roomMaxWidth;
	public int roomMinHeight;
	public int roomMaxHeight;

	void OnValidate()
	{
		int x = Mathf.Max(spawnRoomDims.x, 3);
		int y = Mathf.Max(spawnRoomDims.y, 3);
		spawnRoomDims = new Vector2Int(x, y);
		int bossx = Mathf.Max(bossRoomDims.x, 4);
		int bossy = Mathf.Max(bossRoomDims.y, 4);
		bossRoomDims = new Vector2Int(bossx, bossy);

		// Force Rooms to be at least 2x2 tiles large
		if (roomMinHeight < 4)
		{
			Debug.LogWarning("min dimensions must be 4 or larger to account for padding");
			roomMinHeight = 4;
		}
		if (roomMinWidth < 4)
		{
			Debug.LogWarning("min dimensions must be 4 or larger to account for padding");
			roomMinWidth = 4;
		}
		if (roomMinWidth >= roomMaxWidth)
		{
			roomMaxWidth = roomMinWidth + 1;
		}
		if (roomMinHeight >= roomMaxHeight)
		{
			roomMaxHeight = roomMinHeight + 1;
		}
	}
}

} // namespace DungeonGeneration

