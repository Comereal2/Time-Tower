using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidesWithWall : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            Destroy(gameObject);
        }
    }
}
