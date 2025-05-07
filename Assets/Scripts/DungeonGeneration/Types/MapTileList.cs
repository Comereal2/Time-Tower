using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace DungeonGeneration
{

[CreateAssetMenu(menuName = "Dungeon Generation/MapTileList")]
public class MapTileList : ScriptableObject
{
	public List<Tile> floors;
	public List<Tile> walls;
	public List<Tile> cornerIns;
	public List<Tile> cornerOuts;
	public Tile invisible;
	public float accentChance;

	private int BaseOrAccent()
	{
		return (UnityEngine.Random.Range(0f,1f) < accentChance)
			? 1
			: 0;
	}
	public Tile PickFloor()
	{
		return floors[BaseOrAccent()];
	}
	public Tile PickWall()
	{
		return walls[BaseOrAccent()];
	}
	public Tile PickCornerIn()
	{
		return cornerIns[BaseOrAccent()];
	}
	public Tile PickCornerOut()
	{
		return cornerOuts[BaseOrAccent()];
	}
};

} // namespace DungeonGeneration

