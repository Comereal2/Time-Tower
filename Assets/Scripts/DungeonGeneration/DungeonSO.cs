using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonGeneration
{

// TODO: rename to FloorSO
[CreateAssetMenu(fileName = "DungeonSO", menuName = "Dungeon Generation/DungeonSO")]
public class DungeonSO : ScriptableObject
{
	[Header("Settings")]
	[SerializeField] Vector2Int mapSize; // Map Bounds center around origin (0,0)
	[SerializeField] int roomAttempts;
	[SerializeField] RoomSizeParameters roomSizeParameters;
	[SerializeField] MapTileList mapTileList;

	List<DungeonRoom> rooms;

	public void GenerateDungeon(Tilemap map)
	{
		rooms = new List<DungeonRoom>();
		for (int i = 0; i < roomAttempts; ++i)
		{
			DungeonRoom potenchRoom = new DungeonRoom(mapSize, roomSizeParameters);
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

		foreach (var room in rooms)
		{
			// TODO: draw colliders as well
			room.Draw(map, mapTileList);
		}
	}
}

} // namespace DungeonGeneration

