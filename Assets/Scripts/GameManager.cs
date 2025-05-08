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
        ReloadEnemies();
        empty = new GameObject("Empty");
        empty.AddComponent<Text>();
        Instantiate(canvases);
        Instantiate(player);
        Instantiate(roomObjectManager);
        Instantiate(dungeonGenerator);
        Instantiate(pathfinderManager);
        /* Canvases appear first for displays like timers to draw on them
         * Player appears second as a reference to other objects and to be teleported
         * The room object holder appears third for the dungeon generator to place objects in it
         * Dungeon generator appears fourth
         * Pathfinder appears last so it generates a path with the dungeon generated
         */
    }

    private void ReloadEnemies()
    {
        foreach (var enemy in Resources.LoadAll<Enemy>("Data/Enemies"))
        {
            var defaultE = Resources.Load<Enemy>("Data/DefaultEnemies/" + enemy.name);
            enemy.health = defaultE.health;
            enemy.speed = defaultE.speed;
            enemy.damageMultiplier = defaultE.damageMultiplier;
            enemy.rangedAttackCooldown = defaultE.rangedAttackCooldown;
            enemy.projectileSpeed = defaultE.projectileSpeed;
        }
    }
}