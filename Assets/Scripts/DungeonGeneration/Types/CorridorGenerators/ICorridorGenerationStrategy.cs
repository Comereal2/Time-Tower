using UnityEngine;
using System.Collections.Generic;

namespace DungeonGeneration
{

public abstract class ICorridorGenerationStrategy : ScriptableObject
{
	public abstract void GenerateCorridors(List<DungeonRoom> rooms, ref DungeonTerrainType[,] terrains);
}

} // namespace DungeonGeneration


