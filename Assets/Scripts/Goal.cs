using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
	void OnTriggerEnter2D(Collider2D other)
	{
		//You need to check if it actually is the player or something else
		if (other.gameObject.CompareTag("Player"))
		{
            Debug.Log("Goal collided with player");
            gameObject.SetActive(false);
            //GameObject.Find("MapManager").GetComponent<DungeonGeneration.DungeonGenerator>().GenerateFloor(true);
            //This is why you use tags pal
            //Also you need to get rid of things like shops, bullets and coins
            GameObject.FindGameObjectWithTag("DungeonGenerator").GetComponent<DungeonGeneration.DungeonGenerator>().GenerateFloor(true);
        }
	}
}