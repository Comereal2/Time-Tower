using System.Collections.Generic;
using UnityEngine;

public class FightingController : MonoBehaviour
{
    protected GameObject emptyGameObject;

    /// <summary>
    /// Teleports the object to a random location it can see
    /// </summary>
    /// <param name="range">Range of the teleport</param>
    protected void Teleport(float range)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range);
        List<Vector2> potentialPositions = new ();

        //Not required but makes teleports better
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector2 playerPosition = player.transform.position;

        for (float x = -range; x <= range; x += 0.5f)
        {
            for (float y = -range; y <= range; y += 0.5f)
            {
                Vector2 potentialPosition = new (transform.position.x + x, transform.position.y + y);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, potentialPosition - (Vector2)transform.position, range);

                if (hit.collider == null || hit.collider.gameObject == gameObject)
                {
                    bool isBlocked = false;
                    foreach (var collider in colliders)
                    {
                        if (Physics2D.Linecast(transform.position, potentialPosition, LayerMask.GetMask("LevelObjects")))
                        {
                            isBlocked = true;
                            break;
                        }
                    }

                    if (!isBlocked)
                    {
                        potentialPositions.Add(potentialPosition);
                    }
                }
            }
        }

        if (potentialPositions.Count > 0)
        {
            //Makes enemies run from player and player teleport max distance
            Vector2 furthestPosition = potentialPositions[0];
            
            if(player != null)
            {
                float maxDistance = Vector2.Distance(playerPosition, furthestPosition);

                foreach (var position in potentialPositions)
                {
                    float distance = Vector2.Distance(playerPosition, position);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        furthestPosition = position;
                    }
                }
            }
            transform.position = furthestPosition;
        }
    }

    /// <summary>  
    /// Teleports the object as far as possible in the given direction within the specified range.  
    /// </summary>  
    /// <param name="range">Range of the teleport</param>  
    /// <param name="direction">Direction of the teleport</param>  
    public void Teleport(float range, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range, LayerMask.GetMask("LevelObjects"));

        Vector2 targetPosition = hit.collider != null
            ? (Vector2)transform.position + direction * (hit.distance - 0.1f)
            : (Vector2)transform.position + direction * range;

        transform.position = targetPosition;
    }

    /// <summary>
    /// Makes the object shoot out a projectile towards a target with speed
    /// </summary>
    /// <param name="projectile">Prefab or definition of the created projectile</param>
    /// <param name="targetPosition">Position of the target or any point in a ray shot from the current position to the target(not relative)</param>
    /// <param name="speed">Speed of the projectile</param>
    /// <returns>
    /// GameObject reference to the projectile sent
    /// </returns>
    protected GameObject Shoot(GameObject projectile, Vector2 targetPosition, float speed, float despawnTime)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        projectile = Instantiate(projectile, (Vector2)transform.position + direction, Quaternion.identity);
        bool projectileExists = projectile.TryGetComponent(out Rigidbody2D projectileRb);
        if (projectileExists)
        {
            projectileRb.velocity = direction * speed;
        }
        else
        {
            projectile.AddComponent<Rigidbody2D>().gravityScale = 0;
            projectileRb = projectile.GetComponent<Rigidbody2D>();
            projectileRb.velocity = direction * speed;
        }
        Destroy(projectile, despawnTime);
        return projectile;
    }

    /// <summary>
    /// Drops the weapon on the ground at position
    /// </summary>
    /// <param name="weapon">Dropped weapon</param>
    /// <param name="position">Global position of where it should drop</param>
    protected void DropWeapon(Weapon weapon, Vector2 position)
    {
        var container = GameObject.FindGameObjectWithTag("RoomContainer");
        GameObject droppedWeapon = Instantiate(emptyGameObject, position, Quaternion.identity, container != null ? container.transform : transform.root);
        droppedWeapon.AddComponent<SpriteRenderer>().sprite = weapon.weaponDropped;
        droppedWeapon.AddComponent<BoxCollider2D>().isTrigger = true;
        droppedWeapon.tag = "Weapon";
        droppedWeapon.GetComponent<UnityEngine.UI.Text>().text = weapon.name; //Text is only used to store the weapon name
        droppedWeapon.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
    }
}