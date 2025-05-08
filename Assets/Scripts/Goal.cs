using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
	void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log("Goal collided with player");
		gameObject.SetActive(false);
		GameObject.Find("MapManager").GetComponent<DungeonGeneration.DungeonGenerator>().GenerateFloor(true);
	}
}

