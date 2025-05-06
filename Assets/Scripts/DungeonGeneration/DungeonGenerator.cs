using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace DungeonGeneration
{

public class DungeonGenerator : MonoBehaviour
{
	public DungeonSO dungeon;
	public Tilemap tilemap;

	void Start()
	{
		dungeon.GenerateDungeon(tilemap);
	}

}

} // namespace DungeonGeneration

