using System;
using System.Collections;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    public AudioClip playerHurtSFX;

    public GameObject bullet;
    public GameObject coin;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject weaponCompare;
    private Button pauseButton;

    private TMP_Text coinCounter;

    public Weapon equippedWeapon;

    private float lastShootTime = -1f;

    [SerializeField] private InputActionAsset playerInputActions;
    private InputAction moveAction;
    private InputAction shootAction;
    private InputAction equipAction;
    private InputAction dashAction;

    private Rigidbody2D rb;

    private Animator animator;
    #endregion

    public static PlayerController playerController;

    public int score = 0;

    private int maxScore = 0;

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
    public float timeMultiplier = 1f;
    public float enemyTimeMultiplier = 1f;
    private int scoreFromCoins = 1;
    private int numberOfAttacks = 1;
    private Vector2 meleeRange = new (0f, 1.5f);
    #endregion

    private bool isHardMode = false;

    private void Awake()
    {
        playerController = this;
        emptyGameObject = GameManager.empty;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        // I tried making the player not rely on the coinCounter, but couldnt find a way. Welp, guess this is the one thing you have to add to the scene with him. Whoops
        coinCounter = GameObject.FindGameObjectWithTag("CoinCounter").GetComponent<TMP_Text>();
        pauseButton = coinCounter.transform.parent.Find("PauseButton").GetComponent<Button>();
        pauseButton.onClick.AddListener(Pause);
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
        MusicManager.musicManager.ChangeMusic(MusicManager.musicManager.dungeonTheme);
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
                MusicManager.musicManager.PlaySound(attackRangedSFX);
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
                    MusicManager.musicManager.PlaySound(attackMeleeSFX);
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
        rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * playerMovement);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    private bool hasIFrames = false;

    private void OnCollisionStay2D(Collision2D collision)
    {
        var timerManager = gameObject.GetComponent<TimerManager>();
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Every enemy has an enemyBehavior
            var enemy = collision.gameObject.GetComponent<EnemyBehavior>();

            if (hasIFrames)
            {
                return;
            }

            hasIFrames = true;
            MusicManager.musicManager.PlaySound(playerHurtSFX);
            StartCoroutine(DisableIFrames());

            /* The damage system has two cases, because score is a shield for the player in case of melee attacks to make the game a bit more bearable
             * An enemy always takes at least 1 score, even with 100% damage resistance, enemies also have a damageMultiplier, which negates itself with the resistance
             * For any enemy which is not a boss, you always deal the amount of damage multiplied by health to increase the penalty on the player
             * If a player does not have score, you subtract time and the calculation is more brutal in that case, because it always takes as least the score to time conversion
             */

            var damage = Mathf.Max(enemy.enemyStats.damageMultiplier / damageResistance, 1);
            if (enemy.enemyStats.isBoss)
            {
                rb.velocity = (rb.position - (Vector2)collision.transform.position).normalized * 20f;
                timerManager.timeLeft -= 20f * damage;
                return;
            }
            timerManager.timeLeft -= Mathf.Max(collision.gameObject.GetComponent<TimerManager>().timeLeft, 20f) * damage * enemy.enemyStats.health;
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Coin"))
        {
            timerManager.timeLeft += bonusTimeFromCoins * timeMultiplier;
            MusicManager.musicManager.PlaySound(coinCollectSFX);
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

    private IEnumerator DisableIFrames()
    {
        yield return new WaitForSeconds(0.5f);
        hasIFrames = false;
    }

    /// <summary>
    /// Adds the change to the current score and updates the display
    /// </summary>
    /// <param name="change">Amount of score to add</param>
    public void ChangeScore(int change)
    {
        score += change;
        coinCounter.text = "Coins:" + score.ToString();
        if (change > 0) maxScore += change;
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
                    timerManager.timeLeft += modifier.modifierValue * timeMultiplier;
                    break;
                case ModifierType.Subtract:
                    timerManager.timeLeft -= modifier.modifierValue * timeMultiplier;
                    break;
                case ModifierType.Multiply:
                    timerManager.timeLeft *= modifier.modifierValue * timeMultiplier;
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
            field.SetValue(this, (modifier.modifierValue != 0));
        }
        else
        {
            Debug.LogWarning($"Field '{modifier.modifiedVariable}' is of unsupported type '{field.FieldType}'.");
        }
    }

    /// <summary>
    /// Removes the stats of the previous weapon, drops it and equips the new one
    /// </summary>
    /// <param name="newWeapon"></param>
    public void UpdateWeaponStats(Weapon newWeapon)
    {
        RemoveWeaponStats(equippedWeapon);
        DisplayWeaponCompare(equippedWeapon, newWeapon);
        DropWeapon(equippedWeapon, gameObject.transform.position);
        AddWeaponStats(newWeapon);
        equippedWeapon = newWeapon;
    }

    /// <summary>
    /// Removes the weapon's bonuses from the player
    /// </summary>
    /// <param name="weapon"></param>
    private void RemoveWeaponStats(Weapon weapon)
    {
        bulletDamage -= weapon.baseDamage;
        shootCooldown -= weapon.attackCooldown;
        bulletSpeed -= weapon.bulletTravelSpeed;
        bulletDespawnTime -= weapon.attackDespawnTime;
        numberOfAttacks -= weapon.numberOfAttacks;
    }

    /// <summary>
    /// Adds the weapon's bonuses to the player
    /// </summary>
    /// <param name="weapon"></param>
    private void AddWeaponStats(Weapon weapon)
    {
        bulletDamage += weapon.baseDamage;
        shootCooldown += weapon.attackCooldown;
        bulletSpeed += weapon.bulletTravelSpeed;
        bulletDespawnTime += weapon.attackDespawnTime;
        numberOfAttacks += weapon.numberOfAttacks;
        hasRangedWeapon = weapon.isRanged;
        meleeRange.x = weapon.meleeRangeX;
    }

    /// <summary>  
    /// Compares two weapons and displays the difference (counting modifiers)
    /// </summary>  
    /// <param name="oldWeapon"></param>  
    /// <param name="newWeapon"></param>  
    private void DisplayWeaponCompare(Weapon oldWeapon, Weapon newWeapon)
    {
        GameObject currentWeaponCompare = Instantiate(weaponCompare, coinCounter.transform.parent);
        string description = "<size=60><b>" + newWeapon.name + "</b></size><size=40>" + '\n';

        var comparisons = new (float oldValue, float newValue, string label, float currentValue)[]
        {
           (oldWeapon.baseDamage, newWeapon.baseDamage, "Base Damage", bulletDamage),
           (oldWeapon.attackCooldown, newWeapon.attackCooldown, "Attack Cooldown", shootCooldown),
           (oldWeapon.bulletTravelSpeed, newWeapon.bulletTravelSpeed, "Bullet Speed", bulletSpeed),
           (oldWeapon.attackDespawnTime, newWeapon.attackDespawnTime, "Bullet Despawn Time", bulletDespawnTime),
           (oldWeapon.numberOfAttacks, newWeapon.numberOfAttacks, "Number of Attacks", numberOfAttacks),
           (oldWeapon.meleeRangeX, newWeapon.meleeRangeX, "Melee Range X", meleeRange.x)
        };

        foreach (var (oldValue, newValue, label, currentValue) in comparisons)
        {
            string change = (newValue - oldValue) > 0 ? $"+{Mathf.Round((newValue - oldValue) * 10f) / 10f}" : $"{Mathf.Round((newValue - oldValue) * 10f) / 10f}";
            description += $"{label}: {(Mathf.Round(newValue * 10f) / 10f) + Mathf.Round(currentValue * 10f) / 10f} ({change}) \n";
        }

        description += $"Is Ranged: {newWeapon.isRanged}\n";
        description += "</size>";

        currentWeaponCompare.transform.GetChild(0).GetComponent<TMP_Text>().text = description;
        Destroy(currentWeaponCompare, 3f);
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

    /// <summary>
    /// Should be executed when the time runs out for the player
    /// </summary>
    public void Defeat() 
    {
        Time.timeScale = 0;
		int floorNumber = GameObject.FindWithTag("DungeonGenerator").GetComponent<DungeonGeneration.DungeonGenerator>().floorNumber;
        GameObject defeatScreen = Instantiate(weaponCompare, coinCounter.transform.parent);
        defeatScreen.transform.position = new Vector2(Screen.width/2, Screen.height/2);
        defeatScreen.transform.GetChild(0).GetComponent<TMP_Text>().text = $"<align=center><size=72>Score: {maxScore} \n </size><size=108><b>Defeat</b></size> \n <size=72>Floor: {floorNumber} \n CR: {PlayerPrefs.GetInt("ChallengeRating", 0)}</size></align>";
        StartCoroutine(SpawnButtons());
        pauseButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Only use this in Defeat()
    /// </summary>
    private IEnumerator SpawnButtons()
    {
        yield return new WaitForSecondsRealtime(2f);
        GameObject pauseMenu = Instantiate(menu, gameObject.transform);
        var quitButton = pauseMenu.transform.GetChild(1);
        var resumeButton = pauseMenu.transform.GetChild(0);
        quitButton.position += new Vector3(0, -50f, 0);
        quitButton.GetComponent<Button>().onClick.AddListener(ToMainMenu);
        resumeButton.position += new Vector3(0, 50f, 0);
        resumeButton.GetChild(0).GetComponent<TMP_Text>().text = "Restart";
        resumeButton.GetComponent<Button>().onClick.AddListener(() => {
            Time.timeScale = 1;
            SceneManager.LoadScene(1);
        });
    }

    /// <summary>
    /// Pauses the game
    /// </summary>
    private void Pause()
    {
        GameObject pauseMenu = Instantiate(menu, gameObject.transform);
        Time.timeScale = 0;
        pauseMenu.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => Resume(pauseMenu));
        pauseMenu.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(ToMainMenu);
        pauseButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Resumes the game
    /// </summary>
    private void Resume(GameObject pauseMenu)
    {
        pauseButton.onClick.AddListener(Pause);
        Time.timeScale = 1;
        Destroy(pauseMenu);
    }

    /// <summary>
    /// Loads 1st scene in build hierarchy
    /// </summary>
    private void ToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
