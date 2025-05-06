using System;
using System.Collections;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Modifier;

[RequireComponent(typeof(TimerManager))]
public class PlayerController : FightingController
{
    #region ScriptVariables
    public Vector2 playerMovement;

    public AudioClip attackMeleeSFX;
    public AudioClip attackRangedSFX;
    public AudioClip coinCollectSFX;
    public AudioClip enemyHurtSFX;
    public AudioClip enemyDefeatSFX;

    public GameObject bullet;
    public GameObject coin;

    private TMP_Text coinCounter;

    private Weapon equippedWeapon;

    private float lastShootTime = -1f;

    [SerializeField] private InputActionAsset playerInputActions;
    private InputAction moveAction;
    private InputAction shootAction;
    private InputAction equipAction;
    private InputAction dashAction;

    private Rigidbody2D rb;

    private Animator animator;
    #endregion

    public int score = 0;

    #region Modifiers
    private bool hasRangedWeapon = false;
    private bool weirdBullets = false;
    private bool bouncyBullets = false;
    private bool canTeleport = false;
    private float speed = 5f;
    private float bulletSpeed;
    private float shootCooldown;
    private float bulletDespawnTime;
    private float bonusTimeFromCoins;
    private float bulletArch = 15f;
    private float damageResistance = 1f;
    public float timeConsumeSpeed = 1f;
    public float costModifier = 1f;
    public int bulletDamage;
    private int scoreFromCoins = 1;
    private int numberOfAttacks = 1;
    private Vector2 meleeRange = new Vector2(0f, 1.5f);
    #endregion

    private bool isHardMode = false;

    private void Awake()
    {
        // I honestly hate the fact that I couldnt instantiate the emptyGameObject in the FightingController, but that Awake would always be overwritten by this one
        emptyGameObject = new GameObject("Empty");
        emptyGameObject.AddComponent<Text>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        // I tried making the player not rely on the coinCounter, but couldnt find a way. Welp, guess this is the one thing you have to add to the scene with him. Whoops
        coinCounter = GameObject.FindGameObjectWithTag("CoinCounter").GetComponent<TMP_Text>();
        moveAction = playerInputActions.FindAction("Move");
        shootAction = playerInputActions.FindAction("Shoot");
        equipAction = playerInputActions.FindAction("Equip");
        dashAction = playerInputActions.FindAction("Dash");
        equippedWeapon = new Weapon();
        isHardMode = PlayerPrefs.GetInt("HardMode", 0) == 1;
        if (isHardMode) gameObject.GetComponent<TimerManager>().canAutoConvertScoreToTime = false;
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
        ChangeScore(PlayerPrefs.GetInt("BonusCash", 0) == 1 ? 5 : 0);
        UpdateWeaponStats(Resources.Load<Weapon>("Data/Weapons/Default"));
    }
        
    private void Update()
    {
        playerMovement = moveAction.ReadValue<Vector2>();

        if(playerMovement != Vector2.zero)
        {
            animator.SetFloat("XInput", playerMovement.x);
            animator.SetFloat("YInput", playerMovement.y);
        }

        if (shootAction.triggered && Time.time >= lastShootTime + shootCooldown)
        {
            lastShootTime = Time.time;
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

            if (hasRangedWeapon)
            {
                // 1 attack is a special case where the formula wouldnt work, so we set the arch to 0
                float angleStep = numberOfAttacks > 1 ? bulletArch : 0f;
                float startAngle = -angleStep * (numberOfAttacks - 1) / 2;

                for (int i = 0; i < numberOfAttacks; i++)
                {
                    float currentAngle = startAngle + i * angleStep;
                    Vector2 bulletDirection = Quaternion.Euler(0, 0, currentAngle) * direction;

                    GameObject spawnedBullet = Shoot(bullet, (Vector2)transform.position + bulletDirection, bulletSpeed, bulletDespawnTime);
                    spawnedBullet.GetComponent<BulletBehavior>().bouncyBullets = bouncyBullets;
                    Rigidbody2D bulletRb = spawnedBullet.GetComponent<Rigidbody2D>();

                    if (weirdBullets)
                    {
                        StartCoroutine(MoveBulletInSinPattern(bulletRb, bulletDirection));
                    }
                }
                PlaySound(attackRangedSFX);
            }
            else
            {
                //Actual collider of the attack
                Vector2 rectangleCenter = (Vector2)transform.position + direction * (meleeRange.x+1f)/2f;
                Vector2 rectangleSize = meleeRange;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Collider2D[] hitColliders = Physics2D.OverlapBoxAll(rectangleCenter, rectangleSize, angle);
                bool enemyFound = false;
                int currentAttack = 1;
                foreach (var collider in hitColliders)
                {
                    if (collider.CompareTag("Enemy"))
                    {
                        //Attack is NOT AOE, it attacks at most numberOfAttacks enemies, but one enemy cannot be attacked twice
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

                //Graphics for the player, scalable based on range
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
                    // I would like to have a better way to do this, because having the sprite also have a text for this isnt great
                    UpdateWeaponStats(Resources.Load<Weapon>("Data/Weapons/" + collider.GetComponent<Text>().text));
                    Destroy(collider.gameObject);
                    break;
                }
            }
        }
        if (dashAction.triggered && canTeleport)
        {
            Teleport(speed, playerMovement.normalized);
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
            // Every enemy has an enemyBehavior
            var enemy = collision.gameObject.GetComponent<EnemyBehavior>();

            /* The damage system has two cases, because score is a shield for the player in case of melee attacks to make the game a bit more bearable
             * An enemy always takes at least 1 score, even with 100% damage resistance, enemies also have a damageMultiplier, which negates itself with the resistance
             * For any enemy which is not a boss, you always deal the amount of damage multiplied by health to increase the penalty on the player
             * If a player does not have score, you subtract time and the calculation is more brutal in that case, because it always takes as least the score to time conversion
             */

            var damage = Mathf.Max(enemy.enemyStats.damageMultiplier / damageResistance, 1);
            if (enemy.enemyStats.isBoss)
            {
                rb.velocity = (rb.position - (Vector2)collision.transform.position).normalized * 20f;
                if (score > damage && !isHardMode)
                {
                    ChangeScore((int)-damage);
                }
                else
                {
                    timerManager.timeLeft -= 20f * (damage - score);
                    ChangeScore(-score);
                }
                return;
            }
            if(score > damage && !isHardMode)
            {
                ChangeScore((int)((float)-enemy.enemyStats.health * damage));
            }
            else
            {
                timerManager.timeLeft -= Mathf.Max(collision.gameObject.GetComponent<TimerManager>().timeLeft, 20f) * (damage - score);
                ChangeScore(-score);
            }
            Destroy(collision.gameObject.GetComponent<TimerManager>().timerText.gameObject);
            Destroy(enemy.healthBar);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Coin"))
        {
            timerManager.timeLeft += bonusTimeFromCoins;
            PlaySound(coinCollectSFX);
            ChangeScore(scoreFromCoins);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            //This is the ranged damage, it is the only form which pierces the "score shield" to make the, already rare form, that extra bit more dangerous,
            //making it possible to die with score
            timerManager.timeLeft -= 20f;
            Destroy(collision.gameObject);
        }
    }

    /// <summary>
    /// Adds the change to the current score and updates the display
    /// </summary>
    /// <param name="change">Amount of score to add</param>
    public void ChangeScore(int change)
    {
        score += change;
        coinCounter.text = "Coins:" + score.ToString();
    }

    /// <summary>
    /// Applies the sent modifier to the player
    /// </summary>
    /// <param name="modifier"></param>
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

    /// <summary>
    /// Plays a one-time audio clip directly to the player
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySound(AudioClip clip)
    {
        //Checks if the current camera has an audio source and if not adds it
        AudioSource audioSource = Camera.main.GetComponent<AudioSource>() == null ? Camera.main.AddComponent<AudioSource>() : Camera.main.GetComponent<AudioSource>();
        audioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Removes the stats of the previous weapon, drops it and equips the new one
    /// </summary>
    /// <param name="newWeapon"></param>
    public void UpdateWeaponStats(Weapon newWeapon)
    {
        bulletDamage -= equippedWeapon.baseDamage;
        shootCooldown -= equippedWeapon.attackCooldown;
        bulletSpeed -= equippedWeapon.bulletTravelSpeed;
        bulletDespawnTime -= equippedWeapon.attackDespawnTime;
        numberOfAttacks -= equippedWeapon.numberOfAttacks;
        DropWeapon(equippedWeapon, gameObject.transform.position);
        equippedWeapon = newWeapon;
        bulletDamage += equippedWeapon.baseDamage;
        shootCooldown += equippedWeapon.attackCooldown;
        bulletSpeed += equippedWeapon.bulletTravelSpeed;
        bulletDespawnTime += equippedWeapon.attackDespawnTime;
        numberOfAttacks += equippedWeapon.numberOfAttacks;
        hasRangedWeapon = equippedWeapon.isRanged;
        meleeRange.x = equippedWeapon.meleeRangeX;
    }

    /// <summary>
    /// Used for weird bullets. Takes in the rigidbody and direction and changes the movement of the bullet according to the sin graph
    /// </summary>
    /// <param name="bulletRb">Rigidbody of the projectile</param>
    /// <param name="direction">Normalized direction towards the target point</param>
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
        StopCoroutine(MoveBulletInSinPattern(bulletRb, direction));
    }
}