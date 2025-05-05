using System;
using System.Collections;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Modifier;

public class PlayerController : MonoBehaviour
{
    #region ScriptVariables
    public Vector2 playerMovement;

    public AudioClip attackMeleeSFX;
    public AudioClip coinCollectSFX;
    public AudioClip enemyHurtSFX;
    public AudioClip enemyDefeatSFX;

    public InputActionAsset playerInputActions;

    public GameObject bullet;
    public GameObject coin;

    private TMP_Text coinCounter;

    private Weapon equippedWeapon;

    private float lastShootTime = -1f;

    private InputAction moveAction;
    private InputAction shootAction;
    private InputAction equipAction;
    private GameObject emptyGameObject;

    private Rigidbody2D rb;
    #endregion

    public int score = 0;

    #region Modifiers
    private bool hasRangedWeapon = false;
    private bool weirdBullets = false;
    private bool bouncyBullets = false;
    private float speed = 5f;
    private float bulletSpeed;
    private float bulletSpawnOffset = 1f;
    private float shootCooldown;
    private float bulletDespawnTime;
    private float bonusTimeFromCoins;
    private float bulletArch = 15f;
    private float damageResistance = 1f;
    public float timeConsumeSpeed = 1f;
    public int bulletDamage;
    private int scoreFromCoins = 1;
    private int numberOfAttacks = 1;
    private Vector2 meleeRange;
    #endregion

    private void Awake()
    {
        emptyGameObject = new GameObject("Empty");
        emptyGameObject.AddComponent<Text>();
        rb = GetComponent<Rigidbody2D>();
        coinCounter = GameObject.FindGameObjectWithTag("CoinCounter").GetComponent<TMP_Text>();
        moveAction = playerInputActions.FindAction("Move");
        shootAction = playerInputActions.FindAction("Shoot");
        equipAction = playerInputActions.FindAction("Equip");
        equippedWeapon = new Weapon();
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
    }

    private void Start()
    {
        ChangeScore(0);
        UpdateWeaponStats(Resources.Load<Weapon>("Data/Weapons/Default"));
    }
        
    void Update()
    {
        playerMovement = moveAction.ReadValue<Vector2>();
        if (shootAction.triggered && Time.time >= lastShootTime + shootCooldown)
        {
            lastShootTime = Time.time;
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

            if (hasRangedWeapon)
            {
                float angleStep = numberOfAttacks > 1 ? bulletArch : 0f;
                float startAngle = -angleStep * (numberOfAttacks - 1) / 2;

                for (int i = 0; i < numberOfAttacks; i++)
                {
                    float currentAngle = startAngle + i * angleStep;
                    Vector2 bulletDirection = Quaternion.Euler(0, 0, currentAngle) * direction;
                    Vector2 spawnPosition = (Vector2)transform.position + bulletDirection * bulletSpawnOffset;
                    GameObject spawnedBullet = Instantiate(bullet, spawnPosition, Quaternion.identity);
                    spawnedBullet.GetComponent<BulletBehavior>().bouncyBullets = bouncyBullets;
                    Rigidbody2D bulletRb = spawnedBullet.GetComponent<Rigidbody2D>();
                    if (bulletRb != null)
                    {
                        if (weirdBullets)
                        {
                            StartCoroutine(MoveBulletInSinPattern(bulletRb, bulletDirection));
                        }
                        else
                        {
                            bulletRb.velocity = bulletDirection * bulletSpeed;
                        }
                    }
                    if (weirdBullets) StopCoroutine(MoveBulletInSinPattern(bulletRb, bulletDirection));
                    Destroy(spawnedBullet, bulletDespawnTime);
                }
            }
            else
            {
                Vector2 rectangleCenter = (Vector2)transform.position + direction * 1.5f;
                Vector2 rectangleSize = meleeRange;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Collider2D[] hitColliders = Physics2D.OverlapBoxAll(rectangleCenter, rectangleSize, angle);
                bool enemyFound = false;
                int currentAttack = 1;
                foreach (var collider in hitColliders)
                {
                    if (collider.CompareTag("Enemy"))
                    {
                        enemyFound = true;
                        collider.GetComponent<EnemyBehavior>().Attacked();
                        if (currentAttack == numberOfAttacks) break;
                        currentAttack++;
                    }
                }
                if (enemyFound)
                {
                    PlaySound(attackMeleeSFX);
                }
                GameObject weaponObject = Instantiate(emptyGameObject, (Vector2)transform.position + direction, Quaternion.LookRotation(Vector3.forward, direction), transform);
                SpriteRenderer spriteRenderer = weaponObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = equippedWeapon.weaponEquipped;
                float scaleFactor = (meleeRange.magnitude - 1) / meleeRange.magnitude;
                weaponObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
                Destroy(weaponObject, shootCooldown * 0.7f);
            }
        }
        if (equipAction.triggered)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Weapon"))
                {
                    UpdateWeaponStats(Resources.Load<Weapon>("Data/Weapons/" + collider.GetComponent<Text>().text));
                    Destroy(collider.gameObject);
                    break;
                }
            }
        }
    }

    private IEnumerator MoveBulletInSinPattern(Rigidbody2D bulletRb, Vector2 direction)
    {
        float time = 0f;
        while (bulletRb != null)
        {
            float sinOffset = Mathf.Sin(time * 10f) * 1.5f;
            Vector2 sinMovement = new Vector2(-direction.y, direction.x) * sinOffset;
            bulletRb.velocity = (direction * bulletSpeed) + sinMovement;
            time += Time.deltaTime;
            yield return null;
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + playerMovement * speed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var timerManager = gameObject.GetComponent<TimerManager>();
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyBehavior>();
            if (enemy.enemyStats.isBoss)
            {
                rb.AddForce(((Vector2)collision.transform.position - rb.position) * 3f, ForceMode2D.Impulse);
                if (score > 0)
                {
                    ChangeScore((int)((float)-enemy.enemyStats.health * enemy.enemyStats.damageMultiplier / damageResistance));
                }
                else
                {
                    timerManager.timeLeft -= 20f * enemy.enemyStats.damageMultiplier / damageResistance;
                }
                return;
            }
            if(score > 0)
            {
                ChangeScore((int)((float)-enemy.enemyStats.health * enemy.enemyStats.damageMultiplier / damageResistance));
            }
            else
            {
                timerManager.timeLeft -= Mathf.Max(collision.gameObject.GetComponent<TimerManager>().timeLeft, 20) * enemy.enemyStats.damageMultiplier / damageResistance;
            }
            Destroy(collision.gameObject.GetComponent<TimerManager>().timerText.gameObject);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Coin"))
        {
            timerManager.timeLeft += bonusTimeFromCoins;
            PlaySound(coinCollectSFX);
            ChangeScore(scoreFromCoins);
            Destroy(collision.gameObject);
        }
    }

    public void ChangeScore(int change)
    {
        score += change;
        coinCounter.text = "Coins:" + score.ToString();
    }

    //Use in places like Store to buff or nerf the player
    public void ChangeVariable(Modifier modifier)
    {
        if (modifier.modifiedVariable == "timeLeft")
        {
            var timerManager = gameObject.GetComponent<TimerManager>();
            switch (modifier.modifierType)
            {
                case ModifierType.Add:
                    timerManager.timeLeft += modifier.modifierValue;
                    break;
                case ModifierType.Subtract:
                    timerManager.timeLeft -= modifier.modifierValue;
                    break;
                case ModifierType.Multiply:
                    timerManager.timeLeft *= modifier.modifierValue;
                    break;
            }
            return;
        }

        var field = GetType().GetField(modifier.modifiedVariable, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null)
        {
            Debug.LogWarning($"Field '{modifier.modifiedVariable}' not found in {GetType()}.");
            return;
        }

        if (field.FieldType == typeof(float))
        {
            float currentValue = (float)field.GetValue(this);
            switch (modifier.modifierType)
            {
                case ModifierType.Add:
                    field.SetValue(this, currentValue + modifier.modifierValue);
                    break;
                case ModifierType.Subtract:
                    field.SetValue(this, currentValue - modifier.modifierValue);
                    break;
                case ModifierType.Multiply:
                    field.SetValue(this, currentValue * modifier.modifierValue);
                    break;
            }
        }
        else if (field.FieldType == typeof(int))
        {
            int currentValue = (int)field.GetValue(this);
            switch (modifier.modifierType)
            {
                case ModifierType.Add:
                    field.SetValue(this, currentValue + (int)modifier.modifierValue);
                    break;
                case ModifierType.Subtract:
                    field.SetValue(this, currentValue - (int)modifier.modifierValue);
                    break;
                case ModifierType.Multiply:
                    field.SetValue(this, currentValue * (int)modifier.modifierValue);
                    break;
            }
        }
        else if (field.FieldType == typeof(bool))
        {
            field.SetValue(this, (modifier.modifierValue == 0 ? false : true));
        }
        else
        {
            Debug.LogWarning($"Field '{modifier.modifiedVariable}' is of unsupported type '{field.FieldType}'.");
        }
    }

    //Play sound directly to the camera on the player
    public void PlaySound(AudioClip clip)
    {
        AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
        audioSource.PlayOneShot(clip);
    }

    //Execute to give player new weapon
    public void UpdateWeaponStats(Weapon newWeapon)
    {
        bulletDamage -= equippedWeapon.baseDamage;
        shootCooldown -= equippedWeapon.attackCooldown;
        bulletSpeed -= equippedWeapon.bulletTravelSpeed;
        bulletDespawnTime -= equippedWeapon.attackDespawnTime;
        numberOfAttacks -= equippedWeapon.numberOfAttacks;
        DropWeapon(equippedWeapon, gameObject.transform);
        equippedWeapon = newWeapon;
        bulletDamage += equippedWeapon.baseDamage;
        shootCooldown += equippedWeapon.attackCooldown;
        bulletSpeed += equippedWeapon.bulletTravelSpeed;
        bulletDespawnTime += equippedWeapon.attackDespawnTime;
        numberOfAttacks += equippedWeapon.numberOfAttacks;
        hasRangedWeapon = equippedWeapon.isRanged;
        meleeRange.x = equippedWeapon.meleeRangeX;
    }

    public void DropWeapon(Weapon weapon, Transform transform)
    {
        GameObject droppedWeapon = Instantiate(emptyGameObject, transform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("RoomContainer").transform);
        droppedWeapon.AddComponent<SpriteRenderer>().sprite = weapon.weaponDropped;
        droppedWeapon.AddComponent<BoxCollider2D>().isTrigger = true;
        droppedWeapon.tag = "Weapon";
        droppedWeapon.GetComponent<Text>().text = weapon.name;
        droppedWeapon.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
    }
}