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
	public Tile cornerIn;
	public Tile cornerOut;
};

} // namespace DungeonGeneration

