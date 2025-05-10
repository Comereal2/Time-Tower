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
        ReloadItemCosts();
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

    /// <summary>
    /// Reloads all enemy stats by loading in the default
    /// </summary>
    private void ReloadEnemies()
    {
        foreach (Enemy enemy in Resources.LoadAll<Enemy>("Data/Enemies"))
        {
            Enemy defaultE = Resources.Load<Enemy>("Data/DefaultEnemies/" + enemy.name);
            enemy.health = defaultE.health;
            enemy.speed = defaultE.speed;
            enemy.damageMultiplier = defaultE.damageMultiplier;
            enemy.rangedAttackCooldown = defaultE.rangedAttackCooldown;
            enemy.projectileSpeed = defaultE.projectileSpeed;
            enemy.spawnTime = defaultE.spawnTime;
            enemy.scale = defaultE.scale;
            enemy.coinChance = defaultE.coinChance;
            enemy.isBoss = defaultE.isBoss;
            enemy.isRanged = defaultE.isRanged;
            enemy.sprite = defaultE.sprite;
			enemy.drops = defaultE.drops;
        }
    }

    /// <summary>
    /// Reloads the cost of all items by loading in the default
    /// </summary>
    private void ReloadItemCosts()
    {
        foreach(Item item in Resources.LoadAll<Item>("Data/Items"))
        {
            item.cost = Resources.Load<Item>("Data/DefaultItems/" + item.name).cost;
        }
    }
}
