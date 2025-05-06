using Pathfinding;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TimerManager))]
public class EnemyBehavior : FightingController
{
    public Enemy enemyStats;
    public Weapon equippedWeapon;

    private int maxHealth = 1;
    private GameObject enemyProjectile;
    private GameObject healthBar;
    private Vector2 bossBarOffset = new(0f, 1f); //This theoretically could just be a constant, but changing it for bosses with a larger scale should be possible

    private void Awake()
    {
        if (enemyStats.health > 1) healthBar = Resources.Load<GameObject>("Prefabs/HealthBar");
        if (enemyStats.isRanged) enemyProjectile = Resources.Load<GameObject>("Prefabs/EnemyBullet");
        // I honestly hate the fact that I couldnt instantiate the emptyGameObject in the FightingController, but that Awake would always be overwritten by this one
        emptyGameObject = new GameObject("Empty");
        emptyGameObject.AddComponent<Text>();
    }

    private void Start()
    {
        // Pathfinding and movement is handled in the pathfinding asset entirely, so we need to refer to that when setting enemy speed
        // Every enemy should be able to pathfind, even stationary ones, if you want an enemy to not move, set their speed to 0
        GetComponent<AIPath>().maxSpeed = enemyStats.speed;
        if (enemyStats.health > 1)
        {
            healthBar = Instantiate(healthBar, GameObject.FindGameObjectWithTag("TimerCanvas").transform);
            maxHealth = enemyStats.health;
            UpdateHealthBar();
        }
        if (enemyStats.sprite != null) transform.GetComponent<SpriteRenderer>().sprite = enemyStats.sprite;
        gameObject.transform.localScale = new Vector2(enemyStats.scale, enemyStats.scale);
        if (enemyStats.isRanged) InvokeRepeating("DetermineShot", 0, enemyStats.rangedAttackCooldown);
    }

    private void Update()
    {
        if (maxHealth > 1) healthBar.transform.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)bossBarOffset);
    }

    /// <summary>
    /// Updates the enemy health bar
    /// </summary>
    private void UpdateHealthBar()
    {
        healthBar.GetComponent<Slider>().value = (float)enemyStats.health / (float)maxHealth;
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
        if (enemyStats.health - player.bulletDamage > 0)
        {
            enemyStats.health -= player.bulletDamage;
            if (maxHealth > 1) UpdateHealthBar();
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
            if (maxHealth > 1) Destroy(healthBar);
            player.PlaySound(player.enemyDefeatSFX);
            if(equippedWeapon != null) DropWeapon(equippedWeapon, gameObject.transform.position);
            Destroy(gameObject.GetComponent<TimerManager>().timerText.gameObject);
            Destroy(gameObject);
        }
    }
}
