using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Goal : MonoBehaviour
{
    [SerializeField] private GameObject continueButton;
    void OnTriggerEnter2D(Collider2D other)
	{
		//You need to check if it actually is the player or something else
		if (other.gameObject.CompareTag("Player"))
		{
            continueButton = Instantiate(continueButton, GameObject.FindGameObjectWithTag("CoinCounter").transform.parent);
            gameObject.SetActive(false);
            continueButton.GetComponent<Button>().onClick.AddListener(ContinueButton);
        }
	}

    private void ContinueButton()
    {
        GameObject.FindGameObjectWithTag("DungeonGenerator").GetComponent<DungeonGeneration.DungeonGenerator>().GenerateFloor(true);
        //For some reason these sometimes duplicate so I just destroy all >:3
        foreach (Transform child in continueButton.transform.parent)
        {
            if (child.CompareTag("ContinueButtonERRORFIX")) Destroy(child.gameObject);
        }
        Destroy(gameObject);
    }
}
