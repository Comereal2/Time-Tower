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
    private GameObject healthBar;
    private Vector2 bossBarOffset = new(0f, 1f);

    private void Awake()
    {
        if (enemyStats.health > 1) healthBar = (GameObject)Resources.Load("Prefabs/HealthBar");
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
    }

    private void Update()
    {
        if (maxHealth > 1) healthBar.transform.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)bossBarOffset);
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
}
