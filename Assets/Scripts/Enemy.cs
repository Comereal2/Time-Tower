using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public int health = 1;
    public float speed = 3f;
    public float spawnTime = 30f;
    public bool hasCoin = false;
    public bool isBoss = false;
    public Weapon equippedWeapon;

    private int maxHealth = 1;
    private GameObject healthBar;
    private Vector2 bossBarOffset = new Vector2(0f, 1f);

    private void Awake()
    {
        if(health > 1) healthBar = (GameObject)Resources.Load("Prefabs/HealthBar");
    }

    private void Start()
    {
        GetComponent<AIPath>().maxSpeed = speed;
        if(health > 1)
        {
            healthBar = Instantiate(healthBar, GameObject.FindGameObjectWithTag("TimerCanvas").transform);
            maxHealth = health;
            UpdateHealthBar();
        }
    }

    private void Update()
    {
        if(maxHealth > 1) healthBar.transform.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)bossBarOffset);
    }

    public void Attacked()
    {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (health - player.bulletDamage > 0)
        {
            health -= player.bulletDamage;
            if(maxHealth > 1) UpdateHealthBar();
            player.PlaySound(player.enemyHurtSFX);
        }
        else
        {
            player.gameObject.GetComponent<TimerManager>().timeLeft += gameObject.GetComponent<TimerManager>().timeLeft;
            if (hasCoin)
            {
                Instantiate(player.coin, transform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("RoomContainer").transform);
            }
            if(maxHealth > 1) Destroy(healthBar);
            player.PlaySound(player.enemyDefeatSFX);
            player.DropWeapon(equippedWeapon, gameObject.transform);
            Destroy(gameObject.GetComponent<TimerManager>().timerText.gameObject);
            Destroy(gameObject);
        }
    }

    private void UpdateHealthBar()
    {
        healthBar.GetComponent<Slider>().value = (float)health / (float)maxHealth;
    }
}