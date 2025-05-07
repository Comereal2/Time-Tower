using UnityEngine;
using System.Collections.Generic;

namespace DungeonGeneration
{

[CreateAssetMenu(menuName = "Dungeon Generation/ClosestRoomCorridorGeneration")]
public class ClosestRoomCorridorGeneration : ICorridorGenerationStrategy
{
	public int numRandomCorridors;

	public override void GenerateCorridors(List<DungeonRoom> rooms, ref DungeonTerrainType[,] terrains)
	{
		var corridors = MinimumCorridors(rooms);
		corridors.UnionWith(RandomCorridors(rooms));

		foreach (Vector2Int pos in corridors)
		{
			if (terrains[pos.x, pos.y] == DungeonTerrainType.Rock || terrains[pos.x, pos.y] == DungeonTerrainType.RoomOutline)
			{
				terrains[pos.x, pos.y] = DungeonTerrainType.CorridorFloor;
			}
		}
	}

	private HashSet<Vector2Int> MinimumCorridors(List<DungeonRoom> rooms)
	{
		HashSet<DungeonRoom> roomsToConnect = new HashSet<DungeonRoom>(rooms);
		DungeonRoom currentRoom = rooms[Random.Range(0, rooms.Count)];
		roomsToConnect.Remove(currentRoom);
		HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

		while (roomsToConnect.Count > 0)
		{
			DungeonRoom nextRoom = FindClosestRoom(currentRoom, roomsToConnect);
			corridors.UnionWith(CorridorBetween(currentRoom, nextRoom));

			roomsToConnect.Remove(nextRoom);
			currentRoom = nextRoom;
		}
		return corridors;
	}

	private HashSet<Vector2Int> RandomCorridors(List<DungeonRoom> rooms)
	{
		HashSet<Vector2Int> corridors = new();
		if (rooms.Count < 2)
			return corridors;

		for (int i = 0; i < numRandomCorridors; ++i)
		{
			int firstIndex = UnityEngine.Random.Range(0, rooms.Count);
			int secondIndex = UnityEngine.Random.Range(0, rooms.Count);
			while (firstIndex == secondIndex)
			{
				secondIndex = UnityEngine.Random.Range(0, rooms.Count);
			}
			corridors.UnionWith(CorridorBetween(rooms[firstIndex], rooms[secondIndex]));
		}
		return corridors;
	}

	private HashSet<Vector2Int> CorridorBetween(DungeonRoom currentRoom, DungeonRoom nextRoom)
	{
		HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
		Vector2Int currentPoint = currentRoom.RandomPointInside();
		Vector2Int destinationPoint = nextRoom.RandomPointInside();
		corridor.Add(currentPoint);

		while (currentPoint.y != destinationPoint.y)
		{
			currentPoint = currentPoint + ((currentPoint.y < destinationPoint.y)
					? Vector2Int.up
					: Vector2Int.down);
			corridor.Add(currentPoint);
		}
		while (currentPoint.x != destinationPoint.x)
		{
			currentPoint = currentPoint + ((currentPoint.x < destinationPoint.x)
					? Vector2Int.right
					: Vector2Int.left);
			corridor.Add(currentPoint);
		}
		return corridor;
	}

	private DungeonRoom FindClosestRoom(DungeonRoom currentRoom, HashSet<DungeonRoom> roomsToConnect)
	{
		DungeonRoom closest = currentRoom;
		float closestMagnitude = float.MaxValue;
		foreach (DungeonRoom room in roomsToConnect)
		{
			float currMag = Vector2.Distance(room.rect.center, currentRoom.rect.center);
			if (currMag < closestMagnitude)
			{
				closestMagnitude = currMag;
				closest = room;
			}
		}
		return closest;
	}
}

} // namespace DungeonGeneration

