using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Object/Enemy")]

public class Enemy : MonoBehaviour
{
    public int health = 1;
    public float speed = 3f;
    public float spawnTime = 30f;
    public bool hasCoin = false;

    private void Start()
    {
        GetComponent<AIPath>().maxSpeed = speed;
    }

    public void Attacked()
    {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (health - player.bulletDamage > 0)
        {
            health -= player.bulletDamage;
            player.PlaySound(player.enemyHurtSFX);
        }
        else
        {
            player.gameObject.GetComponent<TimerManager>().timeLeft += gameObject.GetComponent<TimerManager>().timeLeft;
            if (hasCoin)
            {
                Instantiate(player.coin, transform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("RoomContainer").transform);
            }
            player.PlaySound(player.enemyDefeatSFX);
            Destroy(gameObject.GetComponent<TimerManager>().timerText.gameObject);
            Destroy(gameObject);
        }
    }
}