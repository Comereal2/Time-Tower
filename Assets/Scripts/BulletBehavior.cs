using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public bool bouncyBullets;
    private Vector2 velocity;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        velocity = rb.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyBehavior>().Attacked();
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == 3)
        {
            if (bouncyBullets)
            {
                Vector2 normal = collision.GetContact(0).normal;
                rb.velocity = Vector2.Reflect(velocity * 1.3f, normal);
                velocity = rb.velocity;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}