using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float bulletSpeed = 10f;
    public float bulletSpawnOffset = 1f;
    public float shootCooldown = 0.5f;
    public Vector2 playerMovement;

    public InputActionAsset playerInputActions;

    public GameObject bullet;

    private float lastShootTime = -1f;

    private InputAction moveAction;
    private InputAction shootAction;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
            gameObject.GetComponent<TimerManager>().timeLeft -= collision.gameObject.GetComponent<TimerManager>().timeLeft;
            Destroy(collision.gameObject);
        }
    }
}
