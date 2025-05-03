using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float bulletSpeed = 10f;
    public float bulletSpawnOffset = 1f;
    public float shootCooldown = 0.5f;
    public int scoreFromCoins = 1;
    public Vector2 playerMovement;

    public int score = 0;

    public InputActionAsset playerInputActions;

    public GameObject bullet;

    private TMP_Text coinCounter;

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
        UpdateCoinCounter();
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
            GameObject spawnedBullet = Instantiate(bullet, spawnPosition, Quaternion.identity);
            Rigidbody2D bulletRb = spawnedBullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = direction * bulletSpeed;
            }
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
            score += scoreFromCoins;
            UpdateCoinCounter();
            Destroy(collision.gameObject);
        }
    }

    private void UpdateCoinCounter()
    {
        coinCounter.text = "Coins: " + score.ToString();
    }
}
