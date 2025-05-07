using UnityEngine;

namespace DungeonGeneration
{

public static class RotationMatrices
{
	public static Matrix4x4 Identity = Matrix4x4.identity;
	public static Matrix4x4 Rotate90 = Matrix4x4.Rotate(Quaternion.Euler(0,0,90));
	public static Matrix4x4 Rotate180 = Matrix4x4.Rotate(Quaternion.Euler(0,0,180));
	public static Matrix4x4 Rotate270 = Matrix4x4.Rotate(Quaternion.Euler(0,0,270));
};

} // namespace DungeonGeneration

