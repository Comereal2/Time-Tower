using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace DungeonGeneration
{

[RequireComponent(typeof(DungeonRenderer))]
public class DungeonGenerator : MonoBehaviour
{
	// TODO: link multiple FloorSOs to make a cohesive dungeon
	// TODO: Have FloorSO type that builds the boss room

	public FloorSO dungeon;
	public Goal goalPrefab;
	public PlayerController playerPrefab;

	DungeonRenderer dungeonRenderer;

	public Tilemap collisionTilemap;
	public Tilemap displayTilemap;

	int floorNumber;

	void Start()
	{
		dungeonRenderer = GetComponent<DungeonRenderer>();
		GenerateFloor();
	}

	public void GenerateFloor()
	{
		dungeon.GenerateDungeon();
		dungeonRenderer.Draw(dungeon.terrains, collisionTilemap, displayTilemap);

		DungeonRoom goalRoom = dungeon.RandomNonSpawnRoom();
		Vector3 pos = displayTilemap.gameObject.transform.position + 2 * (Vector3)goalRoom.rect.center;
		Instantiate(goalPrefab, pos, Quaternion.identity);
		++floorNumber;

		Debug.Log($"Floor {floorNumber} generated");
		PlacePlayer();
	}

	private void PlacePlayer()
	{
		GameObject player = GameObject.Find("Player");
		if (player == null)
		{
			player = Instantiate(playerPrefab).gameObject;
		}
		player.transform.position = displayTilemap.gameObject.transform.position + 2 * (Vector3)dungeon.SpawnRoom().rect.center; 
	}
}

} // namespace DungeonGeneration

