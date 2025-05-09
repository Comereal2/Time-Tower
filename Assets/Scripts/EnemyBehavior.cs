using Pathfinding;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TimerManager))]
public class EnemyBehavior : FightingController
{
    public Enemy enemyStats;
    public Weapon equippedWeapon;
    private int currentHealth = 1;
    private bool displayHealthBars = true;
    private bool hasCustomSprite = false;
    private GameObject enemyProjectile;
    private GameObject endOfLevelFlag;
    public GameObject healthBar;
    private Vector2 bossBarOffset = new(0f, 1f); //This theoretically could just be a constant, but changing it for bosses with a larger scale should be possible

    private void Awake()
    {
        if (enemyStats.health > 1) healthBar = Resources.Load<GameObject>("Prefabs/HealthBar");
        if (enemyStats.isRanged) enemyProjectile = Resources.Load<GameObject>("Prefabs/EnemyBullet");
        endOfLevelFlag = Resources.Load<GameObject>("Prefabs/Goal");
        displayHealthBars = PlayerPrefs.GetInt("EnemyHealthBars", 1) == 1;
        emptyGameObject = GameManager.empty;
    }

    private void Start()
    {
        if (enemyStats.health > 1 && displayHealthBars)
        {
            healthBar = Instantiate(healthBar, GameObject.FindGameObjectWithTag("TimerCanvas").transform);
            currentHealth = enemyStats.health;
            UpdateHealthBar();
        }
        if (enemyStats.sprite != null)
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = enemyStats.sprite;
            hasCustomSprite = true;
        }
        gameObject.transform.localScale = new Vector2(enemyStats.scale, enemyStats.scale);
        if (enemyStats.isRanged) InvokeRepeating(nameof(DetermineShot), 0, enemyStats.rangedAttackCooldown);
        if (enemyStats.isBoss) MusicManager.musicManager.ChangeMusic(MusicManager.musicManager.bossTheme);

        // Pathfinding and movement is handled in the pathfinding asset entirely, so we need to refer to that when setting enemy speed
        // Every enemy should be able to pathfind, even stationary ones, if you want an enemy to not move, set their speed to 0
        GetComponent<AIPath>().maxSpeed = enemyStats.speed;
    }

    private void Update()
    {
        if (enemyStats.health > 1 && displayHealthBars) healthBar.transform.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)bossBarOffset);
        if (hasCustomSprite)
        {
            transform.GetChild(0).rotation = Quaternion.identity;
            var aiPath = GetComponent<AIPath>();
            if (aiPath.desiredVelocity.x < 0)
                transform.GetChild(0).localScale = new Vector2(-1f, 1f) * enemyStats.scale;
            else if (aiPath.desiredVelocity.x > 0)
                transform.GetChild(0).localScale = new Vector2(1f, 1f) * enemyStats.scale;
        }
    }

    private void OnDestroy()
    {
        if (enemyStats.health > 1 && displayHealthBars) Destroy(healthBar);
        Destroy(gameObject.GetComponent<TimerManager>().timerText.gameObject);
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
        var player = PlayerController.playerController.gameObject;
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
        PlayerController player = PlayerController.playerController;
        if (currentHealth - player.bulletDamage > 0)
        {
            currentHealth -= player.bulletDamage;
            if (enemyStats.health > 1 && displayHealthBars) UpdateHealthBar();
            MusicManager.musicManager.PlaySound(player.enemyHurtSFX);
            if(enemyStats.isBoss && enemyStats.isRanged)
            {
                //Made the teleport range for enemies constant to not make it too unbalanced
                Teleport(5f);
            }
        }
        else
        {
            player.gameObject.GetComponent<TimerManager>().timeLeft += gameObject.GetComponent<TimerManager>().timeLeft * player.timeMultiplier * player.enemyTimeMultiplier * 0.4f; //Turns out it was too strong so we nerf it a lot
            if (UnityEngine.Random.value < enemyStats.coinChance)
            {
                Instantiate(player.coin, transform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("RoomContainer").transform);
            }
            if (enemyStats.isBoss)
            {
                MusicManager.musicManager.ChangeMusic(MusicManager.musicManager.dungeonTheme);
				GameObject.FindWithTag("DungeonGenerator").SendMessage("BossDefeated");
                Instantiate(endOfLevelFlag, transform.position, Quaternion.identity, transform.parent);
            }
            MusicManager.musicManager.PlaySound(player.enemyDefeatSFX);
            if (equippedWeapon != null) DropWeapon(equippedWeapon, gameObject.transform.position);
            Destroy(gameObject);
        }
    }
}
