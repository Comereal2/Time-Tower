using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Object/Enemy")]

public class Enemy : ScriptableObject
{
    public int health = 1;
    public float damageMultiplier = 1f;
    public float speed = 3f;
    public float spawnTime = 30f;
    public float scale = 1f;
    public float rangedAttackCooldown = 1f;
    public bool hasCoin = false;
    public bool isBoss = false;
    public bool isRanged = false;
    public Sprite sprite;

    //Execute when floor ends
    public void UpgradeEnemy()
    {
        switch(Random.Range(1, 5))
        {
            case 1:
                health = Mathf.Max(health + 1, (int)((float)health * 1.2f));
                break;
            case 2:
                speed = Mathf.Max(speed + 2, speed * 1.2f);
                break;
            case 3:
                damageMultiplier += 0.5f;
                break;
            case 4:
                if (isRanged)
                {
                    rangedAttackCooldown *= 0.7f;
                }
                else
                {
                    health = Mathf.Max(health + 1, (int)((float)health * 1.2f));
                }
                break;
        }
    }
}