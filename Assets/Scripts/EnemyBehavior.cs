using Pathfinding;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TimerManager))]
public class EnemyBehavior : FightingController
{
    public Enemy enemyStats;
    public Weapon equippedWeapon;
    private int currentHealth = 1;
    private bool displayHealthBars = true;
    private GameObject enemyProjectile;
    public GameObject healthBar;
    private Vector2 bossBarOffset = new(0f, 1f); //This theoretically could just be a constant, but changing it for bosses with a larger scale should be possible

    private void Awake()
    {
        if (enemyStats.health > 1) healthBar = Resources.Load<GameObject>("Prefabs/HealthBar");
        if (enemyStats.isRanged) enemyProjectile = Resources.Load<GameObject>("Prefabs/EnemyBullet");
        // I honestly hate the fact that I couldnt instantiate the emptyGameObject in the FightingController, but that Awake would always be overwritten by this one
        emptyGameObject = new GameObject("Empty");
        emptyGameObject.AddComponent<Text>();
        displayHealthBars = PlayerPrefs.GetInt("EnemyHealthBars", 1) == 1;
    }

    private void Start()
    {
        // Pathfinding and movement is handled in the pathfinding asset entirely, so we need to refer to that when setting enemy speed
        // Every enemy should be able to pathfind, even stationary ones, if you want an enemy to not move, set their speed to 0
        GetComponent<AIPath>().maxSpeed = enemyStats.speed;
        if (enemyStats.health > 1 && displayHealthBars)
        {
            healthBar = Instantiate(healthBar, GameObject.FindGameObjectWithTag("TimerCanvas").transform);
            currentHealth = enemyStats.health;
            UpdateHealthBar();
        }
        if (enemyStats.sprite != null) transform.GetComponent<SpriteRenderer>().sprite = enemyStats.sprite;
        gameObject.transform.localScale = new Vector2(enemyStats.scale, enemyStats.scale);
        if (enemyStats.isRanged) InvokeRepeating("DetermineShot", 0, enemyStats.rangedAttackCooldown);
    }

    private void Update()
    {
        if (enemyStats.health > 1 && displayHealthBars) healthBar.transform.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)bossBarOffset);
    }

    /// <summary>
    /// Updates the enemy health bar
    /// </summary>
    private void UpdateHealthBar()
    {
        healthBar.GetComponent<Slider>().value = (float)currentHealth / (float)enemyStats.health;
    }

    /// <summary>
    /// Finds the player and shoots towards him
    /// </summary>
    private void DetermineShot()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        else
        {
            Shoot(enemyProjectile, player.transform.position, enemyStats.projectileSpeed, 3f); // Enemy projectiles shouldnt last too long, range can be controlled with speed
        }
    }

    /// <summary>
    /// Deals damage to the specific enemy
    /// </summary>
    public void Attacked()
    {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (currentHealth - player.bulletDamage > 0)
        {
            currentHealth -= player.bulletDamage;
            if (enemyStats.health > 1 && displayHealthBars) UpdateHealthBar();
            player.PlaySound(player.enemyHurtSFX);
            if(enemyStats.isBoss && enemyStats.isRanged)
            {
                //Made the teleport range for enemies constant to not make it too unbalanced
                Teleport(5f);
            }
        }
        else
        {
            player.gameObject.GetComponent<TimerManager>().timeLeft += gameObject.GetComponent<TimerManager>().timeLeft;
            if (enemyStats.hasCoin)
            {
                Instantiate(player.coin, transform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("RoomContainer").transform);
            }
            if (enemyStats.health > 1 && displayHealthBars) Destroy(healthBar);
            player.PlaySound(player.enemyDefeatSFX);
            if(equippedWeapon != null) DropWeapon(equippedWeapon, gameObject.transform.position);
            Destroy(gameObject.GetComponent<TimerManager>().timerText.gameObject);
            Destroy(gameObject);
        }
    }
}
