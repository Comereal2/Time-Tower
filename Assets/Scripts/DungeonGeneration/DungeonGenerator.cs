using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

namespace DungeonGeneration
{

[RequireComponent(typeof(DungeonRenderer))]
public class DungeonGenerator : MonoBehaviour
{
	// TODO: link multiple FloorSOs to make a cohesive dungeon
	// TODO: Have FloorSO type that builds the boss room

	public FloorSO dungeon;
	GameObject disabledInitializer;

	DungeonRenderer dungeonRenderer;

	public Tilemap collisionTilemap;
	public Tilemap displayTilemap;
	public ShopTile shopTilePrefab;
	public ShopTile bonusTimeShopTile;

	int floorNumber;

	void Start()
	{
		disabledInitializer = GameObject.Find("DisabledInitializer");
		dungeonRenderer = GetComponent<DungeonRenderer>();
		GenerateFloor();
	}

	public void GenerateFloor()
	{
		dungeon.GenerateDungeon();
		dungeonRenderer.Draw(dungeon.terrains, collisionTilemap, displayTilemap);

		DungeonRoom goalRoom = dungeon.RandomNonSpecialRoom();
		++floorNumber;

		Debug.Log($"Floor {floorNumber} generated");
		dungeon.PlaceEnemies(displayTilemap);
		PlacePlayer();
		PopulateSpawnRoom();
	}

	public void PopulateSpawnRoom()
	{
		Vector3Int shopPos = (Vector3Int)dungeon.SpawnRoom().ShopTilePosition();
		Instantiate(shopTilePrefab, displayTilemap.GetCellCenterWorld(shopPos) + .7f * Vector3.down, Quaternion.identity);
		Vector3Int bonusTimeShopPos = (Vector3Int)dungeon.SpawnRoom().ShopTilePosition();
		while (shopPos == bonusTimeShopPos)
			bonusTimeShopPos = (Vector3Int)dungeon.SpawnRoom().ShopTilePosition();
		Instantiate(bonusTimeShopTile, displayTilemap.GetCellCenterWorld(bonusTimeShopPos) + .7f * Vector3.down, Quaternion.identity);
	}

	private void PlacePlayer()
	{
		PlayerController player = PlayerController.playerController;
		if (player == null)
		{
			Debug.LogWarning("Player not in scene");
		}
		player.transform.position = displayTilemap.transform.position + 2 * (Vector3)dungeon.SpawnRoom().rect.center; 
	}
}

} // namespace DungeonGeneration

