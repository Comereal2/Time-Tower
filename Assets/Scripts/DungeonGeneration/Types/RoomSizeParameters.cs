using UnityEngine;


namespace DungeonGeneration
{

// These dimensions include the walls
[CreateAssetMenu(menuName = "Dungeon Generation/RoomSizeParameters")]
public class RoomSizeParameters : ScriptableObject
{
	public int minWidth;
	public int maxWidth;
	public int minHeight;
	public int maxHeight;

	void OnValidate()
	{
		if (minHeight < 1)
			minHeight = 1;
		if (minWidth < 1)
			minWidth = 1;

		// These checks will force height or width to be the min value
		if (maxHeight < minHeight)
			maxHeight = minHeight + 1;
		if (maxWidth < minWidth)
			maxWidth = minWidth + 1;

		if (minWidth < 3 || minHeight < 3)
		{
			Debug.LogWarning("Are you sure you want min height or width to be less than 3? Rooms might feel claustrophobic");
		}
	}
};

public struct RoomSizeParametersWithPadding
{
	public int minWidth;
	public int maxWidth;
	public int minHeight;
	public int maxHeight;

	RoomSizeParametersWithPadding(RoomSizeParameters rsp)
	{
		minWidth = rsp.minWidth + 2;
		maxWidth = rsp.maxWidth + 2;
		minHeight = rsp.minHeight + 2;
		maxHeight = rsp.maxHeight + 2;
	}

	public void UpdateParams(RoomSizeParameters rsp)
	{
		minWidth = rsp.minWidth + 2;
		maxWidth = rsp.maxWidth + 2;
		minHeight = rsp.minHeight + 2;
		maxHeight = rsp.maxHeight + 2;
	}
}


} // namespace DungeonGeneration

