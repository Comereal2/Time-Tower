using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Object/Weapon")]

public class Weapon : ScriptableObject
{
    public int baseDamage = 0;
    public int numberOfAttacks = 1;
    public float attackCooldown = 0;
    public float attackDespawnTime = 0;
    public float bulletTravelSpeed = 0;
    public float meleeRangeX = 1f;
    public bool isRanged = false;
    public Sprite weaponEquipped;
    public Sprite weaponDropped;
}