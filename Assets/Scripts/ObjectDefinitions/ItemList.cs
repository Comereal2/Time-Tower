using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Object/WeaponList")]

public class WeaponList : ScriptableObject
{
	public List<Weapon> items;
	public List<float> cdf;

	void OnValidate()
	{
		if (items.Count != cdf.Count)
		{
			Debug.LogWarning("Cdf must be same size as items. Increase EnemyTypes if youre trying to add elements");
			float chanceForEach = 1.0f / items.Count;
			cdf = new();
			for (int i = 0; i < items.Count; ++i)
			{
				cdf.Add(chanceForEach * (i + 1));
			}
		}
		else if (cdf.Count > 0 && !Mathf.Approximately(cdf[cdf.Count - 1], 1.0f))
		{
			Debug.LogWarning("Cdf end should be 1.0f. Changing");
			cdf[cdf.Count - 1] = 1.0f;
		}
	}
	
	public Weapon RandomWeapon()
	{
		float val = UnityEngine.Random.value;
		for (int i = 0; i < cdf.Count; ++i)
		{
			if (val < cdf[i])
				return items[i];
		}
		return items[items.Count - 1];
	}
}

