using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace DungeonGeneration
{

[CreateAssetMenu(menuName = "Dungeon Generation/EnemyList")]
public class EnemyList : ScriptableObject
{
	public List<Enemy> enemyTypes;
	public List<float> cdf;

	void OnValidate()
	{
		if (enemyTypes.Count != cdf.Count)
		{
			Debug.LogWarning("Cdf must be same size as enemyTypes. Increase EnemyTypes if youre trying to add elements");
			float chanceForEachEnemy = 1.0f / enemyTypes.Count;
			cdf = new();
			for (int i = 0; i < enemyTypes.Count; ++i)
			{
				cdf.Add(chanceForEachEnemy * (i + 1));
			}
		}
		else if (cdf.Count > 0 && !Mathf.Approximately(cdf[cdf.Count - 1], 1.0f))
		{
			Debug.LogWarning("Cdf end should be 1.0f. Changing");
			cdf[cdf.Count - 1] = 1.0f;
		}
	}
	
	public Enemy RandomEnemy()
	{
		float val = UnityEngine.Random.value;
		for (int i = 0; i < cdf.Count; ++i)
		{
			if (val < cdf[i])
				return enemyTypes[i];
		}
		return enemyTypes[enemyTypes.Count - 1];
	}
};

} // namespace DungeonGeneration

