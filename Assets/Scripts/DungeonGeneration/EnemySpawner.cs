using UnityEngine;

namespace DungeonGeneration
{

[RequireComponent(typeof(TriggerHandler))]
public class EnemySpawner : MonoBehaviour
{
	public EnemyBehavior enemyPrefab;
	public EnemyList enemyList;
	public WeaponList enemyDroppables;
	// Weapon drops
	private CircleCollider2D trigger;
	
	void Start()
	{
		trigger = GetComponent<CircleCollider2D>();
		if (trigger == null)
		{
			Debug.LogWarning("trigger null");
		}
	}

	public void SwapSpawnerAndEnemy()
	{
		gameObject.SetActive(false);
		EnemyBehavior newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, gameObject.transform);
		newEnemy.enemyStats = enemyList.RandomEnemy();
		newEnemy.equippedWeapon = enemyDroppables.RandomWeapon();
		newEnemy.gameObject.transform.SetParent(null);
		Destroy(gameObject);
	}
};

} // namespace DungeonGeneration

