using UnityEngine;


namespace DungeonGeneration
{

[CreateAssetMenu(menuName = "Dungeon Generation/RoomSizeParameters")]
public class RoomSizeParameters : ScriptableObject
{
	public int minWidth;
	public int maxWidth;
	public int minHeight;
	public int maxHeight;
};


} // namespace DungeonGeneration

