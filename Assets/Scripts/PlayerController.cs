using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Vector2 playerMovement;

    public AudioClip attackMeleeSFX;
    public AudioClip coinCollectSFX;
    public AudioClip enemyHurtSFX;
    public AudioClip enemyDefeatSFX;

    public int score = 0;

    public InputActionAsset playerInputActions;

    public GameObject bullet;
    public GameObject coin;

    private TMP_Text coinCounter;

    private Weapon equippedWeapon;

    private bool hasRangedWeapon = false;
    private float speed = 5f;
    private float bulletSpeed;
    private float bulletSpawnOffset;
    private float shootCooldown;
    private float bulletDespawnTime;
    public int bulletDamage { get; private set; }
    private int scoreFromCoins = 1;
    private Vector2 meleeRange;

    private float lastShootTime = -1f;

    private InputAction moveAction;
    private InputAction shootAction;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coinCounter = GameObject.FindGameObjectWithTag("CoinCounter").GetComponent<TMP_Text>();
        moveAction = playerInputActions.FindAction("Move");
        shootAction = playerInputActions.FindAction("Shoot");
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
            Vector2 spawnPosition = (Vector2)transform.position + direction * bulletSpawnOffset;
            if (hasRangedWeapon)
            {
                GameObject spawnedBullet = Instantiate(bullet, spawnPosition, Quaternion.identity);
                Rigidbody2D bulletRb = spawnedBullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = direction * bulletSpeed;
                }
                Destroy(spawnedBullet, bulletDespawnTime);
            }
            else
            {
                Vector2 rectangleCenter = (Vector2)transform.position + direction * 1.5f;
                Vector2 rectangleSize = meleeRange;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Collider2D[] hitColliders = Physics2D.OverlapBoxAll(rectangleCenter, rectangleSize, angle);
                bool enemyFound = false;
                foreach (var collider in hitColliders)
                {
                    if (collider.CompareTag("Enemy"))
                    {
                        enemyFound = true;
                        collider.GetComponent<Enemy>().Attacked();
                        break;
                    }
                }
                if (enemyFound)
                {
                    PlaySound(attackMeleeSFX);
                }
            }
            Sprite spawnedWeapon = Instantiate(equippedWeapon.weapon, spawnPosition, Quaternion.LookRotation(Vector3.forward, direction));
            Destroy(spawnedWeapon, 0.5f);
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + playerMovement * speed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if(score > 0)
            {
                score--;
            }
            else
            {
                gameObject.GetComponent<TimerManager>().timeLeft -= Mathf.Max(collision.gameObject.GetComponent<TimerManager>().timeLeft, 20);
            }
            Destroy(collision.gameObject.GetComponent<TimerManager>().timerText.gameObject);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Coin"))
        {
            PlaySound(coinCollectSFX);
            ChangeScore(scoreFromCoins);
            Destroy(collision.gameObject);
        }
    }

    public void ChangeScore(int change)
    {
        score += change;
        coinCounter.text = "Coins: " + score.ToString();
    }

    public void ChangeVariable(string name, float change)
    {
        if(name == "timeLeft")
        {
            gameObject.GetComponent<TimerManager>().timeLeft += change;
            return;
        }
        var field = GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
        {
            Debug.LogWarning($"Field '{name}' not found in {GetType().Name}.");
            return;
        }

        if (field.FieldType == typeof(float))
        {
            float currentValue = (float)field.GetValue(this);
            field.SetValue(this, currentValue + change);
        }
        else if (field.FieldType == typeof(int))
        {
            int currentValue = (int)field.GetValue(this);
            field.SetValue(this, currentValue + (int)change);
        }
        else
        {
            Debug.LogWarning($"Field '{name}' is of unsupported type '{field.FieldType}'.");
        }
    }

    public void PlaySound(AudioClip clip)
    {
        AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
        audioSource.PlayOneShot(clip);
    }

    public void UpdateWeaponStats(Weapon newWeapon)
    {
        bulletDamage -= equippedWeapon.baseDamage;
        shootCooldown -= equippedWeapon.attackCooldown;
        bulletSpeed -= equippedWeapon.bulletTravelSpeed;
        bulletDespawnTime -= equippedWeapon.attackDespawnTime;
        equippedWeapon = newWeapon;
        bulletDamage += equippedWeapon.baseDamage;
        shootCooldown += equippedWeapon.attackCooldown;
        bulletSpeed += equippedWeapon.bulletTravelSpeed;
        bulletDespawnTime += equippedWeapon.attackDespawnTime;
        hasRangedWeapon = equippedWeapon.isRanged;
        meleeRange = equippedWeapon.meleeRange;
    }
}