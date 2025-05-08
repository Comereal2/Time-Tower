using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonGeneration;

public class GameManager : MonoBehaviour
{
	public GameObject canvases;
	public PlayerController player;
	public AstarPath pathfinderManager;
	public GameObject roomObjectManager;
	public DungeonGenerator dungeonGenerator;

    void Start()
    {
        Instantiate(canvases);
        Instantiate(player);
        Instantiate(pathfinderManager);
        Instantiate(roomObjectManager);
        Instantiate(dungeonGenerator);
    }
}
