using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class EnemyBehavior : MonoBehaviour
{
    public Enemy enemyStats;
    public Weapon equippedWeapon;

    private int maxHealth = 1;
    private float lastShootTime = -1f;
    private GameObject enemyProjectile;
    private GameObject healthBar;
    private Vector2 bossBarOffset = new(0f, 1f);

    private void Awake()
    {
        if (enemyStats.health > 1) healthBar = Resources.Load<GameObject>("Prefabs/HealthBar");
        if (enemyStats.isRanged) enemyProjectile = Resources.Load<GameObject>("Prefabs/EnemyBullet");
    }

    private void Start()
    {
        GetComponent<AIPath>().maxSpeed = enemyStats.speed;
        if (enemyStats.health > 1)
        {
            healthBar = Instantiate(healthBar, GameObject.FindGameObjectWithTag("TimerCanvas").transform);
            maxHealth = enemyStats.health;
            UpdateHealthBar();
        }
        if (enemyStats.sprite != null) transform.GetComponent<SpriteRenderer>().sprite = enemyStats.sprite;
        gameObject.transform.localScale = new Vector2(enemyStats.scale, enemyStats.scale);
    }

    private void Update()
    {
        if (maxHealth > 1) healthBar.transform.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)bossBarOffset);
        if (enemyStats.isRanged)
        {
            if (Time.time >= lastShootTime + enemyStats.rangedAttackCooldown)
            {
                lastShootTime = Time.time;
                Vector2 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
                Vector2 direction = (playerPosition - (Vector2)transform.position).normalized;

                GameObject bullet = Instantiate(enemyProjectile, (Vector2)transform.position + direction, Quaternion.identity);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = direction * 8f;
                }
            }
        }
    }

    //Execute when touched by attack
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
                Teleport();
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
            if(equippedWeapon != null) player.DropWeapon(equippedWeapon, gameObject.transform);
            Destroy(gameObject.GetComponent<TimerManager>().timerText.gameObject);
            Destroy(gameObject);
        }
    }

    private void UpdateHealthBar()
    {
        healthBar.GetComponent<Slider>().value = (float)enemyStats.health / (float)maxHealth;
    }

    private void Teleport()
    {
        float teleportRange = 3f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, teleportRange);
        List<Vector2> potentialPositions = new List<Vector2>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector2 playerPosition = player.transform.position;

        for (float x = -teleportRange; x <= teleportRange; x += 0.5f)
        {
            for (float y = -teleportRange; y <= teleportRange; y += 0.5f)
            {
                Vector2 potentialPosition = new Vector2(transform.position.x + x, transform.position.y + y);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, potentialPosition - (Vector2)transform.position, teleportRange);

                if (hit.collider == null || hit.collider.gameObject == gameObject)
                {
                    bool isBlocked = false;
                    foreach (var collider in colliders)
                    {
                        if (Physics2D.Linecast(transform.position, potentialPosition, LayerMask.GetMask("LevelObjects")))
                        {
                            isBlocked = true;
                            break;
                        }
                    }

                    if (!isBlocked)
                    {
                        potentialPositions.Add(potentialPosition);
                    }
                }
            }
        }

        if (potentialPositions.Count > 0)
        {
            Vector2 furthestPosition = potentialPositions[0];
            float maxDistance = Vector2.Distance(playerPosition, furthestPosition);

            foreach (var position in potentialPositions)
            {
                float distance = Vector2.Distance(playerPosition, position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthestPosition = position;
                }
            }

            // Ensure the teleport does not go through walls  
            RaycastHit2D finalHit = Physics2D.Raycast(transform.position, furthestPosition - (Vector2)transform.position, teleportRange, LayerMask.GetMask("Wall"));
            if (finalHit.collider == null)
            {
                transform.position = furthestPosition;
            }
        }
    }
}
