using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonGeneration;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public GameObject canvases;
	public PlayerController player;
	public AstarPath pathfinderManager;
	public GameObject roomObjectManager;
	public DungeonGenerator dungeonGenerator;

    //Empty game object for enemy and player to reference
    public static GameObject empty;

    void Start()
    {
        empty = new GameObject("Empty");
        empty.AddComponent<Text>();
        Instantiate(canvases);
        Instantiate(player);
        Instantiate(pathfinderManager);
        Instantiate(roomObjectManager);
        Instantiate(dungeonGenerator);
    }
}
