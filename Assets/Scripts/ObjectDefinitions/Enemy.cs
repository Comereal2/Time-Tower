using System.Collections;
using System.IO;
using UnityEditor;
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
    public float projectileSpeed = 5f;
    public float coinChance = .4f;
    public bool isBoss = false;
    public bool isRanged = false;
    public Sprite sprite;

    /// <summary>
    /// Upgrades the selected enemy statblock GLOBALLY
    /// </summary>
    public void UpgradeEnemy()
    {
        switch(Random.Range(1, 5))
        {
            case 1:
                health = Mathf.Max(health + 1, (int)((float)health * 1.2f));
                break;
            case 2:
                if (speed == 0)
                {
                    UpgradeEnemy();
                    break;
                }
                speed = Mathf.Max(speed + 2, speed * 1.2f);
                break;
            case 3:
                damageMultiplier += 0.5f;
                break;
            case 4:
                if (isRanged)
                {
                    rangedAttackCooldown *= 0.7f;
                    projectileSpeed *= 1.3f;
                }
                else
                {
                    health = Mathf.Max(health + 1, (int)((float)health * 1.2f));
                }
                break;
        }
    }
}
