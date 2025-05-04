using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Object/Weapon")]

public class Weapon : ScriptableObject
{
    public int baseDamage = 0;
    public float attackCooldown = 0;
    public float attackDespawnTime = 0;
    public float bulletTravelSpeed = 0;
    public bool isRanged = false;
    public Vector2 meleeRange = new Vector2(1, 1);
    public Sprite weaponEquipped;
    public Sprite weaponDropped;
}